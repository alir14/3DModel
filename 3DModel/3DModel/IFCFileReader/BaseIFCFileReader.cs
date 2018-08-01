using IfcEngineCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _3DModel.IFCFileReader
{
    public abstract class BaseIFCFileReader
    {
        public IFCItem RootItem;

        protected IfcEngine IfcEngine { get; set; }
        protected string Path { get; set; }
        protected IntPtr IfcModel { get; set; }

        private IntPtr IfcObjectInstances, NumberIfcObjectInstance;

        public BaseIFCFileReader(IfcEngine engine, string path)
        {
            this.IfcEngine = engine;
            this.Path = path;
        }

        // should override
        public virtual void ParsIFCFile()
        {
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
        }

        private void RetrieveObjectsFromReader(string attribute)
        {
            IfcObjectInstances = IfcEngine.GetEntityExtent(IfcModel, attribute);

            NumberIfcObjectInstance = IfcEngine.GetMemberCount(IfcObjectInstances);

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

                this.IfcEngine.GetAggregationElement(this.IfcObjectInstances, i, IfcEngine.SdaiType.Instance, out ifcObject);

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

            this.IfcEngine.GetAttribute(ifcObject, attributeKey, IfcEngine.SdaiType.Unicode, out ifcAttribute);

            return Marshal.PtrToStringUni(ifcAttribute);
        }

        private void GenerateGeometry(IntPtr model, IFCItem item)
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
            this.IfcEngine.SetFormat(model, setting, mask);

            while(item != null)
            {
                GenerateWireFrameGeometry(model, item);

                this.IfcEngine.CleanMemory(model);

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

                this.IfcEngine.InitializeModellingInstance(model, ref noVertices, ref noIndices, 0, item.ifcID);

                if (noVertices != IntPtr.Zero && noIndices != IntPtr.Zero)
                {
                    item.noVerticesForWireFrame = noVertices.ToInt32();
                    item.verticesForWireFrame = new float[3 * noVertices.ToInt32()];
                    item.indicesForWireFrame = new int[noIndices.ToInt32()];

                    this.IfcEngine.FinalizeModelling(model, item.verticesForWireFrame, item.indicesForWireFrame, IntPtr.Zero);

                    item.noPrimitivesForWireFrame = 0;
                    item.indicesForWireFrameLineParts = new int[2 * noIndices.ToInt32()];

                    int faceCount = this.IfcEngine.GetConceptualFaceCount(item.ifcID).ToInt32();

                    for (int i = 0; i < faceCount; i++)
                    {
                        IntPtr startIndexFacesPolygons = IntPtr.Zero, 
                            noIndicesFacesPolygons = IntPtr.Zero, 
                            indexTriangles = IntPtr.Zero, 
                            indexLines = IntPtr.Zero,
                            indexPoints = IntPtr.Zero,
                            indexConceptualFacePolygon = IntPtr.Zero;

                        this.IfcEngine.GetConceptualFaceEx(item.ifcID, new IntPtr(i), ref indexTriangles, ref indexTriangles,
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
    }
}
