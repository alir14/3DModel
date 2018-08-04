using System;
using SharpDX;
using IfcEngineCS;
using _3DModel.IFCFileReader;
using HelixToolkit.Wpf.SharpDX;
using System.Runtime.InteropServices;

namespace _3DModel.Managers
{
    public class ModelManager
    {
        float[] minCorner = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
        float[] maxCorner = new float[3] { float.MinValue, float.MinValue, float.MinValue };

        IFCItem HoverIfcItem { get; set; }
        IFCItem SelectedIfcItem { get; set; }
        readonly IfcEngine ifcEngine = new IfcEngine();

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

        public void LoadModel(string filePath)
        {
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
                this.IfcObject.RootItem = null;

            HoverIfcItem = null;

            SelectedIfcItem = null;
        }

        public void CreateMeshes(Vector3 center)
        {
            IfcObject.CreateFaceModelsRecursive_WPFtyle(this.IfcObject.RootItem, center);
        }

        public void CreateWireFrames(Vector3 center)
        {
            IfcObject.CreateWireFrameModelsRecursive_WPFStyle(this.IfcObject.RootItem, center); 
        }

        private IFCType GetIfcType(string path)
        {
            IFCType result = IFCType.None;

            IntPtr ifcModel = ifcEngine.OpenModel(IntPtr.Zero, path, Constants.IFC2X3_SCHEMA_NAME);

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
