using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Drawingboard.DataContracts;
using Drawingboard.Helper;
using KComponents;

namespace Drawingboard
{
    /// <summary>
    /// Interaction logic for Wall.xaml
    /// </summary>
    public partial class Wall : UserControl
    {
        List<Image> pictures = new List<Image>();
        Thickness margin = new Thickness(5);

        Storyboard sbShow;
        Storyboard sbHide;

        Drawingboard currentDb;

        Dictionary<string, DrawingboardData> boards = new Dictionary<string, DrawingboardData>();

        public event EventHandler CloseEvent;

        bool isLoadFiles = false;

        const int maxCount = 16;
        const int layoutKey = 4;
        public Wall()
        {
            InitializeComponent();
            this.kMenu.FireEvent += kMenu_FireEvent;

            KMenuItem open = new KMenuItem(Config.KEYOPEN, "Doodling");
            KMenuItem close = new KMenuItem(Config.KEYCLOSE, "Exit");
            this.kMenu.Items.Add(open);
            this.kMenu.Items.Add(close);
            this.kMenu.Render();

            this.sbShow = this.Resources["showDrawing"] as Storyboard;
            this.sbHide = this.Resources["hideDrawing"] as Storyboard;

            this.sbShow.Completed += sbShow_Completed;
            this.sbHide.Completed += sbHide_Completed;


            Brush bgBrush = Brushes.Transparent;

            for (int i = 0; i < maxCount; i++)
            {
                Rectangle rec = new Rectangle();
                rec.RadiusX = 5;
                rec.RadiusY = 5;
                rec.Fill = bgBrush;
                rec.Margin = this.margin;
                rec.TouchUp += rec_TouchUp;
                int row = this.GetRowNumber(i);
                int column = this.GetColumnNumber(i);

                Grid.SetRow(rec, row);
                Grid.SetColumn(rec, column);

                this.gdBack.Children.Add(rec);
            }

            this.Load();
        }

        public void OpenNewDrawing()
        {
            this.gdDrawing.Children.Clear();
            this.currentDb = new Drawingboard();
            this.currentDb.CloseEvent += db_CloseEvent;
            this.gdDrawing.Children.Add(this.currentDb);

            this.gdDrawing.Visibility = System.Windows.Visibility.Visible;
            this.sbShow.Begin();
        }

        #region Menu handle
        void sbHide_Completed(object sender, EventArgs e)
        {

        }

        void sbShow_Completed(object sender, EventArgs e)
        {

        }

        void kMenu_FireEvent(object sender, KComponents.KMenuArgs e)
        {
            if (e.Key == Config.KEYOPEN)
            {
                this.OpenNewDrawing();
            }

        }


        void rec_TouchUp(object sender, TouchEventArgs e)
        {
            Rectangle rec = sender as Rectangle;
            if (rec.Tag != null)
            {
                string id = rec.Tag.ToString();

                this.currentDb = new Drawingboard();
                this.currentDb.CloseEvent += this.db_CloseEvent;
                this.currentDb.Load(this.boards[id]);
                this.gdDrawing.Children.Clear();
                this.gdDrawing.Children.Add(this.currentDb);
                this.gdDrawing.Visibility = System.Windows.Visibility.Visible;
                this.sbShow.Begin();
            }

        }


        void db_CloseEvent(object sender, EventArgs e)
        {
            try
            {
                this.gdDrawing.Visibility = System.Windows.Visibility.Collapsed;

                Image img = this.currentDb.GetSnapshot();
                Rectangle rec = this.gdBack.Children[0] as Rectangle;
                img.Width = rec.ActualWidth;
                img.Height = rec.ActualHeight;

                DrawingboardData data = this.currentDb.GetModel();
                this.AddToCache(this.currentDb.ID, data);

                this.Add(img, this.currentDb.ID);
                RenderTargetBitmap bitmapSource = img.Source as RenderTargetBitmap;
                this.Save(data, bitmapSource);

                if (this.CloseEvent != null)
                {
                    this.CloseEvent(this, e);
                }
            }
            catch (Exception ex)
            {
            }

        }

        #endregion

