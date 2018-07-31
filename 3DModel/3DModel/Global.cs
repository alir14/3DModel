using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DModel
{
    public enum IFCType:short
    {
        None = 0,
        IFC2,
        IFC4
    }

    public class Constants
    {
        public const string IFC4_SCHEMA_NAME = "IFC4.exp";
        public const string IFC2X3_SCHEMA_NAME = "IFC2X3_TC1.exp";
        public const string XML_SETTINGS_IFC2x3 = @"IFC2X3-Settings.xml";
        public const string XML_SETTINGS_IFC4 = @"IFC4-Settings.xml";
    }

}
