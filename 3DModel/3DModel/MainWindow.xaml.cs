using System;
using SharpDX;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using _3DModel.Entity;
using HelixToolkit.Wpf;
using _3DModel.Managers;
using System.Windows.Media;
using System.Windows.Input;
using _3DModel.DataComponent;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
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
        BitmapImage selectedBitmap = null;
        List<string> lstAttachedFile = new List<string>();

        public ObservableCollection<Sticker> UserStickers { get; set; }

        public Sticker UserSticker { get; set; }

        public MainWindow()
        {
            InitializeComponent();

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
                    BindDetail();
                }
                else
                {
                    DetailSection.Visibility = Visibility.Hidden;

                }
            }
        }

        private void BtnBrows_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            ResetControl();

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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    lstAttachedFile.Add(openFileDialog.FileName);
                    lstAttachedName.ItemsSource = lstAttachedFile;
                }
            }
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            selectedBitmap = CaptureImage(viewer, 10, 30);
            selectedImage.Source = selectedBitmap;
        }

        private void BindDetail()
        {
            try
            {

                DetailSection.Visibility = Visibility.Visible;

                var entity = DataKeeper.Instance.ReadData(ModelManager.Instance.ModelName,
                    ModelManager.Instance.SelectedIfcItem.globalID);

                txtItemModelName.Text = ModelManager.Instance.ModelName;
                txtItemTitle.Text = ModelManager.Instance.SelectedIfcItem.ifcType;
                txtItemGlobalId.Text = ModelManager.Instance.SelectedIfcItem.globalID;

                if (entity != null)
                {
                    txtItemComment.Text = entity.SelectedItemComment;
                    selectedImage.Source = entity.CapturedImage;
                    lstAttachedName.ItemsSource = entity.AttachedFileNames;
                }
                else
                {
                    txtItemComment.Text = "";
                    selectedImage.Source = null;
                    lstAttachedFile = new List<string>();
                    lstAttachedName.ItemsSource = null;

                }
            }
            catch
            {

            }
        }

        private BitmapImage CaptureImage(UIElement element, double scale, int quality)
        {
            var result = new BitmapImage();

            Brush sourceBrush = Brushes.Black;
            double actualWidth = element.RenderSize.Width;
            double actualHeight = element.RenderSize.Height;

            double renderWidth = actualWidth * scale;
            double renderHeight = ActualHeight * scale;

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)actualWidth, (int)actualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(element);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            
            using (var stream = new MemoryStream())
            {
                pngImage.Save(stream);

                stream.Seek(0, SeekOrigin.Begin);

                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
            }

            return result;
        }

        private void ResetControl()
        {
            DetailSection.Visibility = Visibility.Hidden;
            selectedBitmap = null;
            selectedImage.Source = null;
            txtItemModelName.Text = "";
            txtItemTitle.Text = "";
            txtItemGlobalId.Text = "";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var item = new ModelEntity()
            {
                Id = Guid.NewGuid(),
                CapturedImage = selectedBitmap,
                SelectedItemComment = txtItemComment.Text,
                SelectedItemId = txtItemGlobalId.Text,
                ModelName = txtItemModelName.Text,
                SelectedItemTitle = txtItemTitle.Text,
                AttachedFileNames = lstAttachedFile
            };

            DataKeeper.Instance.Save(item);
        }

        
    }
}
