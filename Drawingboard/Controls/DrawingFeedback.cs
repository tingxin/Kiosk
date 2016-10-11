using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using KComponents;

namespace Drawingboard.controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Drawingboard.controls.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Drawingboard.controls.Controls;assembly=Drawingboard.controls.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:DrawingFeedback/>
    ///
    /// </summary>
    [TemplatePart(Name = "imgIcon", Type = typeof(Image))]
    internal class DrawingFeedback : Control
    {
        #region Private
        private Image imgIcon;
        #endregion

        static DrawingFeedback()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DrawingFeedback), new FrameworkPropertyMetadata(typeof(DrawingFeedback)));
        }

        public DrawingFeedback()
            : base()
        {
            {

            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.imgIcon = this.GetTemplateChild("imgIcon") as Image;
            this.InitImageIcon(this.FeedbackType);
        }

        #region Public Properties
        [Category("DrawingFeedback")]
        public PenMenuCommand FeedbackType
        {
            get
            {
                return (PenMenuCommand)GetValue(FeedbackTypeProperty);
            }
            set
            {
                SetValue(FeedbackTypeProperty, value);
            }
        }

        public static readonly DependencyProperty FeedbackTypeProperty = DependencyProperty.Register("FeedbackType", typeof(PenMenuCommand), typeof(DrawingFeedback), new PropertyMetadata(PenMenuCommand.MarkerWhite, new PropertyChangedCallback(OnFeedbackTypePropertyChanged)));

        static void OnFeedbackTypePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ex)
        {
            if (ex.NewValue != null)
            {
                DrawingFeedback self = o as DrawingFeedback;

                self.InitImageIcon((PenMenuCommand)ex.NewValue);
            }
        }

        void InitImageIcon(PenMenuCommand type)
        {
            if (this.imgIcon != null)
            {
                BitmapImage bitImage;
                switch (type)
                {
                    case PenMenuCommand.HighlightPurple:
                    case PenMenuCommand.MarkerPurple:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/purplePenFeedback.png", UriKind.RelativeOrAbsolute));
                        break;
                    case PenMenuCommand.HightCyan:
                    case PenMenuCommand.MarkerCyan:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/cyanPenFeedback.png", UriKind.RelativeOrAbsolute));
                        break;
                    case PenMenuCommand.HighlightRed:
                    case PenMenuCommand.MarkerRed:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/redPenFeedback.png", UriKind.RelativeOrAbsolute));
                        break;
                    case PenMenuCommand.HightYellow:
                    case PenMenuCommand.MarkerYellow:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/yellowPenFeedback.png", UriKind.RelativeOrAbsolute));
                        break;
                    case PenMenuCommand.MarkerWhite:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/greyPenFeedback.png", UriKind.RelativeOrAbsolute));
                        break;
                    case PenMenuCommand.Eraser:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/eraserFeedback.png", UriKind.RelativeOrAbsolute));
                        break;
                    case PenMenuCommand.LassoSelected:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/lassoFeedback.png", UriKind.RelativeOrAbsolute));
                        break;
                    case PenMenuCommand.Panning:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/panning.png", UriKind.RelativeOrAbsolute));
                        break;
                    default:
                        bitImage = new BitmapImage(new Uri("/Drawingboard.controls;component/Assets/greyPenFeedback.png", UriKind.RelativeOrAbsolute));

                        break;
                }
                this.imgIcon.Source = bitImage;

            }
        }


        #endregion

    }
}
