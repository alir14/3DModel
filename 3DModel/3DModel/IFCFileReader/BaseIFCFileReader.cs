using _3DModel.Managers;
using HelixToolkit.Wpf.SharpDX;
using IfcEngineCS;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Xml;

namespace _3DModel.IFCFileReader
{
    public abstract class BaseIFCFileReader
    {
        public IFCItem RootItem;

        protected string Path { get; set; }
        protected IntPtr IfcModel { get; set; }

        IntPtr IfcObjectInstances, NumberIfcObjectInstance;
        Brush DefaultBrush = Brushes.Gray;
        Material DefaultMaterial = HelixToolkit.Wpf.SharpDX.PhongMaterials.Bronze;
        System.Windows.Media.Color DefaultLineColor = System.Windows.Media.Color.FromRgb(0, 0, 0);
        Dictionary<HelixToolkit.Wpf.MeshGeometryVisual3D, IFCItem> MeshToIfcItems = new Dictionary<HelixToolkit.Wpf.MeshGeometryVisual3D, IFCItem>();
        List<HelixToolkit.Wpf.MeshGeometryVisual3D> modelElementCollection = new List<HelixToolkit.Wpf.MeshGeometryVisual3D>();
        List<HelixToolkit.Wpf.LinesVisual3D> modelLineCollection = new List<HelixToolkit.Wpf.LinesVisual3D>();

        public List<HelixToolkit.Wpf.LinesVisual3D> ModelLineCollection
        {
            get
            {
                return modelLineCollection;
            }
        }
        public List<HelixToolkit.Wpf.MeshGeometryVisual3D> ModelElementCollection
        {
            get
            {
                return modelElementCollection;
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

        public void CreateFaceModelsRecursive(IFCItem item, Vector3 center)
        {
            while (item != null)
            {
                if (item.ifcID != IntPtr.Zero && item.noVerticesForFaces != 0 && item.noPrimitivesForFaces != 0)
                {
                    var positions = new System.Windows.Media.Media3D.Point3DCollection();
                    var normals = new System.Windows.Media.Media3D.Vector3DCollection();

                    if (item.verticesForFaces != null)
                    {
                        for (int i = 0; i < item.noVerticesForFaces; i++)
                        {
                            positions.Add(new System.Windows.Media.Media3D.Point3D(item.verticesForFaces[6 * i + 0] - center.X,
                                item.verticesForFaces[6 * i + 1] - center.Y, item.verticesForFaces[6 * i + 2] - center.Z));

                            normals.Add(new System.Windows.Media.Media3D.Vector3D(item.verticesForFaces[6 * i + 3],
                                item.verticesForFaces[6 * i + 4], item.verticesForFaces[6 * i + 5]));
                        }
                    }

                    var indices = new Int32Collection();
                    if (item.indicesForFaces != null)
                    {
                        for (int i = 0; i < 3 * item.noPrimitivesForFaces; i++)
                        {
                            indices.Add(item.indicesForFaces[i]);
                        }
                    }

                    var meshGeometry = new System.Windows.Media.Media3D.MeshGeometry3D
                    {
                        Positions = positions,
                        Normals = normals,
                        TriangleIndices = indices
                    };

                    var mesh = new HelixToolkit.Wpf.MeshGeometryVisual3D() { MeshGeometry = meshGeometry };

                    item.Mesh3d = mesh;

                    MeshToIfcItems[mesh] = item;

                    FillMeshByIfcColor(item);

                    modelElementCollection.Add(mesh);
                }

                CreateFaceModelsRecursive(item.child, center);

                item = item.next;
            }
        }

        public void CreateWireFrameModelsRecursive(IFCItem item, Vector3 center)
        {
            while (item != null)
            {
                if (item.ifcID != IntPtr.Zero && item.noVerticesForWireFrame != 0 && item.noPrimitivesForWireFrame != 0)
                {
                    var points = new System.Windows.Media.Media3D.Point3DCollection();
                    System.Windows.Media.Media3D.Point3DCollection positions;

                    if (item.verticesForWireFrame != null)
                    {
                        for (int i = 0; i < item.noVerticesForWireFrame; i++)
                        {
                            points.Add(new System.Windows.Media.Media3D.Point3D(
                                (item.verticesForWireFrame[3 * i + 0] - center.X),
                                (item.verticesForWireFrame[3 * i + 1] - center.Y),
                                (item.verticesForWireFrame[3 * i + 2] - center.Z)));
                        }
                    }

                    if (item.indicesForWireFrameLineParts != null)
                    {
                        positions = new System.Windows.Media.Media3D.Point3DCollection();
                        for (int i = 0; i < item.noPrimitivesForWireFrame; i++)
                        {
                            var idx = item.indicesForWireFrameLineParts[2 * i + 0];
                            positions.Add(points[idx]);
                            idx = item.indicesForWireFrameLineParts[2 * i + 1];
                            positions.Add(points[idx]);
                        }
                    }
                    else
                    {
                        positions = points;
                    }

                    var wireframe = new HelixToolkit.Wpf.LinesVisual3D()
                    {
                        Points = positions,
                        Color = DefaultLineColor
                    };
                    item.Wireframe = wireframe;

                    modelLineCollection.Add(wireframe);
                }

                CreateWireFrameModelsRecursive(item.child, center);

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

        int counter = 0;

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

                counter++;
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

        private void FillMeshByIfcColor(IFCItem item)
        {
            if (item.Mesh3d != null)
            {
                //if (item.ifcTreeItem.ifcColor != null)
                //{
                    //var ifcColor = item.ifcTreeItem.ifcColor;
                    //var color = System.Windows.Media.Color.FromArgb((byte)(255 - ifcColor.A * 255),
                    //    (byte)(ifcColor.R * 255), (byte)(ifcColor.G * 255), (byte)(ifcColor.B * 255));
                    Random rand = new Random();
                    byte[] colorValue = new byte[3];
                    rand.NextBytes(colorValue);

                    var color = new System.Windows.Media.Color();
                    color.R = colorValue[0];
                    color.G = colorValue[1];
                    color.B = colorValue[2];

                    item.Mesh3d.Fill = new SolidColorBrush(color);
                //}
                //else
                //{
                //    item.Mesh3d.Fill = DefaultBrush;
                //}
            }
        }

    }
}
