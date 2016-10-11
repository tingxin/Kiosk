using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Awesomium.Windows.Controls;

namespace Kiosk.Browser
{
    /// <summary>
    /// Interaction logic for Wall.xaml
    /// </summary>
    public partial class Wall : UserControl
    {
        WebControl web;
        public Wall()
        {
            InitializeComponent();

            if (web == null)
            {
                web = new WebControl();
                Panel.SetZIndex(web, 0);
                this.gdRoot.Children.Add(web);
            }

        }

        public void SetAddress(string uri)
        {
            try
            {
                Uri address = new Uri(uri, UriKind.Absolute);
                this.web.Source = address;

            }
            catch (Exception ex)
            {
                return;
            }


        }

        private Image GetSnapshot()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(this.gdRoot) { Stretch = Stretch.None };
                context.DrawRectangle(brush, null, new Rect(0, 0, this.gdRoot.ActualWidth, this.gdRoot.ActualHeight));
                context.Close();
            }

            //dpi可以自己设定   // 获取dpi方法：PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)this.gdRoot.ActualWidth, (int)this.gdRoot.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            Image img = new Image();
            img.BeginInit();
            img.Source = bitmap;
            img.EndInit();

            return img;
        }

        public void ShowSnapShot()
        {
            Image img = this.GetSnapshot();
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            this.gdSnapShot.Children.Add(img);
        }

        public void ShowUI()
        {
            this.gdSnapShot.Children.Clear();
        }
    }
}
