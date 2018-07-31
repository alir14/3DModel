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
        protected IfcEngine IfcEngine { get; set; }
        protected string path { get; set; }
        protected IntPtr IfcModel { get; set; }
        IFCItem RootItem { get; set; }

        public BaseIFCFileReader(IfcEngine engine, string path)
        {
            this.IfcEngine = engine;
            this.path = path;
        }

        protected virtual void ParsIFCFile()
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
        }

        internal void RetrieveObjectsFromReader(string attribute)
        {
            IntPtr IfcObjectInstance, NumberIfcObjectInstance;

            IfcObjectInstance = IfcEngine.GetEntityExtent(IfcModel, attribute);

            NumberIfcObjectInstance = IfcEngine.GetEntityCount(IfcObjectInstance);

            if(NumberIfcObjectInstance != IntPtr.Zero)
            {
                RecursiveGettingItem(RootItem, attribute);
            }

        }

        internal void RecursiveGettingItem(IFCItem item, string value)
        {
            if (RootItem == null)
            {
                CreateItem(value, RootItem);
            }
            else
            {
                RecursiveGettingItem(RootItem.next, value);
            }
        }

        internal void CreateItem(string value, IFCItem item)
        {
            item.CreateItem(null, IntPtr.Zero, "", value, "", "");
        }

    }
}
