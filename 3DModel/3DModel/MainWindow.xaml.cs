using System;
using System.IO;
using _3DModel.IFC;
using System.Windows;
using _3DModel.Entity;
using Microsoft.Win32;
using _3DModel.Managers;
using System.Windows.Input;
using System.Windows.Media;
using _3DModel.DataComponent;
using HelixToolkit.Wpf.SharpDX;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using _3DModel.ViewModel;

namespace _3DModel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapImage selectedBitmap = null;
        HelixToolkit.Wpf.SharpDX.Material originalItemColor;
        IFCItem SelectedIfcItem { get; set; }

        Point point;

        public MainWindow()
        {
            InitializeComponent();

            viewer.Drop += Viewer_Drop;
            viewer.DragOver += Viewer_DragOver;
            viewer.MouseDoubleClick += Viewer_MouseDoubleClick;
            this.DataContext = ModelManager.Instance.ViewModel;
        }

        private void Viewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedpoint = Mouse.GetPosition(viewer);
            if (HelixToolkit.Wpf.SharpDX.ViewportExtensions.FindNearest(viewer, selectedpoint,
                out System.Windows.Media.Media3D.Point3D point3d,
                out System.Windows.Media.Media3D.Vector3D normal,
                out HelixToolkit.Wpf.SharpDX.Model3D model))
            {
                var mesh = (model as MeshGeometryModel3D);
                if (mesh != null && ModelManager.Instance.IfcObject.MeshToIfcItems.ContainsKey(mesh))
                {
                    SelectedIfcItem = ModelManager.Instance.IfcObject.MeshToIfcItems[mesh];
                }

                var entity = DataKeeper.Instance.ReadData(ModelManager.Instance.ModelName, SelectedIfcItem.globalID);

                if (entity != null)
                {
                    txtItemModelName.Text = entity.ModelName;
                    txtItemGlobalId.Text = entity.SelectedItemId;
                    txtItemComment.Text = entity.SelectedItemComment;
                    selectedImage.Source = entity.CapturedImage;
                    ModelManager.Instance.ViewModel.AttachmentList = entity.AttachedFile;
                    lstcontrolAttachment.ItemsSource = ModelManager.Instance.ViewModel.AttachmentList;
                }

            }
        }

        private void Viewer_DragOver(object sender, DragEventArgs e)
        {
            point = e.GetPosition(viewer);
        }

        private void Viewer_Drop(object sender, DragEventArgs e)
        {
            if (point != null)
            {
                if (ViewportExtensions.FindNearest(viewer, point,
                    out System.Windows.Media.Media3D.Point3D point3d,
                    out System.Windows.Media.Media3D.Vector3D normal,
                    out HelixToolkit.Wpf.SharpDX.Model3D model))
                {
                    if (model != null)
                    {
                        if (SelectedIfcItem != null)
                            SelectedIfcItem.Mesh3d.Material = originalItemColor;

                        var mesh = (model as MeshGeometryModel3D);
                        if (mesh != null && ModelManager.Instance.IfcObject.MeshToIfcItems.ContainsKey(mesh))
                        {
                            originalItemColor = mesh.Material;
                            mesh.Material = PhongMaterials.Chrome;
                            SelectedIfcItem = ModelManager.Instance.IfcObject.MeshToIfcItems[mesh];
                            this.viewer.ReAttach();

                            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                            foreach (string file in files)
                            {
                                AttachFile(file);
                            }
                        }
                    }

                    BindDetail();
                }
                else
                {
                    SelectedIfcItem = null;
                }
            }
        }

        private void AttachFile(string file)
        {
            var selectedFile = file.Split('.');
            try
            {
                var destinationPath = Path.Combine(Environment.CurrentDirectory, "Attachments",
                    $"{Guid.NewGuid().ToString()}.{selectedFile[1]}");
                File.Copy(file, destinationPath);

                //System.Windows.Controls.Image img = new System.Windows.Controls.Image
                //{
                //    Source = new BitmapImage(new Uri(destinationPath)),
                //    Width = Height = 100
                //};
                ModelManager.Instance.ViewModel.AttachmentList.Add(new AttachmentModel() { Address = destinationPath});
            }
            catch
            {
                MessageBox.Show("Unable to save the selected file");
            }
        }

        private void LoadIFCFile(string filePath)
        {
            ModelManager.Instance.ResetModel();
            SelectedIfcItem = null;
            ModelManager.Instance.LoadModel(filePath);
            ModelManager.Instance.InitModel();
            ModelManager.Instance.ZoomExtent(this.viewer);
            this.viewer.ReAttach();
        }

        private void BindDetail()
        {
            try
            {
                txtItemModelName.Text = ModelManager.Instance.ModelName;
                txtItemTitle.Text = SelectedIfcItem.ifcType;
                txtItemGlobalId.Text = SelectedIfcItem.globalID;
                txtItemComment.Text = "";
                selectedImage.Source = null;
                lstcontrolAttachment.ItemsSource = ModelManager.Instance.ViewModel.AttachmentList;

            }
            catch
            {

            }
        }

        private BitmapImage CaptureImage(UIElement element, int quality)
        {
            var result = new BitmapImage();

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)element.RenderSize.Width, (int)element.RenderSize.Height, 96, 96, PixelFormats.Pbgra32);
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
            selectedBitmap = null;
            selectedImage.Source = null;
            txtItemModelName.Text = "";
            txtItemTitle.Text = "";
            txtItemGlobalId.Text = "";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnCaptureImage_Click(object sender, RoutedEventArgs e)
        {
            selectedBitmap = CaptureImage(viewer, 80);
            selectedImage.Source = selectedBitmap;
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "IFC files (*.ifc)|*.ifc";
            ResetControl();

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    ModelManager.Instance.ModelName = openFileDialog.SafeFileName;
                    LoadIFCFile(openFileDialog.FileName);
                }
            }
        }

        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            var item = new ModelEntity()
            {
                Id = Guid.NewGuid(),
                CapturedImage = selectedBitmap,
                SelectedItemComment = txtItemComment.Text,
                SelectedItemId = txtItemGlobalId.Text,
                ModelName = txtItemModelName.Text,
                SelectedItemTitle = txtItemTitle.Text,
                AttachedFile = ModelManager.Instance.ViewModel.AttachmentList
            };

            DataKeeper.Instance.Save(item);
        }
    }
}
