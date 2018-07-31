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
    public class IFC2FileReader: BaseIFCFileReader
    {
        public IFC2FileReader(IfcEngine engine, string path)
            :base(engine,path)
        {
            this.path = path;
        }

        protected override void ParsIFCFile()
        {
            IntPtr ifcModel = IfcEngine.OpenModelUnicode(IntPtr.Zero, path, Constants.IFC2X3_SCHEMA_NAME);

            if (ifcModel != IntPtr.Zero)
            {
                var textReader = new XmlTextReader(Constants.XML_SETTINGS_IFC2x3);

                base.ReadObjectsFromIFCFile(textReader);
            }
        }
    }
}
