using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DModel.ViewModel
{
    public class BaseViewModel: ObservableObject
    {
        private RenderTechnique renderTechnique;
        private string cameraModel;
        private Camera camera;
        public event EventHandler CameraModelChanged;

        public BaseViewModel()
        {
            this.CameraModelChanged += BaseViewModel_CameraModelChanged;
        }

        private void BaseViewModel_CameraModelChanged(object sender, EventArgs e)
        {
            if (this.cameraModel == Constants.ORTHOGRAPHIC_CAMERA)
            {
                this.Camera = new OrthographicCamera
                {
                    Position = new System.Windows.Media.Media3D.Point3D(0, 0, 5),
                    LookDirection = new System.Windows.Media.Media3D.Vector3D(-0, -0, -5),
                    UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0),
                    NearPlaneDistance = 1,
                    FarPlaneDistance = 100
                };
            }
            else if (this.cameraModel == Constants.PERSPECTIVE_CAMERA)
            {
                this.Camera = new PerspectiveCamera {
                    Position = new System.Windows.Media.Media3D.Point3D(0, 0, 5),
                    LookDirection = new System.Windows.Media.Media3D.Vector3D(-0, -0, -5),
                    UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0),
                    NearPlaneDistance = 0.5,
                    FarPlaneDistance = 150 };
            }
            else
            {
                throw new HelixToolkitException("Camera Model Error.");
            }

            this.cameraModel = Constants.PERSPECTIVE_CAMERA;
        }

        protected virtual void OnCameraModelChanged()
        {
            if(this.CameraModelChanged != null)
            {
                CameraModelChanged(this, new EventArgs());
            }
        }

        public RenderTechnique RenderTechnique
        {
            get
            { return renderTechnique; }
            set
            {
                base.SetValue(ref renderTechnique, value, "RenderTechnique");
            }
        }

        public string CameraModel
        {
            get
            {
                return this.cameraModel;
            }
            set
            {
                if (base.SetValue(ref cameraModel, value, "CameraModel"))
                {
                    OnCameraModelChanged();
                }
            }
        }

        public Camera Camera
        {
            get
            {
                return this.camera;
            }

            protected set
            {
                base.SetValue(ref this.camera, value, "Camera");
                this.CameraModel = value is PerspectiveCamera
                                       ? Constants.PERSPECTIVE_CAMERA
                                       : value is OrthographicCamera ? Constants.ORTHOGRAPHIC_CAMERA : null;
            }
        }
    }
}
