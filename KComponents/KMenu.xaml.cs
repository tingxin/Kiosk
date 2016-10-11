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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KComponents
{
    /// <summary>
    /// Interaction logic for KMenu.xaml
    /// </summary>
    public partial class KMenu : UserControl
    {
        bool isOpen = false;
        Grid gdOpen;
        List<Storyboard> _rotShowStbs = new List<Storyboard>();
        List<Storyboard> _rotHideStbs = new List<Storyboard>();

        public KMenu()
        {
            InitializeComponent();
            this.RenderOrientation = LayoutOrirentation.Up;
            this.Items = new List<KMenuItem>();
        }


        public Brush ItemColor { get; set; }
        public Brush CenterColor { get; set; }
        public List<KMenuItem> Items { private set; get; }
        public LayoutType RenderLayout { set; get; }
        public LayoutOrirentation RenderOrientation { set; get; }
        public event EventHandler<KMenuArgs> FireEvent;

        public void Render()
        {
            Point rendreSize = GetRenderSize();
            double yCenter = this.RenderOrientation == LayoutOrirentation.Bottom ? 0 : rendreSize.Y;
            Point centerP = new Point(rendreSize.X / 2, yCenter);
            double itemRadius = rendreSize.X / 10;
            double centerRadius = itemRadius * 0.84;
            Brush centerColor = this.CenterColor == null ? Brushes.BurlyWood : this.CenterColor;

            EllipseGeometry centerClip = new EllipseGeometry(new Point(centerRadius, centerRadius), centerRadius, centerRadius);

            this.gdOpen = this.GetItemContainer(centerRadius, centerClip, centerColor);
            gdOpen.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            gdOpen.VerticalAlignment = this.RenderOrientation == LayoutOrirentation.Bottom ? VerticalAlignment.Top : VerticalAlignment.Bottom;
            KMenuItem cancelItem = new KMenuItem("cancel", "cancel");
            gdOpen.Tag = cancelItem;
            gdOpen.MouseLeftButtonUp += gdOpen_MouseLeftButtonUp;
            gdOpen.Loaded += gdOpen_Loaded;
            Image centerIcon = this.GetItemIcon(cancelItem.Icon + "_on.png");
            gdOpen.Children.Add(centerIcon);

            this.gdRoot.Children.Add(gdOpen);

            if (this.Items.Count > 1)
            {
                EllipseGeometry clip = new EllipseGeometry(new Point(itemRadius, itemRadius), itemRadius, itemRadius);

                Brush foreground = this.ItemColor == null ? Brushes.BurlyWood : this.ItemColor;
                this.cvsRoot.Children.Clear();

                double radius = itemRadius * 4;

                bool isOdd = this.Items.Count % 2 == 1;
                double angle = Math.PI / (isOdd ? (this.Items.Count - 1) : (this.Items.Count + 1));
                for (int i = 0; i < this.Items.Count; i++)
                {
                    int angleIndex = isOdd ? i : (i + 1);
                    double itemAngle = angle * angleIndex + Math.PI;
                    double xpos = radius * Math.Cos(itemAngle);
                    double ypos = radius * Math.Sin(itemAngle);
                    Grid gdItem = GetItemContainer(itemRadius, clip, foreground);


                    gdItem.Tag = this.Items[i];

                    Image img = GetItemIcon(this.Items[i].Icon + "_on.png");
                    gdItem.Children.Add(img);

                    //TextBlock txt = new TextBlock();
                    //txt.Text = this.Items[i].Title;
                    //txt.Foreground = Brushes.White;
                    //txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    //txt.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    //txt.Margin = new Thickness(0, 0, 0, 10);
                    //gdItem.Children.Add(txt);

                    //gdItem.TouchDown += gdItem_TouchDown;
                    //gdItem.TouchUp += gdItem_TouchUp;
                    //gdItem.TouchLeave += gdItem_TouchLeave;

                    gdItem.MouseLeftButtonDown += gdItem_MouseLeftButtonDown;
                    gdItem.MouseLeftButtonUp += gdItem_MouseLeftButtonUp;
                    gdItem.MouseLeave += gdItem_MouseLeave;

                    Canvas.SetLeft(gdItem, centerP.X + xpos - itemRadius);
                    double yPosWithRender = this.RenderOrientation == LayoutOrirentation.Bottom ? centerP.Y - ypos : centerP.Y + ypos - gdItem.Height;
                    Canvas.SetTop(gdItem, yPosWithRender);

                    this.cvsRoot.Children.Add(gdItem);
                    _rotShowStbs.Add(BuildRotAnimation(gdItem, -240, 0));
                    _rotHideStbs.Add(BuildRotAnimation(gdItem, 0, -240));
                }
            }

        }


        private Storyboard BuildRotAnimation(FrameworkElement fe, double from, double to)
        {
            var rot = new RotateTransform(45);
            fe.RenderTransformOrigin = new Point(0.5, 0.5);
            fe.RenderTransform = rot;

            DoubleAnimation da = new DoubleAnimation(to, new Duration(TimeSpan.FromSeconds(0.25))) { From = from };
            Storyboard.SetTarget(da, fe);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.Angle"));

            Storyboard stb = new Storyboard();
            stb.Children.Add(da);
            return stb;
        }

        void gdOpen_Loaded(object sender, RoutedEventArgs e)
        {
            Point pos = this.cvsMarkLayout.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
            this.cvsMark.Width = Application.Current.MainWindow.ActualWidth;
            this.cvsMark.Height = Application.Current.MainWindow.ActualHeight;
            Canvas.SetLeft(this.cvsMark, 0 - pos.X);
            Canvas.SetTop(this.cvsMark, 0 - pos.Y);

            this.cvsMark.TouchUp += cvsMark_TouchUp;
        }

        void cvsMark_TouchUp(object sender, TouchEventArgs e)
        {
            this.TouchUpCenter(this.gdOpen);
        }

        private Image GetItemIcon(string path)
        {
            Image img = new Image();
            img.SetImage(path);

            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            img.Stretch = Stretch.Uniform;
            img.IsHitTestVisible = false;
            return img;
        }

        private Point GetRenderSize()
        {
            double renderWidth = 300;
            double renderHeight = 300;
            if (double.IsNaN(this.Width) == false && double.IsNaN(this.Height) == false)
            {
                renderWidth = this.Width;
                renderHeight = this.Height;

            }
            else
            {
                if (this.ActualWidth > 0)
                {
                    renderWidth = this.ActualWidth;
                }

                if (this.ActualHeight > 0)
                {
                    renderHeight = this.ActualHeight;
                }
            }
            return new Point(renderWidth, renderHeight);
        }



        private void ChangeStatus(Grid gd)
        {

            if (this.isOpen == false)
            {
                this.cvsRoot.Visibility = System.Windows.Visibility.Visible;
                this.cvsMark.Visibility = System.Windows.Visibility.Visible;
                this.RevertImage(gd, "_off.png");
                Storyboard sb = this.Resources["show"] as Storyboard;
                sb.Begin();
                _rotShowStbs.ForEach((i) => { i.Begin(); });
            }
            else
            {
                this.cvsMark.Visibility = System.Windows.Visibility.Collapsed;
                this.RevertImage(gd, "_on.png");
                Storyboard sb = this.Resources["hide"] as Storyboard;
                sb.Completed += Hide_Completed;
                sb.Begin();
                _rotHideStbs.ForEach((i) => { i.Begin(); });
            }
            this.isOpen = !this.isOpen;
        }


        void Hide_Completed(object sender, EventArgs e)
        {

            this.cvsRoot.Visibility = System.Windows.Visibility.Collapsed;
        }

        void gdOpen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Grid gd = sender as Grid;
            TouchUpCenter(gd);
        }



        private void TouchUpCenter(Grid gd)
        {
            this.ChangeStatus(gd);

        }





        private Grid GetItemContainer(double itemRadius, EllipseGeometry clip, Brush foreground)
        {
            Grid gdItem = new Grid();
            gdItem.Width = gdItem.Height = itemRadius * 2;
            gdItem.Background = foreground;
            gdItem.Clip = clip;
            return gdItem;
        }

        void gdItem_TouchUp(object sender, TouchEventArgs e)
        {
            Grid gd = sender as Grid;
            this.RevertImage(gd, "_on.png");
            KMenuItem item = gd.Tag as KMenuItem;
            this.ChangeStatus(this.gdOpen);
            if (this.FireEvent != null)
            {
                KMenuArgs arg = new KMenuArgs(item.Id);
                this.FireEvent(this, arg);
            }
        }

        void gdItem_TouchLeave(object sender, TouchEventArgs e)
        {
            Grid gd = sender as Grid;
            this.RevertImage(gd, "_on.png");
        }


        void gdItem_TouchDown(object sender, TouchEventArgs e)
        {
            Grid gd = sender as Grid;
            this.RevertImage(gd, "_off.png");
        }

        void gdItem_MouseLeave(object sender, MouseEventArgs e)
        {
            this.gdItem_TouchLeave(sender, null);
        }

        void gdItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.gdItem_TouchUp(sender, null);
        }

        void gdItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.gdItem_TouchDown(sender, null);
        }

        private void RevertImage(Grid gd, string key)
        {
            KMenuItem item = gd.Tag as KMenuItem;
            foreach (UIElement ui in gd.Children)
            {
                if (ui is Image)
                {
                    Image img = ui as Image;
                    img.SetImage(item.Icon + key);
                    break;
                }
            }
        }


    }



    public class KMenuItem
    {
        public KMenuItem(string id, string title)
        {
            this.Id = id;
            this.Title = title;
            this.Icon = AppDomain.CurrentDomain.BaseDirectory + "Assets\\" + title.ToLower();
        }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Icon { get; set; }
    }

    public class KMenuArgs : EventArgs
    {
        public string Key { get; private set; }
        public KMenuArgs(string key)
        {
            this.Key = key;
        }
    }

    public enum LayoutOrirentation
    {
        Up,
        Bottom
    }

    public enum LayoutType
    {
        Odd,
        Even
    }
}

