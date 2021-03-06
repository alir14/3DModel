﻿using System;
using SharpDX;
using IfcEngineCS;
using _3DModel.IFC;
using _3DModel.ViewModel;
using _3DModel.IFCFileReader;
using System.Windows.Controls;
using HelixToolkit.Wpf.SharpDX;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;

namespace _3DModel.Managers
{
    public class ModelManager
    {
        float[] minCorner = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
        float[] maxCorner = new float[3] { float.MinValue, float.MinValue, float.MinValue };
        bool makeModelCentered = true;
        readonly IfcEngine ifcEngine = new IfcEngine();
        MainViewModel viewModel = new MainViewModel();
        TreeView treeViewControl;
        HelixToolkit.Wpf.SharpDX.Material hoverMaterial = PhongMaterials.Violet;
        HelixToolkit.Wpf.SharpDX.Material selectMaterial = PhongMaterials.Chrome;

        IFCItem HoverIfcItem { get; set; }
        Vector3 Max
        {
            get { return new Vector3(maxCorner[0], maxCorner[1], maxCorner[2]) - Center; }
        }
        Vector3 Min
        {
            get { return new Vector3(minCorner[0], minCorner[1], minCorner[2]) - Center; }
        }
        Vector3 Center
        {
            get
            { return makeModelCentered ? Vector3.Zero : new Vector3(minCorner[0] + maxCorner[0], minCorner[1] + maxCorner[1], minCorner[2] + maxCorner[2]) * 0.5f; }
        }

        public IFCItem SelectedIfcItem { get; set; }
        public IfcEngine IFCEngine
        {
            get
            {
                return ifcEngine;
            }
        }
        public BaseIFCFileReader IfcObject { get; set; }
        public float[] MinCorner
        {
            get { return minCorner; }
            set { minCorner = value; }
        }
        public float[] MaxCorner
        {
            get { return maxCorner; }
            set { maxCorner = value; }
        }

        public MainViewModel ViewModel
        {
            get { return viewModel; }
            set { viewModel = value; }
        }
        public string ModelName { get; set; }

        #region singltone

        private static ModelManager instance = null;
        private static readonly object processLock = new object();

