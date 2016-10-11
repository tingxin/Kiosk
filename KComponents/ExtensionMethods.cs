using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Net;
using System.Threading;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Animation;
using SpringRoll.Utility;

namespace KComponents
{
    public static class ExtensionMethods
    {
        #region Memebers
        static Canvas maskPanel = null;
        static EventHandler<TouchEventArgs> eventHandel = null;
        static Storyboard showSb = null;
        static Storyboard closeSb = null;
        #endregion

        #region Drag
        /// <summary>
        /// Drag in canvas
        /// </summary>
        /// <param name="dragTarget">drag target</param>
        /// <param name="dragElement">drag element</param>
        /// <param name="bound">bound</param>
        /// <returns></returns>
        public static bool BindDrag(this FrameworkElement dragTarget, FrameworkElement dragElement, DragBound bound, Action<Point, bool> positonChangeCallback = null)
        {
            bool result = false;
            if (dragTarget.Parent != null && (dragTarget.Parent as Canvas) != null)
            {
                Canvas moveCvs = dragTarget.Parent as Canvas;

                Point positionOnElement = new Point();
                bool isDrag = false;
                int deviceId = 0;
                DragBoundCheck check = new DragBoundCheck(dragTarget, bound);
                dragElement.PreviewTouchDown += delegate(object sender, TouchEventArgs e)
                {
                    IDrag dragObj = dragTarget as IDrag;
                    if (dragObj != null && dragObj.IsDrag == false)
                    {
                        return;
                    }
                    if (isDrag == false)
                    {
                        deviceId = e.TouchDevice.Id;
                        positionOnElement = e.GetTouchPoint(dragTarget).Position;
                        isDrag = true;
                        dragElement.CaptureTouch(e.TouchDevice);
                    }
                };
                dragElement.PreviewTouchMove += delegate(object sender, TouchEventArgs e)
                {
                    if (isDrag && e.TouchDevice.Id == deviceId)
                    {
                        SetNewMovPosition(dragTarget, positonChangeCallback, moveCvs, positionOnElement, false, e);
                    }
                };


                dragElement.PreviewTouchUp += delegate(object sender, TouchEventArgs e)
                {
                    if (isDrag && deviceId == e.TouchDevice.Id)
                    {
                        isDrag = false;
                        SetNewMovPosition(dragTarget, positonChangeCallback, moveCvs, positionOnElement, true, e);
                        dragElement.ReleaseTouchCapture(e.TouchDevice);
                    }
                };
                result = true;
            }
            return result;
        }

        private static void SetNewMovPosition(FrameworkElement dragTarget, Action<Point, bool> positonChangeCallback, Canvas moveCvs, Point positionOnElement, bool isEndMove, TouchEventArgs e)
        {
            Point targetOnParentPosition = e.GetTouchPoint(moveCvs).Position;
            Debug.WriteLine(string.Format("Drag New Position is ({0},{1})", targetOnParentPosition.X, targetOnParentPosition.Y));
            Point newPos = targetOnParentPosition.Sub(positionOnElement);

            dragTarget.SetPosition(newPos);

            if (positonChangeCallback != null)
            {
                Point relativePoin = new Point(newPos.X / moveCvs.ActualWidth, newPos.Y / moveCvs.ActualHeight);
                positonChangeCallback(relativePoin, isEndMove);
            }
        }


        public static bool BindDrag(this FrameworkElement dragTarget, FrameworkElement dragElement, Action<Point, bool> positonChangeCallback = null)
        {
            DragBound bound = new DragBound();
            return BindDrag(dragTarget, dragElement, bound, positonChangeCallback);
        }

        public static bool BindDrag(this FrameworkElement dragTarget, DragBound bound, Action<Point, bool> positonChangeCallback = null)
        {

            return BindDrag(dragTarget, dragTarget, bound, positonChangeCallback);
        }

        public static bool BindDrag(this FrameworkElement dragTarget, Action<Point, bool> positonChangeCallback = null)
        {
            DragBound bound = new DragBound();
            return BindDrag(dragTarget, bound, positonChangeCallback);
        }
        #endregion

        #region Positon
        public static void SetLeft(this FrameworkElement element, double leftPosition)
        {
            element.SetValue(Canvas.LeftProperty, leftPosition);
        }

        public static double GetLeft(this FrameworkElement element)
        {
            return Canvas.GetLeft(element);
        }
        public static void SetTop(this FrameworkElement element, double topPosition)
        {
            element.SetValue(Canvas.TopProperty, topPosition);
        }

