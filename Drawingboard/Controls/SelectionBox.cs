using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using KComponents;
using System.Linq;
using System.Windows.Media;

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
    ///     <MyNamespace:SelectionBox/>
    ///
    /// </summary>
    [TemplatePart(Name = "gdMoveIndicator", Type = typeof(Grid))]
    [TemplatePart(Name = "pathIndicator", Type = typeof(Path))]
    [TemplatePart(Name = "border", Type = typeof(Border))]
    [TemplatePart(Name = "recMain", Type = typeof(Rectangle))]
    [TemplatePart(Name = "recBack", Type = typeof(Rectangle))]
    public class SelectionBox : Control
    {

        Grid gdMoveIndicator;
        Path pathIndicator;
        Border border;
        Rectangle recMain;
        Rectangle recBack;
        static SelectionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectionBox), new FrameworkPropertyMetadata(typeof(SelectionBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.gdMoveIndicator = this.GetTemplateChild("gdMoveIndicator") as Grid;
            this.pathIndicator = this.GetTemplateChild("pathIndicator") as Path;
            this.border = this.GetTemplateChild("border") as Border;
            this.recMain = this.GetTemplateChild("recMain") as Rectangle;
            this.recBack = this.GetTemplateChild("recBack") as Rectangle;
            if (borderColorBrush != null)
            {
                this.recMain.Stroke = borderColorBrush;
                this.recBack.Stroke = borderColorBrush;
            }

            this.gdMoveIndicator.SetVisible(this.IndicatorVisiable);

            this.SetNewValue(this.IsMovable);

            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                this.IsManipulationEnabled = true;
                this.pathIndicator.Visibility = Visibility.Collapsed;
                this.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(SelectionBox_ManipulationStarted);
                this.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(SelectionBox_ManipulationCompleted);
            }

            if (this.border != null)
            {
                Storyboard sb = this.border.Resources["flashSb"] as Storyboard;
                sb.Begin();
            }

        }

        void animationButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.AnimationButtonFireEvent != null)
            {
                this.AnimationButtonFireEvent(this, e);
            }
        }

        void SelectionBox_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            this.pathIndicator.Visibility = Visibility.Collapsed;
        }

        void SelectionBox_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            this.pathIndicator.Visibility = Visibility.Visible;
        }


        #region Public Propertes
        public event EventHandler AnimationButtonFireEvent;

        [Category("SelectionBox")]
        public bool IsMovable
        {
            get
            {
                return (bool)this.GetValue(SelectionBox.IsMovableProperty);
            }
            set
            {
                SetValue(SelectionBox.IsMovableProperty, value);
            }
        }

        public static readonly DependencyProperty IsMovableProperty = DependencyProperty.Register("IsMovable", typeof(bool), typeof(SelectionBox), new PropertyMetadata(true, new PropertyChangedCallback(OnIsMovableChanged)));

        static void OnIsMovableChanged(DependencyObject o, DependencyPropertyChangedEventArgs ex)
        {
            SelectionBox self = o as SelectionBox;
            if (self != null && ex.NewValue != null)
            {
                if (ex.NewValue != ex.OldValue)
                {
                    self.SetNewValue((bool)ex.NewValue);
                }
            }
        }

        void SetNewValue(bool newValue)
        {
            this.gdMoveIndicator.SetVisible(newValue);
        }

        public int TouchCount
        {
            get
            {
                return this.TouchesOver.Count();
            }
        }

        Brush borderColorBrush = null;
        public Color BorderColor
        {
            set
            {
                borderColorBrush = new SolidColorBrush(value);
                if (this.recMain != null)
                {
                    this.recMain.Stroke = borderColorBrush;
                    this.recBack.Stroke = borderColorBrush;
                }
            }
        }

        bool indicatorVisiable = true;
        public bool IndicatorVisiable
        {

            get
            {
                return this.indicatorVisiable;
            }
            set
            {
                this.indicatorVisiable = value;
                if (this.gdMoveIndicator != null)
                {
                    this.gdMoveIndicator.SetVisible(this.indicatorVisiable);
                }
            }
        }
        #endregion
    }
}
