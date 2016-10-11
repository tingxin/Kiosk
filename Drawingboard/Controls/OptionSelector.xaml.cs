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

namespace Drawingboard.Controls
{
    /// <summary>
    /// Interaction logic for OptionSelector.xaml
    /// </summary>
    public partial class OptionSelector : UserControl
    {
        internal event EventHandler<OptionArg> SelectedEvent;

        public OptionSelector()
        {
            InitializeComponent();
            this.Width = 450;
            this.Height = 550;
            this.btnCancel.Click += btnCancel_Click;
            for (int i = 0; i < this.grid.Children.Count; i++)
            {
                Button btn = this.grid.Children[i] as Button;
                if (btn != null)
                {
                    btn.Click += btn_Click;
                }
            }
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedEvent != null)
            {
                this.SelectedEvent(this, new OptionArg(AnimationOption.None));
                this.Hide();
            }
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {

            if (this.SelectedEvent != null)
            {
                Button btn = sender as Button;
                int index = this.grid.Children.IndexOf(btn);
                AnimationOption option = (AnimationOption)index;
                this.SelectedEvent(this, new OptionArg(option));
                this.Hide();
            }
        }

        public void Show()
        {
            Storyboard sb = this.Resources["ShowSb"] as Storyboard;
            if (sb != null)
            {
                this.Visibility = System.Windows.Visibility.Visible;
                sb.Begin();
            }
        }

        void Hide()
        {
            Storyboard sb = this.Resources["HideSb"] as Storyboard;
            if (sb != null)
            {
                sb.Completed += sb_Completed;
                sb.Begin();
            }
        }

        void sb_Completed(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }


    }

    public enum AnimationOption
    {
        RotateLeft,
        RotateRight,
        SqueezeHorizontal,
        SqueeezeVertical,
        SwingTop,
        SwingBottom,
        SwingLeft,
        SwingRight,
        MoveTop,
        MoveBottom,
        MoveLeft,
        MoveRight,
        Jump,
        Shake,
        Flash,
        None
    }

    internal class OptionArg : EventArgs
    {
        public AnimationOption Option { private set; get; }
        public OptionArg(AnimationOption op)
            : base()
        {
            this.Option = op;
        }
    }
}
