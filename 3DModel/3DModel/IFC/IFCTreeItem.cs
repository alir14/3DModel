using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace _3DModel.IFC
{
    public class IFCTreeItem
    {
        /// <summary>
        /// Instance.
        /// </summary>
        public IntPtr instance = IntPtr.Zero;

        /// <summary>
        /// Node.
        /// </summary>
        public TreeViewItem treeNode = null;

        /// <summary>
        /// If it is not null the item can be selected.
        /// </summary>
        public IFCItem ifcItem = null;

        /// <summary>
        /// Color
        /// </summary>
        public IFCItemColor ifcColor = null;

        /// <summary>
        /// Getter
        /// </summary>
        public bool IsVisible
        {
            get
            {
                System.Diagnostics.Debug.Assert(treeNode != null, "Internal error.");

                if (treeNode.IsSelected) { return true; }

                return false;
            }
        }
    }
}
