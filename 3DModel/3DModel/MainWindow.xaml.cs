﻿using System;
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

        //public Point3D? StickerPoint { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            UserStickers = new ObservableCollection<Sticker>();
            ModelManager.Instance.MinCorner = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
            ModelManager.Instance.MaxCorner = new float[3] { float.MinValue, float.MinValue, float.MinValue };


        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            viewer.ZoomExtents();
        }

        private System.Windows.Media.Media3D.Model3D DisplayDesign(string modelPath)
        {
            System.Windows.Media.Media3D.Model3D device = null;

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
            LoadIFCFile(txtModelPath.Text);
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
            viewer.Children.Clear();
        }

        private void viewer_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void viewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    myPopup.IsOpen = true;
        //}

        private void BtnClose_Click (object sender, RoutedEventArgs e)
        {

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

        private void Reset()
        {
            ModelManager.Instance.ResetModel();
            viewer.Children.Clear();
            ModelManager.Instance.MinCorner = new float[3] { float.MinValue, float.MinValue, float.MinValue };
            ModelManager.Instance.MaxCorner = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
        }

        private void LoadIFCFile(string filePath)
        {
            Reset();
            ModelManager.Instance.LoadModel(filePath);
            InitModel();

            if(ModelManager.Instance.IfcObject.ModelElementCollection.Count > 0 &&
                ModelManager.Instance.IfcObject.ModelLineCollection.Count > 0)
            {
                foreach (var item in ModelManager.Instance.IfcObject.ModelElementCollection)
                {
                    viewer.Children.Add(item);
                }

                foreach (var item in ModelManager.Instance.IfcObject.ModelLineCollection)
                {
                    viewer.Children.Add(item);
                }
            }
        }

        private void InitModel()
        {
            Vector3 center = new Vector3(
                (ModelManager.Instance.MinCorner[0] + ModelManager.Instance.MaxCorner[0]) / 2 , 
                (ModelManager.Instance.MinCorner[1] + ModelManager.Instance.MaxCorner[1]) / 2 , 
                (ModelManager.Instance.MinCorner[2] + ModelManager.Instance.MaxCorner[2]) / 2);
            ModelManager.Instance.CreateMeshes(center);
            ModelManager.Instance.CreateWireFrames(center);
        }
    }
}
