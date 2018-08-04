using HelixToolkit.Wpf.SharpDX;
using System.Windows.Media.Media3D;

namespace _3DModel
{
    public class MainViewModel:BaseViewModel
    {
        Element3DCollection model;

        public MainViewModel()
        {
            // camera setup
            base.Camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera {
                Position = new Point3D(8, 9, 7),
                LookDirection = new Vector3D(-5, -12, -5),
                UpDirection = new Vector3D(0, 0, 1) };
        }

        public Element3DCollection Model
        {
            get { return model; }
            set
            {
                model = value;
                base.OnPropertyChanged("Model");
            }
        }
        
    }
}
