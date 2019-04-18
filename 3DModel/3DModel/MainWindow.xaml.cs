using System;
using System.IO;
using System.Linq;
using _3DModel.IFC;
using System.Windows;
using Microsoft.Win32;
using _3DModel.Managers;
using _3DModel.ViewModel;
using System.Windows.Input;
using System.Windows.Media;
using _3DModel.DataComponent;
using HelixToolkit.Wpf.SharpDX;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace _3DModel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //BitmapImage selectedBitmap = null;
        HelixToolkit.Wpf.SharpDX.Material originalItemColor;
        IFCItem SelectedIfcItem { get; set; }
        Point point;
        DetailModel screenModelEntity = new DetailModel();

        public DetailModel ScreenModelEntity
        {
            get { return screenModelEntity; }
            set { screenModelEntity = value; }
        }

        public MainWindow()
        {
            InitializeComponent();

            viewer.Drop += Viewer_Drop;
            viewer.DragOver += Viewer_DragOver;
            viewer.MouseDoubleClick += Viewer_MouseDoubleClick;
            ScreenModelEntity.PropertyChanged += ScreenModelEntity_PropertyChanged;
            this.DataContext = ModelManager.Instance.ViewModel;
            Infosection.DataContext = ScreenModelEntity; 

            lstcontrolAttachment.SelectionChanged += LstcontrolAttachment_SelectionChanged;
        }

        private void ScreenModelEntity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Infosection.DataContext = ScreenModelEntity;
        }

        private void LstcontrolAttachment_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                var item = (AttachmentModel)lstcontrolAttachment.SelectedItem;
                if(item != null)
                    txtImageName.Text = item.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Viewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedpoint = Mouse.GetPosition(viewer);
            if (HelixToolkit.Wpf.SharpDX.ViewportExtensions.FindNearest(viewer, selectedpoint,
                out System.Windows.Media.Media3D.Point3D point3d,
                out System.Windows.Media.Media3D.Vector3D normal,
                out HelixToolkit.Wpf.SharpDX.Model3D model))
            {
                if (SelectedIfcItem != null && originalItemColor!= null)
                    SelectedIfcItem.Mesh3d.Material = originalItemColor;

                var mesh = (model as MeshGeometryModel3D);
                if (mesh != null && ModelManager.Instance.IfcObject.MeshToIfcItems.ContainsKey(mesh))
                {
                    SelectedIfcItem = ModelManager.Instance.IfcObject.MeshToIfcItems[mesh];
                }

                ScreenModelEntity = DataKeeper.Instance.ReadData(ModelManager.Instance.ModelName, SelectedIfcItem.globalID);
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

                            if(SelectedIfcItem.globalID != ScreenModelEntity.SelectedItemId)
                            {
                                ScreenModelEntity = new DetailModel();
                            }

                            ScreenModelEntity.ModelName = ModelManager.Instance.ModelName;
                            ScreenModelEntity.SelectedItemTitle = SelectedIfcItem.ifcType;


                            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                            foreach (string file in files)
                            {
                                AttachFile(file);
                            }
                        }
                    }
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
                File.Copy(file, destinationPath, true);
                File.SetAttributes(destinationPath, FileAttributes.Normal);

                ScreenModelEntity.AttachedFile.Add(new AttachmentModel() {
                    Address = destinationPath,
                    Name = destinationPath.Replace(Path.Combine(Environment.CurrentDirectory, "Attachments"),"").Replace("\\","")
                });
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
            ScreenModelEntity = new DetailModel();
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
            DataKeeper.Instance.Save(ScreenModelEntity);
        }

        private void RemoveAttachment_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(txtImageName.Name))
            {
                var selectedItem = ScreenModelEntity.AttachedFile.First(x => x.Name == txtImageName.Text);
                if(selectedItem != null)
                {
                    ScreenModelEntity.AttachedFile.Remove(selectedItem);

                    lstcontrolAttachment.ItemsSource = null;
                    lstcontrolAttachment.ItemsSource = ScreenModelEntity.AttachedFile;
                    lstcontrolAttachment.SelectedIndex = -1;

                    if (File.Exists(selectedItem.Address))
                    {
                        try
                        {
                            File.Delete(selectedItem.Address);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void captureImage_Click(object sender, RoutedEventArgs e)
        {
            ScreenModelEntity.CapturedImage = CaptureImage(viewer, 80);
        }
    }
}
