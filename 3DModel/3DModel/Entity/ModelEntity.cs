using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace _3DModel.Entity
{
    public class ModelEntity
    {
        public ModelEntity()
        {
            this.AttachedFileNames = new List<string>();
        }

        public Guid Id { get; set; }

        public string SelectedItemId { get; set; }

        public string ModelName { get; set; }

        public string SelectedItemTitle { get; set; }

        public System.Windows.Media.Media3D.Point3D SelectedItemPosition3D { get; set; }

        public string SelectedItemComment { get; set; }

        public BitmapImage CapturedImage { get; set; }

        public List<string> AttachedFileNames { get; set; }
    }
}
