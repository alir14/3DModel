using IfcEngineCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _3DModel.IFCFileReader
{
    public class IFC4FileReader: BaseIFCFileReader
    {
        public IFC4FileReader(IfcEngine engine, string path)
            : base(engine, path)
        {
            this.path = path;
        }

        protected override void ParsIFCFile()
        {
            base.IfcModel = IfcEngine.OpenModelUnicode(IntPtr.Zero, path, Constants.IFC4_SCHEMA_NAME);

            if (base.IfcModel != IntPtr.Zero)
            {
                var textReader = new XmlTextReader(Constants.XML_SETTINGS_IFC4);

                base.ReadObjectsFromIFCFile(textReader);
            }
        }
    }
}
