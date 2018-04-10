using HelixToolkit.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        Popup loadPopup = new Popup();

        public ObservableCollection<Sticker> UserStickers { get; set; }

        public Sticker UserSticker { get; set; }

        public Point3D? StickerPoint { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            UserStickers = new ObservableCollection<Sticker>();

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            viewer.ZoomExtents();
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
            //if(device3D != null && viewer.Children.Contains(device3D))
            //{
            //    bool tmp = viewer.Children.Remove(device3D);
            //}

            viewer.Children.Clear();
        }

        private void viewer_MouseMove(object sender, MouseEventArgs e)
        {
            var point = viewer.FindNearestPoint(e.GetPosition(viewer));

            if(point.HasValue)
            {
                txtX.Text = point.Value.X.ToString();
                txtY.Text = point.Value.Y.ToString();
                txtZ.Text = point.Value.Z.ToString();

                if(CheckCurrentPoint((Point3D)point))
                {
                    lblFlag.Content = "In bound show !!!";
                    ShowDetail();
                }
                else
                {
                    lblFlag.Content = "Out bound show !!!";
                }

            }
        }

        private void viewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                this.StickerPoint = viewer.FindNearestPoint(e.GetPosition(viewer));

                if (this.StickerPoint.HasValue)
                {
                    myPopup.IsOpen = true;


                }

            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    myPopup.IsOpen = true;
        //}

        private void BtnClose_Click (object sender, RoutedEventArgs e)
        {
            var glue = new SphereVisual3D() { Center = (Point3D)this.StickerPoint, Radius = 0.2, Material = Materials.Yellow };
            
            viewer.Children.Add(glue);

            UserStickers.Add(new Sticker()
            {
                ID = Guid.NewGuid(),
                SelectedPoint = (Point3D)this.StickerPoint,
                AttachedSticker = glue,
                UserMessage = txtUserComment.Text
            });

            lstSelectedPoints.ItemsSource = UserStickers;

            myPopup.IsOpen = false;
        }

        private bool CheckCurrentPoint(Point3D point)
        {
            foreach(var item in UserStickers)
            {
                if(
                    (point.X <= (item.SelectedPoint.X + 0.2) && point.X >= (item.SelectedPoint.X - 0.2))
                    && (point.Y <= (item.SelectedPoint.Y + 0.2) && point.Y >= (item.SelectedPoint.Y - 0.2))
                    && (point.Z <= (item.SelectedPoint.Z + 0.2) && point.Z >= (item.SelectedPoint.Z - 0.2))
                )
                {
                    this.UserSticker = item;
                    return true;
                }
            }

            return false;
        }

        private void ShowDetail()
        {
            if(this.UserSticker != null)
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

        private void btnLoadPopUpClose_Click1(object sender, RoutedEventArgs e)
        {
            loadPopup.IsOpen = false;
        }
    }
}