        private void Load()
        {
            if (this.isLoadFiles == false)
            {
                this.isLoadFiles = true;
                string fileDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, StrokeBuilder.FileDirectory);
                if (Directory.Exists(fileDirectory) == false)
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                string snapShotDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, StrokeBuilder.SnapshotDirectory);
                if (Directory.Exists(snapShotDirectory) == false)
                {
                    Directory.CreateDirectory(snapShotDirectory);
                }
                else
                {
                    string[] files = Directory.GetFiles(snapShotDirectory);
                    IEnumerable<string> orderFiles = files.OrderBy(item => item);
                    char[] split = { '\\' };
                    foreach (string file in orderFiles)
                    {

                        string[] filepath = file.Split(split);
                        string fileName = filepath[filepath.Length - 1];
                        int index = fileName.IndexOf('.');
                        string fileId = fileName.Substring(0, index);
                        Image img = new Image();
                        img.SetImage(file, true);

                        DrawingboardData data = StrokeBuilder.Load(fileId);
                        if (data != null)
                        {
                            this.boards.Add(fileId, data);

                            this.Add(img, fileId);
                        }

                    }
                }
            }

        }

        private void Add(Image img, string id)
        {
            int oldIndex = -1;
            for (int i = 0; i < this.gdImageWall.Children.Count; i++)
            {
                Image ui = this.gdImageWall.Children[i] as Image;

                if (ui != null && ui.Tag != null)
                {
                    string oldId = ui.Tag.ToString();
                    if (oldId == id)
                    {
                        oldIndex = i;
                        break;
                    }
                }
            }
            if (oldIndex >= 0)
            {
                Image ui = this.gdImageWall.Children[oldIndex] as Image;
                ui.Source = img.Source;
                img.Tag = ui.Tag;
            }
            else
            {
                if (pictures.Count == maxCount)
                {
                    string removeId = this.pictures[0].Tag.ToString();
                    pictures.RemoveAt(0);
                    this.boards.Remove(removeId);
                    this.gdImageWall.Children.RemoveAt(0);

                    for (int i = 0; i < pictures.Count; i++)
                    {
                        int row = this.GetRowNumber(i);
                        int column = this.GetColumnNumber(i);

                        Grid.SetRow(this.pictures[i], row);
                        Grid.SetColumn(this.pictures[i], column);

                        Rectangle backItem = this.gdBack.Children[i] as Rectangle;
                        backItem.Tag = this.pictures[i].Tag.ToString();
                    }
                }

                pictures.Add(img);

                img.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                img.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                img.Margin = this.margin;
                img.Tag = id;
                img.IsHitTestVisible = false;
                int newRow = this.GetRowNumber(this.pictures.Count - 1);
                int newColumn = this.GetColumnNumber(this.pictures.Count - 1);
                Grid.SetRow(img, newRow);
                Grid.SetColumn(img, newColumn);

                this.gdImageWall.Children.Add(img);


                Rectangle rec = (Rectangle)this.gdBack.Children[this.pictures.Count - 1];
                rec.Fill = new SolidColorBrush(Colors.Green);
                rec.Tag = id;
            }

        }

        private DrawingVisual CreateDrawingVisual(FrameworkElement visual, double width, double height)
        {
            var drawingVisual = new DrawingVisual();
            // open the Render of the DrawingVisual  
            using (var dc = drawingVisual.RenderOpen())
            {
                var vb = new VisualBrush(visual) { Stretch = Stretch.None };
                var rectangle = new Rect
                {
                    X = 0,
                    Y = 0,
                    Width = width,
                    Height = height,
                };

                // draw the white background  
                dc.DrawRectangle(Brushes.White, null, rectangle);
                // draw the visual  
                dc.DrawRectangle(vb, null, rectangle);
            }
            return drawingVisual;
        }

        private void AddToCache(string id, DrawingboardData data)
        {
            if (this.boards.ContainsKey(id))
            {
                this.boards[id] = data;
            }
            else
            {
                this.boards.Add(id, data);
            }
        }

        int GetRowNumber(int index)
        {
            return index / layoutKey;
        }

        int GetColumnNumber(int index)
        {
            return index % layoutKey;
        }



        void Save(DrawingboardData content, RenderTargetBitmap snapShotSource)
        {
            string fileName = content.ID;
            string snapShotAddress = StrokeBuilder.GetSnapShotAddresss(fileName);
            try
            {
                snapShotSource.Save(snapShotAddress);
                Thread saveThread = new Thread(this.SaveContentInBackground);
                saveThread.Start(content);

            }
            catch (Exception ex)
            {
                return;
            }
        }

        void SaveContentInBackground(object o)
        {
            DrawingboardData data = o as DrawingboardData;
            if (data != null)
            {
                bool result = StrokeBuilder.Save(data);
                if (result)
                {

                }
            }
        }

        class SaveContentParamater
        {
            public DrawingboardData Content { set; get; }
            public double Width { get; set; }
            public double Height { set; get; }
            public RenderTargetBitmap SnapShot { get; set; }
            public string FilePath { get; set; }
        }

    }
}
