using _3DModel.Managers;
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
        public IFC2FileReader(string path)
            :base(path)
        {
            this.Path = path;
        }

        public override void ParsIFCFile()
        {
            base.IfcModel = ModelManager.Instance.IFCEngine.OpenModel(IntPtr.Zero, Path, Constants.IFC2X3_SCHEMA_NAME);

            if (base.IfcModel != IntPtr.Zero)
            {
                var textReader = new XmlTextReader(Constants.XML_SETTINGS_IFC2x3);

                base.ReadObjectsFromIFCFile(textReader);
            }
        }
    }
}
