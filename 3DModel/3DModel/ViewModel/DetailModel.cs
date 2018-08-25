using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace _3DModel.ViewModel
{
    public class DetailModel
    {
        List<AttachmentModel> attachedFile = new List<AttachmentModel>();

        public Guid Id { get; set; }

        public string SelectedItemId { get; set; }

        public string ModelName { get; set; }

        public string SelectedItemTitle { get; set; }

        public System.Windows.Media.Media3D.Point3D SelectedItemPosition3D { get; set; }

        public string SelectedItemComment { get; set; }

        public BitmapImage CapturedImage { get; set; }

        public List<AttachmentModel> AttachedFile
        {
            get { return attachedFile; }
            set { attachedFile = value; }
        }
    }
}
