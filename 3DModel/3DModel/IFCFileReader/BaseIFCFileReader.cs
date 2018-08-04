using System;
using SharpDX;
using System.Xml;
using IfcEngineCS;
using _3DModel.IFC;
using HelixToolkit.Wpf;
using _3DModel.Managers;
using HelixToolkit.Wpf.SharpDX;
using System.Collections.Generic;
using HelixToolkit.Wpf.SharpDX.Core;
using System.Runtime.InteropServices;

namespace _3DModel.IFCFileReader
{
    public abstract class BaseIFCFileReader
    {
        public IFCItem RootItem;

        protected string Path { get; set; }
        protected IntPtr IfcModel { get; set; }

        IntPtr IfcObjectInstances, NumberIfcObjectInstance;
        Material DefaultMaterial = PhongMaterials.Bronze;
        System.Windows.Media.Color DefaultLineColor = System.Windows.Media.Color.FromRgb(0, 0, 0);
        //List<MeshGeometryVisual3D> modelElementCollection = new List<MeshGeometryVisual3D>();
        //List<LinesVisual3D> modelLineCollection = new List<LinesVisual3D>();
        Element3DCollection model = new Element3DCollection();
        Dictionary<MeshGeometryModel3D, IFCItem> MeshToIfcItems = new Dictionary<MeshGeometryModel3D, IFCItem>();

        //public List<LinesVisual3D> ModelLineCollection
        //{
        //    get
        //    {
        //        return modelLineCollection;
        //    }
        //}
        //public List<MeshGeometryVisual3D> ModelElementCollection
        //{
        //    get
        //    {
        //        return modelElementCollection;
        //    }
        //}
        public Element3DCollection Model
        {
            get
            {
                return model;
            }
        }

        public BaseIFCFileReader(string path)
        {
            this.Path = path;
        }

        // should override
        public virtual void ParsIFCFile()
        {
        }

        public void CreateFaceModelsRecursive_WinFrmStyle(IFCItem item, Vector3 center)
        {
            //while (item != null)
            //{
            //    if (item.ifcID != IntPtr.Zero && item.noVerticesForFaces != 0 && item.noPrimitivesForFaces != 0)
            //    {
            //        var positions = new System.Windows.Media.Media3D.Point3DCollection();
            //        var normals = new System.Windows.Media.Media3D.Vector3DCollection();

            //        if (item.verticesForFaces != null)
            //        {
            //            for (int i = 0; i < item.noVerticesForFaces; i++)
            //            {
            //                positions.Add(new System.Windows.Media.Media3D.Point3D(item.verticesForFaces[6 * i + 0] - center.X,
            //                    item.verticesForFaces[6 * i + 1] - center.Y, item.verticesForFaces[6 * i + 2] - center.Z));

            //                normals.Add(new System.Windows.Media.Media3D.Vector3D(item.verticesForFaces[6 * i + 3],
            //                    item.verticesForFaces[6 * i + 4], item.verticesForFaces[6 * i + 5]));
            //            }
            //        }

            //        var indices = new System.Windows.Media.Int32Collection();
            //        if (item.indicesForFaces != null)
            //        {
            //            for (int i = 0; i < 3 * item.noPrimitivesForFaces; i++)
            //            {
            //                indices.Add(item.indicesForFaces[i]);
            //            }
            //        }

            //        var meshGeometry = new System.Windows.Media.Media3D.MeshGeometry3D
            //        {
            //            Positions = positions,
            //            Normals = normals,
            //            TriangleIndices = indices
            //        };

            //        var mesh = new HelixToolkit.Wpf.MeshGeometryVisual3D() { MeshGeometry = meshGeometry };

            //        item.Mesh3d = mesh;

            //        //MeshToIfcItems[mesh] = item;

            //        FillMeshByIfcColor_WinForm(item);

            //        modelElementCollection.Add(mesh);
            //    }

            //    CreateFaceModelsRecursive_WinFrmStyle(item.child, center);

            //    item = item.next;
            //}
        }

