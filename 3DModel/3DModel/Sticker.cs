using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DModel
{
    public class Sticker
    {
        public Guid ID { get; set; }

        public Point3D SelectedPoint { get; set; }

        public Visual3D AttachedSticker { get; set; }

        public string X
        {
            get { return this.SelectedPoint.X.ToString(); }
        }

        public string Y {
            get { return this.SelectedPoint.Y.ToString(); }
        }

        public string Z { 
            get { return this.SelectedPoint.Z.ToString(); }
        }

        public string UserMessage{ get; set; }
    }
}
