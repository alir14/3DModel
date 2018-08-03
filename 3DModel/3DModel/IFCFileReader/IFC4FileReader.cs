using _3DModel.Managers;
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
        public IFC4FileReader(string path)
            : base(path)
        {
            this.Path = path;
        }

        public override void ParsIFCFile()
        {
            base.IfcModel = ModelManager.Instance.IFCEngine.OpenModel(IntPtr.Zero, Path, Constants.IFC4_SCHEMA_NAME);

            if (base.IfcModel != IntPtr.Zero)
            {
                var textReader = new XmlTextReader(Constants.XML_SETTINGS_IFC4);

                base.ReadObjectsFromIFCFile(textReader);
            }
        }
    }
}