        public void CreateFaceModelsRecursive_WPFtyle(IFCItem item, Vector3 center)
        {
            while (item != null)
            {
                if (item.ifcID != IntPtr.Zero && item.noVerticesForFaces != 0 && item.noPrimitivesForFaces != 0)
                {
                    var positions = new Vector3Collection();
                    var normals = new Vector3Collection();
                    if (item.verticesForFaces != null)
                    {
                        for (int i = 0; i < item.noVerticesForFaces; i++)
                        {
                            positions.Add(new Vector3(
                                item.verticesForFaces[6 * i + 0] - center.X,
                                item.verticesForFaces[6 * i + 1] - center.Y,
                                item.verticesForFaces[6 * i + 2] - center.Z));

                            normals.Add(new Vector3(item.verticesForFaces[6 * i + 3],
                                item.verticesForFaces[6 * i + 4],
                                item.verticesForFaces[6 * i + 5]));
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

                    var meshGeometry = new MeshGeometry3D();
                    meshGeometry.Positions = positions;
                    meshGeometry.Normals = normals;
                    meshGeometry.Indices = indices;
                    meshGeometry.TextureCoordinates = null;
                    meshGeometry.Colors = null;
                    meshGeometry.Tangents = null;
                    meshGeometry.BiTangents = null;

                    MeshGeometryModel3D mesh = new MeshGeometryModel3D() { Geometry = meshGeometry };
                    item.Mesh3d = mesh;
                    MeshToIfcItems[mesh] = item;

                    FillMeshByIfcColor_WPF(item);

                    mesh.Tag = item.ifcType + ":" + item.ifcID;
                    model.Add(mesh);
                }

                CreateFaceModelsRecursive_WPFtyle(item.child, center);

                item = item.next;
            }
        }

        public void CreateWireFrameModelsRecursive_WinFrmStyle(IFCItem item, Vector3 center)
        {
            //while (item != null)
            //{
            //    if (item.ifcID != IntPtr.Zero && item.noVerticesForWireFrame != 0 && item.noPrimitivesForWireFrame != 0)
            //    {
            //        var points = new System.Windows.Media.Media3D.Point3DCollection();
            //        System.Windows.Media.Media3D.Point3DCollection positions;

            //        if (item.verticesForWireFrame != null)
            //        {
            //            for (int i = 0; i < item.noVerticesForWireFrame; i++)
            //            {
            //                points.Add(new System.Windows.Media.Media3D.Point3D(
            //                    (item.verticesForWireFrame[3 * i + 0] - center.X),
            //                    (item.verticesForWireFrame[3 * i + 1] - center.Y),
            //                    (item.verticesForWireFrame[3 * i + 2] - center.Z)));
            //            }
            //        }

            //        if (item.indicesForWireFrameLineParts != null)
            //        {
            //            positions = new System.Windows.Media.Media3D.Point3DCollection();
            //            for (int i = 0; i < item.noPrimitivesForWireFrame; i++)
            //            {
            //                var idx = item.indicesForWireFrameLineParts[2 * i + 0];
            //                positions.Add(points[idx]);
            //                idx = item.indicesForWireFrameLineParts[2 * i + 1];
            //                positions.Add(points[idx]);
            //            }
            //        }
            //        else
            //        {
            //            positions = points;
            //        }

            //        var wireframe = new HelixToolkit.Wpf.LinesVisual3D()
            //        {
            //            Points = positions,
            //            Color = DefaultLineColor
            //        };
            //        item.Wireframe = wireframe;

            //        modelLineCollection.Add(wireframe);
            //    }

            //    CreateWireFrameModelsRecursive_WinFrmStyle(item.child, center);

            //    item = item.next;
            //}
        }

        public void CreateWireFrameModelsRecursive_WPFStyle(IFCItem item, Vector3 center)
        {
            while (item != null)
            {
                if (item.ifcID != IntPtr.Zero && item.noVerticesForWireFrame != 0 && item.noPrimitivesForWireFrame != 0)
                {
                    var geo = new LineGeometry3D()
                    {
                        Positions = new Vector3Collection(),
                        Indices = new IntCollection()
                    };

                    var points = new Vector3Collection();

                    if (item.verticesForWireFrame != null)
                    {
                        for (int i = 0; i < item.noVerticesForWireFrame; i++)
                        {
                            points.Add(new Vector3(
                                (item.verticesForWireFrame[3 * i + 0] - center.X), 
                                (item.verticesForWireFrame[3 * i + 1] - center.Y), 
                                (item.verticesForWireFrame[3 * i + 2] - center.Z)));
                            geo.Positions.Add(new Vector3(
                                (item.verticesForWireFrame[3 * i + 0] - center.X), 
                                (item.verticesForWireFrame[3 * i + 1] - center.Y), 
                                (item.verticesForWireFrame[3 * i + 2] - center.Z)));
                        }
                    }

                    if (item.indicesForWireFrameLineParts != null)
                    {
                        for (int i = 0; i < item.noPrimitivesForWireFrame; i++)
                        {
                            var idx = item.indicesForWireFrameLineParts[2 * i + 0];
                            geo.Indices.Add(idx);
                            idx = item.indicesForWireFrameLineParts[2 * i + 1];
                            geo.Indices.Add(idx);
                        }
                    }
                    else
                    {
                        for (int i = 0, count = points.Count; i < count; i++)
                        {
                            geo.Indices.Add(i);
                            geo.Indices.Add((i + 1) % count);
                        }
                    }

                    LineGeometryModel3D line = new LineGeometryModel3D();
                    line.Geometry = geo;
                    line.Color = new SharpDX.Color(0, 0, 0);
                    line.Thickness = 0.5;
                    item.Wireframe = line;

                    line.Tag = item.ifcType + ":" + item.ifcID;

                    model.Add(line);
                }

                CreateWireFrameModelsRecursive_WPFStyle(item.child, center);

                item = item.next;
            }
        }

        protected void ReadObjectsFromIFCFile(XmlTextReader textReader)
        {
            while (textReader.Read())
            {
                textReader.MoveToElement();

                if (textReader.AttributeCount > 0)
                {
                    if (textReader.LocalName == "object")
                    {
                        var attributeValue = textReader.GetAttribute("name");

                        if (!string.IsNullOrEmpty(attributeValue))
                        {
                            //retrieve Object()
                            RetrieveObjectsFromReader(attributeValue);
                        }
                    }
                }
            }
            
            GenerateGeometry(this.IfcModel, RootItem);

            ModelManager.Instance.IFCEngine.CloseModel(this.IfcModel);
        }

        private void RetrieveObjectsFromReader(string attribute)
        {
            IfcObjectInstances = ModelManager.Instance.IFCEngine.GetEntityExtent(IfcModel, attribute);

            NumberIfcObjectInstance = ModelManager.Instance.IFCEngine.GetMemberCount(IfcObjectInstances);

            if(NumberIfcObjectInstance != IntPtr.Zero)
            {
                RecursiveGettingItem(ref RootItem, attribute);
            }

        }

        private void RecursiveGettingItem(ref IFCItem item, string attribute)
        {
            if (item == null)
            {
                item = new IFCItem();

                CreateItem(item, attribute);
            }
            else
            {
                RecursiveGettingItem(ref item.next, attribute);
            }
        }

        private void CreateItem(IFCItem item ,string attribute)
        {
            item.CreateItem(null, IntPtr.Zero, "", attribute, "", "");

            CreateSubItem(item, attribute);
        }

        private void CreateSubItem(IFCItem item, string attribute)
        {
            for (int i = 0; i < NumberIfcObjectInstance.ToInt32() ; i++)
            {
                IntPtr ifcObject = IntPtr.Zero;

                ModelManager.Instance.IFCEngine.GetAggregationElement(this.IfcObjectInstances, i, IfcEngine.SdaiType.Instance, out ifcObject);

                var GlobalId = GettingAttribute(ifcObject, "GlobalId");
                var Name = GettingAttribute(ifcObject, "Name");
                var Description = GettingAttribute(ifcObject, "Description");

                IFCItem subItem = new IFCItem();
                subItem.CreateItem(item, ifcObject, attribute, GlobalId, Name, Description);
            }
        }

        private string GettingAttribute(IntPtr ifcObject, string attributeKey)
        {
            IntPtr ifcAttribute = IntPtr.Zero;

            ModelManager.Instance.IFCEngine.GetAttribute(ifcObject, attributeKey, IfcEngine.SdaiType.Unicode, out ifcAttribute);

            return Marshal.PtrToStringUni(ifcAttribute);
        }

        private void GenerateGeometry(IntPtr model, IFCItem item)
        {
            while (item != null)
            {
                // Generate WireFrames Geometry
                IfcEngine.Setting setting = IfcEngine.Setting.Default;
                IfcEngine.Mask mask = IfcEngine.Mask.Default;
                mask |= IfcEngine.Mask.DoublePrecision; //    PRECISION (32/64 bit)
                mask |= IfcEngine.Mask.UseIndex64; //	   INDEX ARRAY (32/64 bit)
                mask |= IfcEngine.Mask.GenNormals; //    NORMALS
                mask |= IfcEngine.Mask.GenTriangles; //    TRIANGLES
                mask |= IfcEngine.Mask.GenWireFrame; //    WIREFRAME
                setting |= IfcEngine.Setting.GenWireframe; //    WIREFRAME ON

                ModelManager.Instance.IFCEngine.SetFormat(model, setting, mask);

                GenerateWireFrameGeometry(model, item);

                setting = IfcEngine.Setting.Default;
                setting |= IfcEngine.Setting.GenNormals; //    NORMALS ON
                setting |= IfcEngine.Setting.GenTriangles; //    TRIANGLES ON

                ModelManager.Instance.IFCEngine.SetFormat(model, setting, mask);

                GenerateFacesGeometry(model, item);

                ModelManager.Instance.IFCEngine.CleanMemory(model);

                GenerateGeometry(model, item.child);

                item = item.next;
            }
        }

        private void GenerateWireFrameGeometry(IntPtr model, IFCItem item)
        {
            if (item.ifcID != IntPtr.Zero)
            {
                IntPtr noVertices = IntPtr.Zero;
                IntPtr noIndices = IntPtr.Zero;

                ModelManager.Instance.IFCEngine.InitializeModellingInstance(model, ref noVertices, ref noIndices, 0, item.ifcID);

                if (noVertices != IntPtr.Zero && noIndices != IntPtr.Zero)
                {
                    item.noVerticesForWireFrame = noVertices.ToInt32();
                    item.verticesForWireFrame = new float[3 * noVertices.ToInt32()];
                    item.indicesForWireFrame = new int[noIndices.ToInt32()];

                    ModelManager.Instance.IFCEngine.FinalizeModelling(model, item.verticesForWireFrame, item.indicesForWireFrame, IntPtr.Zero);

                    item.noPrimitivesForWireFrame = 0;
                    item.indicesForWireFrameLineParts = new int[2 * noIndices.ToInt32()];

                    int faceCount = ModelManager.Instance.IFCEngine.GetConceptualFaceCount(item.ifcID).ToInt32();

                    for (int i = 0; i < faceCount; i++)
                    {
                        IntPtr startIndexFacesPolygons = IntPtr.Zero, 
                            noIndicesFacesPolygons = IntPtr.Zero, 
                            indexTriangles = IntPtr.Zero, 
                            indexLines = IntPtr.Zero,
                            indexPoints = IntPtr.Zero,
                            indexConceptualFacePolygon = IntPtr.Zero;

                        ModelManager.Instance.IFCEngine.GetConceptualFaceEx(item.ifcID, new IntPtr(i), ref indexTriangles, ref indexTriangles,
                            ref indexLines, ref indexLines, ref indexPoints, ref indexPoints, ref startIndexFacesPolygons,
                            ref noIndicesFacesPolygons, ref indexConceptualFacePolygon, ref indexConceptualFacePolygon);

                        int j = 0, lastItem = -1;

                        while (j < noIndicesFacesPolygons.ToInt32())
                        {
                            if (lastItem >= 0 && item.indicesForWireFrame[startIndexFacesPolygons.ToInt32() + j] >= 0)
                            {
                                item.indicesForWireFrameLineParts[2 * item.noPrimitivesForWireFrame] = lastItem;
                                item.indicesForWireFrameLineParts[2 * item.noPrimitivesForWireFrame + 1] = 
                                    item.indicesForWireFrame[startIndexFacesPolygons.ToInt32() + j];

                                item.noPrimitivesForWireFrame++;
                            }

                            lastItem = item.indicesForWireFrame[startIndexFacesPolygons.ToInt32() + j];
                            j++;
                        }

                    }

                }
            }
        }

        private void GenerateFacesGeometry(IntPtr model, IFCItem item)
        {
            if (item.ifcID != IntPtr.Zero)
            {
                IntPtr noVertices = IntPtr.Zero, noIndices = IntPtr.Zero;

                ModelManager.Instance.IFCEngine.InitializeModellingInstance(model, ref noVertices, ref noIndices, 0, item.ifcID);

                if (noVertices != IntPtr.Zero && noIndices != IntPtr.Zero)
                {
                    item.noVerticesForFaces = noVertices.ToInt32();
                    item.noPrimitivesForFaces = noIndices.ToInt32() / 3;
                    item.verticesForFaces = new float[6 * noVertices.ToInt32()];
                    item.indicesForFaces = new int[noIndices.ToInt32()];

                    float[] pVertices = new float[noVertices.ToInt32() * 6];

                    ModelManager.Instance.IFCEngine.FinalizeModelling(model, pVertices, item.indicesForFaces, IntPtr.Zero);

                    int i = 0;
                    while (i < noVertices.ToInt32())
                    {
                        item.verticesForFaces[6 * i + 0] = pVertices[6 * i + 0];
                        item.verticesForFaces[6 * i + 1] = pVertices[6 * i + 1];
                        item.verticesForFaces[6 * i + 2] = pVertices[6 * i + 2];

                        item.verticesForFaces[6 * i + 3] = pVertices[6 * i + 3];
                        item.verticesForFaces[6 * i + 4] = pVertices[6 * i + 4];
                        item.verticesForFaces[6 * i + 5] = pVertices[6 * i + 5];

                        for (int j = 0; j < 3; j++)
                        {
                            ModelManager.Instance.MinCorner[j] = Math.Min(ModelManager.Instance.MinCorner[j], pVertices[6 * i + j]);
                            ModelManager.Instance.MaxCorner[j] = Math.Max(ModelManager.Instance.MaxCorner[j], pVertices[6 * i + j]);
                        }

                        i++;
                    }
                }
            }
        }

        private void FillMeshByIfcColor_WPF(IFCItem item)
        {
            if (item.Mesh3d != null)
            {
                Random rand = new Random();
                byte[] colorValue = new byte[4];
                rand.NextBytes(colorValue);

                //var ifcColor = item.ifcTreeItem.ifcColor;
                var color = System.Windows.Media.Color.FromArgb(
                    (byte)(255 - colorValue[0] * 255), 
                    (byte)(colorValue[1] * 255), 
                    (byte)(colorValue[2] * 255),
                    (byte)(colorValue[3] * 255));

                item.Mesh3d.Material = new PhongMaterial()
                {
                    AmbientColor = System.Windows.Media.Colors.Black.ToColor4(),
                    DiffuseColor = System.Windows.Media.Colors.Black.ToColor4(),
                    EmissiveColor = color.ToColor4(),
                    ReflectiveColor = System.Windows.Media.Colors.Black.ToColor4(),
                    SpecularColor = color.ToColor4(),
                };
            }
            else
            {
                item.Mesh3d.Material = PhongMaterials.Bronze;
            }
        }

        private void FillMeshByIfcColor_WinForm(IFCItem item)
        {
            //if (item.Mesh3d != null)
            //{
            //    var ifcColor = item.ifcTreeItem.ifcColor;
            //    var color = System.Windows.Media.Color.FromArgb((byte)(255 - ifcColor.A * 255),
            //        (byte)(ifcColor.R * 255), (byte)(ifcColor.G * 255), (byte)(ifcColor.B * 255));
            //    item.Mesh3d.Fill = new SolidColorBrush(color);
            //}
            //else
            //{
            //    item.Mesh3d.Fill = _defaultBrush;
            //}

        }

    }
}