        public static double GetTop(this FrameworkElement element)
        {
            return Canvas.GetTop(element);
        }

        public static void SetPosition(this FrameworkElement element, Point position)
        {
            element.SetLeft(position.X);
            element.SetTop(position.Y);
        }

        public static void SetPosition(this FrameworkElement element, double x, double y)
        {
            element.SetLeft(x);
            element.SetTop(y);
        }
        #endregion

        #region Point
        public static Point Sub(this Point selfPoint, Point subPoint)
        {
            Point result = new Point(selfPoint.X - subPoint.X, selfPoint.Y - subPoint.Y);
            return result;
        }

        public static double Length(this Point selfPoint)
        {

            return Math.Sqrt(selfPoint.X * selfPoint.X + selfPoint.Y * selfPoint.Y);
        }

        public static Point Add(this Point selfPoint, Point addPoint)
        {
            Point result = new Point(selfPoint.X + addPoint.X, selfPoint.Y + addPoint.Y);
            return result;
        }
        #endregion

        #region Visible
        public static void SetVisible(this UIElement target, bool isVisible)
        {
            if (target != null)
            {
                target.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static void SetZIndex(this UIElement target, int index)
        {
            if (target != null)
            {
                Panel.SetZIndex(target, index);
            }
        }
        #endregion

        #region Blong to Area
        public static bool IsInElementArea(this FrameworkElement target, Point pos)
        {
            if (pos.X < target.ActualWidth && pos.Y < target.ActualHeight && pos.X > 0 && pos.Y > 0)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Check if the positon in target element
        /// </summary>
        /// <param name="target">Target element</param>
        /// <param name="pos">Postion</param>
        /// <param name="margin">0-1</param>
        /// <returns></returns>
        public static bool IsInArea(this FrameworkElement target, Point pos, Thickness margin)
        {
            if (pos.X < target.ActualWidth * margin.Left || target.ActualHeight * margin.Top < 0 || pos.X > target.ActualWidth * (1 - margin.Right) || pos.Y > target.ActualHeight * (1 - margin.Bottom))
            {
                return false;
            }
            return true;
        }

        public static bool IsInArea(this FrameworkElement target, Point pos)
        {
            return target.IsInArea(pos, new Thickness(0.15, 0.15, 0.15, 0.15));
        }

        #endregion

        #region Date and Time
        public static DateTime LocalTime2GreenwishTime(this DateTime localTime)
        {
            TimeZone localTimeZone = System.TimeZone.CurrentTimeZone;
            TimeSpan timeSpan = localTimeZone.GetUtcOffset(localTime);
            DateTime greenwishTime = localTime - timeSpan;
            return greenwishTime;
        }

        public static string GetMonthStr(this int monthIndex)
        {
            switch (monthIndex)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
                default:
                    return string.Empty;

            }

        }

        public static long TimeToLong(DateTime time)
        {
            return (long)(TimeSpanFromBase(time)).TotalSeconds;
        }

        public static TimeSpan TimeSpanFromBase(DateTime time)
        {
            return (time - new DateTime(1970, 1, 1));
        }

        public static DateTime LongToTime(long val)
        {
            DateTime baseLine = new DateTime(1970, 1, 1);
            var time = baseLine + TimeSpan.FromSeconds(val);
            return time;
        }


        public static long UtcToLocalTime(this long time, int timeZone)
        {
            return time + timeZone * 3600;
        }
        #endregion

        public static T FindParent<T>(this DependencyObject source) where T : DependencyObject
        {
            DependencyObject parent = source;
            Type targetType = typeof(T);
            while (parent != null && parent.GetType() != targetType)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent != null)
            {
                T result = parent as T;
                return result;
            }
            return null;
        }

        public static T FindChild<T>(this DependencyObject parent, string childName)
       where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null)
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }

                    // Need this in case the element we want is nested
                    // in another element of the same type
                    foundChild = FindChild<T>(child, childName);
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// User source update the target
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <typeparam name="P">Source type</typeparam>
        /// <param name="target">Collection needed be update</param>
        /// <param name="source">Source collection</param>
        /// <param name="mergeMethod">Merge method</param>
        public static void MergeTargetCollectionFromSource<T, P>(IList<T> target, IList<P> source, Action<T, P> mergeMethod)
            where T : new()
        {
            while (target.Count > source.Count)
            {
                target.RemoveAt(0);
            }
            P[] sourceArrary = source.ToArray<P>();
            int index = 0;
            for (; index < target.Count; index++)
            {
                T tCurrent = target[index];
                P pCurrent = sourceArrary[index];
                mergeMethod(tCurrent, pCurrent);
            }

            for (; index < sourceArrary.Length; index++)
            {
                T newCurrent = new T();
                P pCurrent = sourceArrary[index];
                mergeMethod(newCurrent, pCurrent);
                target.Add(newCurrent);
            }
        }

