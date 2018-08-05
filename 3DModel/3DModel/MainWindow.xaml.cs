using System;
using SharpDX;
using System.Windows;
using Microsoft.Win32;
using HelixToolkit.Wpf;
using _3DModel.Managers;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace _3DModel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Popup loadPopup = new Popup();

        public ObservableCollection<Sticker> UserStickers { get; set; }

        public Sticker UserSticker { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            viewer.MouseMove += Viewer_MouseMove;
            viewer.MouseUp += Viewer_MouseUp;

            UserStickers = new ObservableCollection<Sticker>();

            this.DataContext = ModelManager.Instance.ViewModel;
        }

        private void Viewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var point = Mouse.GetPosition(viewer);
                if (HelixToolkit.Wpf.SharpDX.ViewportExtensions.FindNearest(viewer, point,
                    out System.Windows.Media.Media3D.Point3D point3d,
                    out System.Windows.Media.Media3D.Vector3D normal,
                    out HelixToolkit.Wpf.SharpDX.Model3D model))
                {
                    ModelManager.Instance.OnModelSelected(model);
                    BindDetail(point3d, normal);
                }
                else
                {
                    DetailSection.Visibility = Visibility.Hidden;

                }
            }
        }

        private void Viewer_MouseMove(object sender, MouseEventArgs e)
        {
            var point = Mouse.GetPosition(viewer);

            if (HelixToolkit.Wpf.SharpDX.ViewportExtensions.FindNearest(viewer, point,
                out System.Windows.Media.Media3D.Point3D point3d,
                out System.Windows.Media.Media3D.Vector3D normal,
                out HelixToolkit.Wpf.SharpDX.Model3D model))
            {
                //ModelManager.Instance.OnModelHovered(model);
            }
        }

        private void BtnBrows_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if(openFileDialog.ShowDialog() == true)
            {
                if(!string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    LoadIFCFile(openFileDialog.FileName);
                }
            }
        }

        private void BtnClose_Click (object sender, RoutedEventArgs e)
        {

        }

        private void btnLoadPopUpClose_Click1(object sender, RoutedEventArgs e)
        {
            loadPopup.IsOpen = false;
        }

        private void LoadIFCFile(string filePath)
        {
            ModelManager.Instance.ResetModel();
            ModelManager.Instance.LoadModel(filePath, ElementObjectTree);
            ModelManager.Instance.InitModel();
            ModelManager.Instance.ZoomExtent(this.viewer);
            this.viewer.ReAttach();
        }

        private void ShowDetail()
        {
            if (this.UserSticker != null)
            {
                loadPopup.PopupAnimation = PopupAnimation.Fade;

                loadPopup.Width = 200;
                loadPopup.Height = 200;
                loadPopup.Placement = PlacementMode.MousePoint;

                Grid grid = new Grid();

                TextBlock popupText = new TextBlock();
                popupText.Text = this.UserSticker.UserMessage;
                popupText.Background = Brushes.LightBlue;
                popupText.Foreground = Brushes.Black;

                Button btnLoadPopUpClose = new Button();
                btnLoadPopUpClose.Width = 75;
                btnLoadPopUpClose.Height = 23;
                btnLoadPopUpClose.Content = "Close";
                btnLoadPopUpClose.Click += btnLoadPopUpClose_Click1;

                grid.Children.Add(popupText);
                grid.Children.Add(btnLoadPopUpClose);

                loadPopup.Child = grid;

                loadPopup.IsOpen = true;
            }
        }

        private void btnAttachFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BindDetail(System.Windows.Media.Media3D.Point3D point3d,
                    System.Windows.Media.Media3D.Vector3D normal)
        {
            DetailSection.Visibility = Visibility.Visible;
            txtItemPoint3D.Text = string.Format("{0} | {1} | {2}", point3d.X, point3d.Y, point3d.Z);
            txtItemPosition.Text = string.Format("{0} | {1} | {2}", normal.X, normal.Y, normal.Z);
            txtItemTitle.Text = ModelManager.Instance.SelectedIfcItem.ifcType;
            txtItemGlobalId.Text = ModelManager.Instance.SelectedIfcItem.globalID;
        }
    }
}
