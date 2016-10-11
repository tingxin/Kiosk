using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace KComponents
{
    public static class StoryboardFactory
    {
        public const string Left = "(Canvas.Left)";
        public const string Top = "(Canvas.Top)";

        public const string Width = "(FrameworkElement.Width)";
        public const string Height = "(FrameworkElement.Height)";

        public const string Opacity = "(UIElement.Opacity)";

        public const string ScareX = "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)";
        public const string ScareY = "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)";
        public const string TranslateX = "(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.X)";
        public const string TranslateY = "(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)";

        public static Storyboard CreateSimpleDoubleAnimationStoryboard(UIElement targetElement, double durationMilliseconds, double? from, double? to, string propertyPath)
        {
            DoubleAnimation myDoubleAnimation = StoryboardFactory.CreateDoubleAnimation(targetElement, durationMilliseconds, from, to, propertyPath);
            Storyboard sb = new Storyboard();
            sb.Children.Add(myDoubleAnimation);

            return sb;
        }

        public static DoubleAnimation CreateDoubleAnimation(UIElement targetElement, double durationMilliseconds, double? from, double? to, string propertyPath, double beginTime = 0)
        {
            Duration duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds));

            // Create two DoubleAnimations and set their properties.
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            if (beginTime > 0)
            {
                myDoubleAnimation.BeginTime = TimeSpan.FromMilliseconds(beginTime);
            }

            myDoubleAnimation.Duration = duration;
            Storyboard.SetTarget(myDoubleAnimation, targetElement);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(propertyPath));
            if (from.HasValue)
            {
                myDoubleAnimation.From = from.Value;
            }
            if (to.HasValue)
            {
                myDoubleAnimation.To = to.Value;
            }
            return myDoubleAnimation;
        }



    }
}
