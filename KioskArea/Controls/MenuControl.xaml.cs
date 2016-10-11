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

namespace KioskArea.Controls
{
    /// <summary>
    /// Interaction logic for MenuControl.xaml
    /// </summary>
    public partial class MenuControl : UserControl
    {

        bool isOpen = false;
        public MenuControl()
        {
            InitializeComponent();

            this.Items = new List<MenuControlItem>();
        }


        public Brush ItemColor { get; set; }
        public MenuControlItem Center { set; get; }
        public List<MenuControlItem> Items { private set; get; }

        public event EventHandler<KioskMenuArgs> FireEvent;

        public void Render()
        {
            Point centerP = new Point(this.ActualWidth / 2, 0);
            double itemRadius = this.ActualWidth / 10;
            Brush foreground = this.ItemColor == null ? Brushes.BurlyWood : this.ItemColor;
            EllipseGeometry clip = new EllipseGeometry(new Point(itemRadius, itemRadius), itemRadius, itemRadius);

            Grid gdOpen = this.GetItemContainer(itemRadius, clip, foreground);
            gdOpen.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            gdOpen.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            gdOpen.TouchDown += gdOpen_TouchDown;
            gdOpen.TouchUp += gdOpen_TouchUp;
            this.gdRoot.Children.Add(gdOpen);

            if (this.Items.Count > 1)
            {
                this.cvsRoot.Children.Clear();

                double radius = itemRadius * 4;

                double angle = Math.PI / (this.Items.Count - 1);

                for (int i = 0; i < this.Items.Count; i++)
                {
                    double itemAngle = angle * i + Math.PI;
                    double xpos = radius * Math.Cos(itemAngle);
                    double ypos = radius * Math.Sin(itemAngle);
                    Grid gdItem = GetItemContainer(itemRadius, clip, foreground);
                    gdItem.Tag = this.Items[i];
                    try
                    {
                        Image img = new Image();
                        BitmapImage bt = new BitmapImage(new Uri(this.Items[i].Icon));
                        img.BeginInit();
                        img.Source = bt;
                        img.EndInit();

                        img.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        img.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        img.Stretch = Stretch.Uniform;
                        img.IsHitTestVisible = false;
                        gdItem.Children.Add(img);
                    }
                    catch
                    {

                    }



                    gdItem.TouchDown += gdItem_TouchDown;
                    gdItem.TouchUp += gdItem_TouchUp;

                    Canvas.SetLeft(gdItem, centerP.X + xpos - itemRadius);
                    Canvas.SetTop(gdItem, centerP.Y - ypos);

                    this.cvsRoot.Children.Add(gdItem);

                }
            }

        }

        void gdOpen_TouchUp(object sender, TouchEventArgs e)
        {
            Grid gd = sender as Grid;
            gd.Background = this.ItemColor == null ? Brushes.BurlyWood : this.ItemColor;

            if (this.isOpen == false)
            {
                this.cvsRoot.Visibility = System.Windows.Visibility.Visible;
                Storyboard sb = this.Resources["show"] as Storyboard;
                sb.Begin();
            }
            else
            {
                Storyboard sb = this.Resources["hide"] as Storyboard;
                sb.Completed += Hide_Completed;
                sb.Begin();
            }
            this.isOpen = !this.isOpen;
        }

        void Hide_Completed(object sender, EventArgs e)
        {
            this.cvsRoot.Visibility = System.Windows.Visibility.Collapsed; ;
        }

        void gdOpen_TouchDown(object sender, TouchEventArgs e)
        {
            Grid gd = sender as Grid;
            gd.Background = Brushes.Silver;
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
            gd.Background = this.ItemColor == null ? Brushes.BurlyWood : this.ItemColor;


            if (this.FireEvent != null)
            {
                MenuControlItem item = (MenuControlItem)gd.Tag;
                KioskMenuArgs arg = new KioskMenuArgs(item.Id);
                this.FireEvent(this, arg);
            }
        }

        void gdItem_TouchDown(object sender, TouchEventArgs e)
        {
            Grid gd = sender as Grid;
            gd.Background = Brushes.Silver;
        }

    }



    public class MenuControlItem
    {
        public MenuControlItem(string id, string title)
        {
            this.Id = id;
            this.Title = title;
        }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Icon { get; set; }

    }

    public class KioskMenuArgs : EventArgs
    {
        public string AppId { get; private set; }
        public KioskMenuArgs(string id)
        {
            this.AppId = id;
        }
    }
}
