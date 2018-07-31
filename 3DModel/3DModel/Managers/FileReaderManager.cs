using IfcEngineCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _3DModel.Managers
{
    public class FileReaderManager
    {
        private readonly IfcEngine ifcEngine = new IfcEngine();
        const string IFC2X3_SCHEMA_NAME = "IFC2X3_TC1.exp";
        const string XML_SETTINGS_IFC2x3 = @"IFC2X3-Settings.xml";
        const string XML_SETTINGS_IFC4 = @"IFC4-Settings.xml";
        #region singltone

        private static FileReaderManager instance = null;
        private static readonly object processLock = new object();

        private FileReaderManager Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(processLock)
                    {
                        if (instance == null)
                        {
                            instance = new FileReaderManager();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        public IFCType GetIfcType(string path)
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