        public static ModelManager Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(processLock)
                    {
                        if (instance == null)
                        {
                            instance = new ModelManager();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        public void LoadModel(string filePath, TreeView treeView)
        {
            this.treeViewControl = treeView;

            var type = GetIfcType(filePath);

            switch (type)
            {
                case IFCType.IFC2:
                    IfcObject = new IFC2FileReader(filePath);
                    IfcObject.ParsIFCFile();
                    break;
                case IFCType.IFC4:
                    IfcObject = new IFC4FileReader(filePath);
                    IfcObject.ParsIFCFile();
                    break;
                default:
                    break;
            }
        }

        public void ResetModel()
        {
            if(this.IfcObject != null)
            {
                this.IfcObject.RootItem = null;
                this.IfcObject.MeshToIfcItems = new Dictionary<MeshGeometryModel3D, IFCItem>();
            }

            viewModel.Model = new Element3DCollection();

            HoverIfcItem = null;
            SelectedIfcItem = null;
            minCorner = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
            maxCorner = new float[3] { float.MinValue, float.MinValue, float.MinValue };
        }

        public void InitModel()
        {
            Vector3 center = new Vector3(
                (MinCorner[0] + MaxCorner[0]) / 2,
                (MinCorner[1] + MaxCorner[1]) / 2,
                (MinCorner[2] + MaxCorner[2]) / 2);

            CreateMeshes(center);
            CreateWireFrames(center);

            this.viewModel.Model = IfcObject.Model;
        }

        public void ZoomExtent(Viewport3DX viewport, double animationTime = 200)
        {
            var center = new Point3D(Center.X, Center.Y, Center.Z);
            var radius = (Max - Min).Length() * 0.5;
            var camera = this.viewModel.Camera;

            var perspectiveCam = camera as HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
            if (perspectiveCam != null)
            {
                perspectiveCam.FarPlaneDistance = radius * 100;
                perspectiveCam.NearPlaneDistance = radius * 0.02;

                double disth = radius / Math.Tan(0.5 * perspectiveCam.FieldOfView * Math.PI / 180);
                double vfov = perspectiveCam.FieldOfView / viewport.ActualWidth * viewport.ActualHeight;
                double distv = radius / Math.Tan(0.5 * vfov * Math.PI / 180);

                double dist = Math.Max(disth, distv);
                var dir = perspectiveCam.LookDirection;
                dir.Normalize();
                perspectiveCam.LookAt(center, dir * dist, animationTime);
            }

            var oethographicCam = camera as HelixToolkit.Wpf.SharpDX.OrthographicCamera;
            if (oethographicCam != null)
            {
                oethographicCam.LookAt(center, oethographicCam.LookDirection, animationTime);
                double newWidth = radius * 2;

                if (viewport.ActualWidth > viewport.ActualHeight)
                {
                    newWidth = radius * 2 * viewport.ActualWidth / viewport.ActualHeight;
                }

                oethographicCam.AnimateWidth(newWidth, animationTime);
            }
        }

        public void CloseCurrentModel(IntPtr model)
        {
            this.ifcEngine.CloseModel(model);
        }

        public void BuildTree(IntPtr model, IFCItem item)
        {
            var treeData = new IFCTreeData(this.ifcEngine, model, item, this.treeViewControl);

            treeData.BuildTree();

            this.ModelName = treeData.ModelName;
        }

        public void OnModelHovered(HelixToolkit.Wpf.SharpDX.Model3D model)
        {
            if (this.HoverIfcItem != null)
            {
                HoverIfcItem.Mesh3d.Material = PhongMaterials.Silver;
                HoverIfcItem = null;
            }
            if (model != null)
            {
                var mesh = (model as MeshGeometryModel3D);
                if (mesh != null && this.IfcObject.MeshToIfcItems.ContainsKey(mesh))
                {
                    mesh.Material = hoverMaterial;
                    HoverIfcItem = this.IfcObject.MeshToIfcItems[mesh];
                }
            }
        }

        public void OnModelSelected(HelixToolkit.Wpf.SharpDX.Model3D model)
        {
            if (SelectedIfcItem != null)
            {
                SelectedIfcItem.ifcTreeItem.treeNode.IsSelected = false;
                SelectedIfcItem.Mesh3d.Material = PhongMaterials.Bronze;
                SelectedIfcItem = null;
            }

            if (model != null)
            {
                var mesh = (model as MeshGeometryModel3D);
                if (mesh != null && this.IfcObject.MeshToIfcItems.ContainsKey(mesh))
                {
                    mesh.Material = selectMaterial;
                    SelectedIfcItem = this.IfcObject.MeshToIfcItems[mesh];
                    SelectedIfcItem.ifcTreeItem.treeNode.IsSelected = true;
                    SelectedIfcItem.ifcTreeItem.treeNode.Focus();
                }
            }
        }

        private void CreateMeshes(Vector3 center)
        {
            IfcObject.CreateFaceModelsRecursive(this.IfcObject.RootItem, center);
        }

        private void CreateWireFrames(Vector3 center)
        {
            IfcObject.CreateWireFrameModelsRecursive(this.IfcObject.RootItem, center); 
        }

        private IFCType GetIfcType(string path)
        {
            IFCType result = IFCType.None;

            IntPtr ifcModel = ifcEngine.OpenModelUnicode(IntPtr.Zero, path, Constants.IFC2X3_SCHEMA_NAME);

            if (ifcModel != IntPtr.Zero)
            {
                ifcEngine.GetSPFFHeaderItem(ifcModel, 9, 0, IfcEngine.SdaiType.String, out IntPtr outputValue);

                string headerItem = Marshal.PtrToStringAnsi(outputValue);

                if (!string.IsNullOrEmpty(headerItem))
                {
                    if (headerItem.Contains(IFCType.IFC2.ToString()))
                    {
                        result = IFCType.IFC2;
                    }
                    else if (headerItem.Contains(IFCType.IFC4.ToString()))
                    {
                        result = IFCType.IFC4;
                    }
                }

                ifcEngine.CloseModel(ifcModel);
            }

            return result;
        }

    }
}