        public static void DownloadImage(string url, string account, string password, string logiInfo, Action<Stream> callBack)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential(account, password);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                callBack(responseStream);
            }
            catch (Exception ex)
            {
                callBack(null);
                Log.Instance().Warn(logiInfo, string.Format("[Can't download thumbernail: {0}] [{1} : {2}]", url, ex.GetType().Name, ex.Message));
            }
        }


        public static void DownloadImageSync(string url, string account, string password, string logiInfo, Action<Stream> callBack)
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                DownloadImage(url, account, password, logiInfo, callBack);
            });
        }

        public static bool IsChildOfTarget(this DependencyObject child, FrameworkElement target)
        {
            FrameworkElement parent = child as FrameworkElement;
            if (parent != null)
            {
                while (parent.Parent != null)
                {
                    if (parent.Parent != target)
                    {
                        parent = parent.Parent as FrameworkElement;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region Dialog
        public static bool ShowDialog(this FrameworkElement uiContent, bool isModelDialog = true, bool canCancel = true, Action closeDialogCallback = null)
        {
            return uiContent.ShowDialog(null, isModelDialog, canCancel, closeDialogCallback);
        }

        public static bool ShowDialog(this FrameworkElement uiContent, Point? dialogPos, bool isModelDialog = true, bool canCancel = true, Action closeDialogCallback = null)
        {
            if (maskPanel == null)
            {
                maskPanel = new Canvas();
                maskPanel.SetZIndex(10000);

                maskPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                maskPanel.VerticalAlignment = VerticalAlignment.Stretch;
                Grid.SetColumnSpan(maskPanel, 1000);
                Grid.SetRowSpan(maskPanel, 1000);
                maskPanel.Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));

                showSb = StoryboardFactory.CreateSimpleDoubleAnimationStoryboard(maskPanel, 500, 0.2, 1.0, StoryboardFactory.Opacity);
                closeSb = StoryboardFactory.CreateSimpleDoubleAnimationStoryboard(maskPanel, 500, 1.0, 0.2, StoryboardFactory.Opacity);
                closeSb.Completed += new EventHandler(closeSb_Completed);
                showSb.Completed += new EventHandler(showSb_Completed);
            }
            if (maskPanel.Children.Count == 0)
            {
                if (Application.Current != null)
                {
                    Panel root = Application.Current.MainWindow.Content as Panel;
                    if (root != null)
                    {

                        if (isModelDialog)
                        {

                            maskPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                            maskPanel.VerticalAlignment = VerticalAlignment.Stretch;
                            if (canCancel)
                            {
                                eventHandel = delegate(object sender, TouchEventArgs ex)
                                   {
                                       FrameworkElement source = ex.Source as FrameworkElement;
                                       bool result = source.IsChildOfTarget(uiContent);
                                       if (result == false && uiContent != source)
                                       {
                                           bool closeResult = uiContent.CloseDialog();
                                           if (closeResult && closeDialogCallback != null)
                                           {
                                               closeDialogCallback();
                                           }
                                       }
                                   };

                                maskPanel.TouchDown += eventHandel;
                            }
                        }
                        else
                        {
                            maskPanel.HorizontalAlignment = HorizontalAlignment.Center;
                            maskPanel.VerticalAlignment = VerticalAlignment.Center;
                        }
                        if (dialogPos.HasValue)
                        {
                            Canvas.SetLeft(uiContent, dialogPos.Value.X);
                            Canvas.SetTop(uiContent, dialogPos.Value.Y);
                        }
                        else
                        {
                            if (uiContent.IsLoaded)
                            {
                                Canvas.SetLeft(uiContent, (Application.Current.MainWindow.ActualWidth - uiContent.RenderSize.Width) / 2);
                                Canvas.SetTop(uiContent, (Application.Current.MainWindow.ActualHeight - uiContent.RenderSize.Height) / 2);
                            }
                            else
                            {
                                uiContent.Loaded += (sender, ex) =>
                                {
                                    Canvas.SetLeft(uiContent, (Application.Current.MainWindow.ActualWidth - uiContent.RenderSize.Width) / 2);
                                    Canvas.SetTop(uiContent, (Application.Current.MainWindow.ActualHeight - uiContent.RenderSize.Height) / 2);
                                };
                            }
                        }
                        maskPanel.Children.Add(uiContent);
                        root.Children.Add(maskPanel);
                        maskPanel.IsHitTestVisible = false;
                        showSb.Begin();
                        return true;
                    }
                }
            }
            return false;
        }

        static void showSb_Completed(object sender, EventArgs e)
        {
            maskPanel.IsHitTestVisible = true;
        }

        static void closeSb_Completed(object sender, EventArgs e)
        {
            maskPanel.Children.Clear();
            Panel parent = maskPanel.Parent as Panel;
            if (parent != null)
            {
                parent.Children.Remove(maskPanel);
            }
            if (eventHandel != null)
            {
                maskPanel.RemoveHandler(Canvas.TouchDownEvent, eventHandel);
            }
            maskPanel.IsHitTestVisible = true;
        }

        public static bool CloseDialog(this UIElement content)
        {
            if (maskPanel.Children.Count == 1)
            {
                if (maskPanel.Children[0] == content)
                {
                    maskPanel.IsHitTestVisible = false;
                    closeSb.Begin();
                    return true;
                }
            }
            return false;
        }
        #endregion

        public static void SetImage(this Image ui, string fileAddress, bool isCopy = false)
        {
            try
            {
                if (isCopy)
                {
                    SetImageWithCopy(ui, fileAddress);
                }
                else
                {
                    BitmapImage bt = new BitmapImage(new Uri(fileAddress, UriKind.RelativeOrAbsolute));
                    ui.BeginInit();
                    ui.Source = bt;
                    ui.EndInit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void SetImageWithCopy(this Image ui, string fileAddress)
        {
            try
            {
                byte[] bytes = null;
                using (var fs = File.Open(fileAddress, FileMode.Open))
                {
                    BinaryReader binReader = new BinaryReader(fs);
                    FileInfo fileInfo = new FileInfo(fileAddress);
                    bytes = binReader.ReadBytes((int)fileInfo.Length);
                    BitmapImage source = new BitmapImage();
                    MemoryStream m = new MemoryStream(bytes);
                    source.BeginInit();
                    source.StreamSource = m;
                    source.EndInit();
                    ui.BeginInit();
                    ui.Source = source;
                    ui.EndInit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void SetImage(this Image ui, BitmapSource source)
        {
            try
            {
                ui.BeginInit();
                ui.Source = source;
                ui.EndInit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void Save(this Image ui, string saveAddress)
        {
            using (FileStream stream = new FileStream(saveAddress, FileMode.OpenOrCreate))
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext context = drawingVisual.RenderOpen())
                {
                    VisualBrush brush = new VisualBrush(ui) { Stretch = Stretch.None };
                    context.DrawRectangle(brush, null, new Rect(0, 0, ui.Width, ui.Height));
                    context.Close();
                }

                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)ui.Width, (int)ui.Height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(drawingVisual);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                stream.Close();
            }

        }

        public static void Save(this RenderTargetBitmap bitmap, string saveAddress)
        {
            using (FileStream stream = new FileStream(saveAddress, FileMode.OpenOrCreate))
            {

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                stream.Close();
            }

        }
    }




    public interface IDrag
    {
        bool IsDrag { set; get; }
    }

    internal class DragBoundCheck
    {
        private FrameworkElement element;
        private DragBound bound;
        public DragBoundCheck(FrameworkElement element, DragBound bound)
        {
            this.element = element;
            this.bound = bound;
        }

        public Point GetInBoundPosition(Point newPosition)
        {

            if (newPosition.X > bound.Right - element.ActualWidth)
            {
                newPosition.X = bound.Right - element.ActualWidth;
            }
            if (newPosition.X < bound.Left)
            {
                newPosition.X = bound.Left;
            }
            if (newPosition.Y < bound.Top)
            {
                newPosition.Y = bound.Top;
            }
            if (newPosition.Y > bound.Bottom - element.ActualHeight)
            {
                newPosition.Y = bound.Bottom - element.ActualHeight;
            }
            return newPosition;
        }
    }

    public class DragBound
    {
        public double Top { set; get; }
        public double Bottom
        {
            set;
            get;
        }
        public double Left { set; get; }
        public double Right
        {
            set;
            get;
        }
        public DragBound()
        {
            Top = 0.0;
            Left = 0.0;

            Right = double.MaxValue;
            Bottom = double.MaxValue;
        }
    }


}
