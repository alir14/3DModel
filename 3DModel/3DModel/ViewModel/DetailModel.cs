using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace _3DModel.ViewModel
{
    public class DetailModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        List<AttachmentModel> attachedFile = new List<AttachmentModel>();
        string _selectedItemId;
        string _modelName;
        string _selectedItemTitle;
        string _selectedItemComment;
        BitmapImage _capturedImage;

        public Guid Id { get; set; }

        public string SelectedItemId
        {
            get { return _selectedItemId; }
            set {
                _selectedItemId = value;
                OnPropertyChanged("SelectedItemId");
            }
        }

        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value;
                OnPropertyChanged("ModelName");
            }
        }

        public string SelectedItemTitle
        {
            get { return _selectedItemTitle; }
            set { _selectedItemTitle = value;
                OnPropertyChanged("SelectedItemTitle");
            }
        }

        public string SelectedItemComment
        {
            get { return _selectedItemComment; }
            set { _selectedItemComment = value;
                OnPropertyChanged("SelectedItemComment");
            }
        }

        public BitmapImage CapturedImage
        {
            get { return _capturedImage; }
            set { _capturedImage = value;
                OnPropertyChanged("CapturedImage");
            }
        }

        public List<AttachmentModel> AttachedFile
        {
            get { return attachedFile; }
            set { attachedFile = value;
                OnPropertyChanged("AttachedFile");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
