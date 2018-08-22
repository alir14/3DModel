using System;
using SharpDX;
using System.Windows.Controls;
using HelixToolkit.Wpf.SharpDX;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace _3DModel.ViewModel
{
    public class MainViewModel:BaseViewModel
    {
        Element3DCollection model;
        List<AttachmentModel> imageList = new List<AttachmentModel>();

        //public HelixToolkit.Wpf.SharpDX.MeshGeometry3D Sphere { get; private set; }
        public Transform3D Light1Transform { get; private set; }
        public Transform3D Light2Transform { get; private set; }
        public Transform3D Light3Transform { get; private set; }
        public Transform3D Light4Transform { get; private set; }
        public Transform3D Light1DirectionTransform { get; private set; }
        public Transform3D Light4DirectionTransform { get; private set; }

        public PhongMaterial LightModelMaterial { get; set; }

        public Vector3 Light1Direction { get; set; }
        public Vector3 Light4Direction { get; set; }
        public Vector3D LightDirection4 { get; set; }
        public Color4 Light1Color { get; set; }
        public Color4 Light2Color { get; set; }
        public Color4 Light3Color { get; set; }
        public Color4 Light4Color { get; set; }
        public Color4 AmbientLightColor { get; set; }
        public Vector3 Light2Attenuation { get; set; }
        public Vector3 Light3Attenuation { get; set; }
        public Vector3 Light4Attenuation { get; set; }
        public bool RenderLight1 { get; set; }
        public bool RenderLight2 { get; set; }
        public bool RenderLight3 { get; set; }
        public bool RenderLight4 { get; set; }
        public List<AttachmentModel> AttachmentList
        {
            get { return imageList; }
            set { imageList = value; }
        }


        public MainViewModel()
        {
            // camera setup
            base.Camera = new HelixToolkit.Wpf.SharpDX.PerspectiveCamera
            {
                Position = new Point3D(8, 9, 7),
                LookDirection = new Vector3D(-5, -12, -5),
                UpDirection = new Vector3D(0, 0, 1)
            };

            // setup scene
            AmbientLightColor = new Color4(0.2f, 0.2f, 0.2f, 1.0f);

            RenderLight1 = true;
            RenderLight2 = false;
            RenderLight3 = false;
            RenderLight4 = true;

            Light1Color = (Color4)Color.White;
            Light2Color = (Color4)Color.Red;
            Light3Color = (Color4)Color.LightYellow;
            Light4Color = (Color4)Color.LightBlue;

            Light2Attenuation = new Vector3(1.0f, 0.5f, 0.10f);
            Light3Attenuation = new Vector3(1.0f, 0.1f, 0.05f);
            Light4Attenuation = new Vector3(1.0f, 0.2f, 0.0f);

            Light1Direction = new Vector3(0, -10, -10);
            Light1Transform = new TranslateTransform3D(-Light1Direction.ToVector3D());

            Light4Direction = new Vector3(0, -5, 0);
            Light4Transform = new TranslateTransform3D(-Light4Direction.ToVector3D());

            this.LightModelMaterial = new PhongMaterial
            {
                AmbientColor = Color.Gray,
                DiffuseColor = Color.Gray,
                EmissiveColor = Color.Yellow,
                SpecularColor = Color.Black,
            };
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
