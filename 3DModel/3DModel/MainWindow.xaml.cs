using HelixToolkit.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _3DModel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ModelVisual3D device3D;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private Model3D DisplayDesign(string modelPath)
        {
            Model3D device = null;

            try
            {
                viewer.RotateGesture = new MouseGesture(MouseAction.RightClick);

                ModelImporter model = new ModelImporter();

                device = model.Load(modelPath); 
            }
            catch (Exception e)
            {
                // Handle exception in case can not find the 3D model file
                MessageBox.Show("Exception Error : " + e.StackTrace);
            }

            return device;
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            device3D = new ModelVisual3D
            {
                Content = DisplayDesign(txtModelPath.Text)
            };

            viewer.Children.Add(device3D);
        }

        private void BtnBrows_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if(openFileDialog.ShowDialog() == true)
            {
                txtModelPath.Text = openFileDialog.FileName;
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            if(device3D != null && viewer.Children.Contains(device3D))
            {
                bool tmp = viewer.Children.Remove(device3D);
            }
        }

        private void viewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var point = viewer.FindNearestPoint(e.GetPosition(viewer));

            if (point.HasValue)
            {
                viewer.Children.Add(new SphereVisual3D() { Center = (Point3D)point, Radius = 5, Material = Materials.Green });
            }
        }

        private void viewer_MouseMove(object sender, MouseEventArgs e)
        {
            var point = viewer.FindNearestPoint(e.GetPosition(viewer));

            if(point.HasValue)
            {
                txtCoordinate.Text = String.Format(@"X: {0} | Y: {1} | Z: {2} ", 
                    point.Value.X, point.Value.Y, point.Value.Z);
            }
        }
    }
}
