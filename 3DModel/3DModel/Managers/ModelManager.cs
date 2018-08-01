using System;
using SharpDX;
using IfcEngineCS;
using _3DModel.IFCFileReader;
using HelixToolkit.Wpf.SharpDX;
using System.Collections.Generic;
using HelixToolkit.Wpf.SharpDX.Core;
using System.Runtime.InteropServices;

namespace _3DModel.Managers
{
    public class ModelManager
    {
        readonly IfcEngine ifcEngine = new IfcEngine();
        BaseIFCFileReader IfcObject { get; set; }
        IFCItem HoverIfcItem { get; set; }
        IFCItem SelectedIfcItem { get; set; }
        Dictionary<MeshGeometryModel3D, IFCItem> MeshToIfcItems = new Dictionary<MeshGeometryModel3D, IFCItem>();


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
                    IfcObject = new IFC2FileReader(ifcEngine, filePath);
                    IfcObject.ParsIFCFile();
                    break;
                case IFCType.IFC4:
                    IfcObject = new IFC4FileReader(ifcEngine, filePath);
                    IfcObject.ParsIFCFile();
                    break;
                default:
                    break;
            }
        }

        public void ResetModel()
        {
            this.IfcObject.RootItem = null;
            HoverIfcItem = null;
            SelectedIfcItem = null;
        }

        public void CreateMeshes(Vector3 center)
        {
            var item = this.IfcObject.RootItem;

            while (item != null)
            {
                if (item.ifcID != IntPtr.Zero && item.noVerticesForFaces != 0 && item.noPrimitivesForFaces != 0)
                {
                    var positions = new Vector3Collection();
                    var normals = new Vector3Collection();

                    if(item.verticesForFaces != null)
                    {
                        for (int i = 0; i < item.noVerticesForFaces; i++)
                        {
                            positions.Add( new Vector3(item.verticesForFaces[6 * i + 0] - center.X,
                                item.verticesForFaces[6 * i + 1] - center.Y, item.verticesForFaces[6 * i + 2] - center.Z));

                            normals.Add( new Vector3(item.verticesForFaces[6 * i + 3], item.verticesForFaces[6 * i + 4], item.verticesForFaces[6 * i + 5]));
                        }
                    }

                    var indices = new IntCollection();
                    if (item.indicesForFaces != null)
                    {
                        for (int i = 0; i < 3 * item.noPrimitivesForFaces; i++)
                        {
                            indices.Add(item.indicesForFaces[i]);
                        }
                    }


                    var meshGeometry = new MeshGeometry3D
                    {
                        Positions = positions,
                        Normals = normals,
                        Indices = indices,
                        TextureCoordinates = null,
                        Colors = null,
                        Tangents = null,
                        BiTangents = null
                    };

                    MeshGeometryModel3D mesh = new MeshGeometryModel3D() { Geometry = meshGeometry };

                    item.Mesh3d = mesh;

                    MeshToIfcItems[mesh] = item;

                    mesh.Tag = item.ifcType + ":" + item.ifcID;



                }
            }
        }

        public void CreateWireFrames()
        {

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
