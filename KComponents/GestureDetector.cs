using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml.Linq;

namespace KComponents
{

    public static class GestureDetector
    {
        public struct TapInfo
        {
            internal int Key;
            public Point Position;
            public int Timestamp;
        }

        static bool usingMended = false;
        static Dictionary<int, TapInfo> tapTrace = new Dictionary<int, TapInfo>();
        static GestureDetector()
        {
            usingMended = true;
        }


        public static void AttachGestureDetector(this FrameworkElement element, Action<GestureArg> callback, Action<GestureArg, ManipulationDelta, ManipulationDelta> gestureDetalBehavior = null, Action<GestureArg> gestureStatusBehavior = null)
        {
            GestureDetector.AttachGestureDetector(element, callback, null, gestureDetalBehavior, gestureStatusBehavior);
        }

        public static void AttachGestureDetector(this FrameworkElement element, Action<GestureArg> callback, GestureSensitive sensiveFactor, Action<GestureArg, ManipulationDelta, ManipulationDelta> gestureDetalBehavior = null, Action<GestureArg> gestureStatusBehavior = null)
        {
            GestureDetectorBehavior behavior = new GestureDetectorBehavior();
            if (gestureDetalBehavior != null)
            {
                behavior.SetContinueCallBack(gestureDetalBehavior);
            }

            if (gestureStatusBehavior != null)
            {
                behavior.SetStatusCallBack(gestureStatusBehavior);
            }

            if (sensiveFactor != null)
            {
                behavior.SetSensitiveCallBack(sensiveFactor);
            }

            behavior.OnGestureDetector += (ele, arg) =>
            {
                callback((GestureArg)arg);
            };
            Interaction.GetBehaviors(element).Add(behavior);
        }

        public static void DetachGestureDetector(this FrameworkElement element)
        {
            BehaviorCollection collection = Interaction.GetBehaviors(element);
            int index = -1;
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].GetType() == typeof(GestureDetectorBehavior))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                collection.RemoveAt(index);
            }
        }

        public static void AttachTouchOperation(this FrameworkElement element, Action<GestureArg> callback, Action<GestureArg, Vector, Vector> gestureDetalBehavior = null)
        {
            AttachTouchOperation(element, callback, null, gestureDetalBehavior);
        }

        public static void AttachTouchOperation(this FrameworkElement element, Action<GestureArg> callback, GestureSensitive sensiveFactor, Action<GestureArg, Vector, Vector> gestureDetalBehavior = null)
        {
            GestureCheckBehavior behavior = new GestureCheckBehavior();
            behavior.OnGestureDetector += (ele, arg) =>
            {
                callback((GestureArg)arg);
            };
            behavior.SetCallBack(gestureDetalBehavior);
            Interaction.GetBehaviors(element).Add(behavior);
        }

        public static void AttachTouchTapEvent(this FrameworkElement element, Action<Point> tapEvent)
        {
            Stylus.SetIsPressAndHoldEnabled(element, false);
            Stylus.SetIsTapFeedbackEnabled(element, false);
            element.PreviewTouchDown += (sender, ex) =>
            {
                if (tapTrace.ContainsKey(ex.TouchDevice.Id) == false)
                {
                    TapInfo info = new TapInfo();
                    info.Key = ex.TouchDevice.Id;
                    info.Position = ex.GetTouchPoint(Application.Current.MainWindow).Position;
                    info.Timestamp = ex.Timestamp;
                    tapTrace.Add(ex.TouchDevice.Id, info);
                }
            };

            element.PreviewTouchUp += (sender, ex) =>
            {
                if (tapTrace.ContainsKey(ex.TouchDevice.Id))
                {
                    TapInfo info = tapTrace[ex.TouchDevice.Id];
                    tapTrace.Remove(ex.TouchDevice.Id);
                    int timeStampDiff = (ex.Timestamp - info.Timestamp);
                    if (timeStampDiff < 400)//0.4 seconds
                    {
                        Point newPos = ex.GetTouchPoint(Application.Current.MainWindow).Position;
                        Point result = newPos.Sub(info.Position);


                        if (result.Length() < GestureConsts.Current.DoubleTapDistance)
                        {
                            tapEvent(info.Position);
                        }
                    }

                }
            };

        }

        public static void DetachTouchOperation(this FrameworkElement element)
        {
            BehaviorCollection collection = Interaction.GetBehaviors(element);
            int index = -1;
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].GetType() == typeof(GestureCheckBehavior))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                collection.RemoveAt(index);
            }
        }

        public static double GetMendedHeight(double width, double height)
        {
            if (usingMended)
            {
                double reduceHeight = width * Math.Pow(3, 0.5);
                double mendedHeight = height - reduceHeight;
                if (mendedHeight <= 0)
                {
                    return height;
                }
                else
                {
                    return mendedHeight;
                }
            }
            return height;
        }

        public static bool IsErase(double width, double height)
        {
            double radio = width / height;
            double size = width * height * SystemHelper.DpiScaleRate * SystemHelper.DpiScaleRate;
            Trace.WriteLineIf(DebugParam.IsGestureTraceEnabled, string.Format("Size is {0},Width={1},Height={2}", size, width, height));
            if (size > GestureConsts.Current.EraseArea || height > GestureConsts.Current.EraseHeight)
            {
                return true;
            }
            return false;
        }
    }

    public class GestureConsts
    {
        static GestureConsts instance = null;
        private GestureConsts()
        {
        }

        public static GestureConsts Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new GestureConsts();
                }
                return instance;
            }
        }

        public void Regist()
        {
            string filePath = "Assets/TouchWallGestureConfig.xml";
            if (SystemHelper.IsTouchWall == false)
            {
                filePath = "Assets/TeamVersionGestureConfig.xml";
            }

            XElement root = XElement.Load(filePath);


            this.VaildSingleTapTimeDiff = double.Parse(root.Element("VaildSingleTapTimeDiff").Attribute("value").Value);
            this.VaildDoubleTapTimeDiff = int.Parse(root.Element("VaildDoubleTapTimeDiff").Attribute("value").Value);
            this.FingersTouchTimeDiff = int.Parse(root.Element("FingersTouchTimeDiff").Attribute("value").Value);
            this.FiveFingersTouchDisLimit = double.Parse(root.Element("FiveFingersTouchDisLimit").Attribute("value").Value);

            this.OneFingerSize = double.Parse(root.Element("OneFingerSize").Attribute("value").Value);
            this.DoubleTapDistance = double.Parse(root.Element("DoubleTapDistance").Attribute("value").Value);
            this.DoubleJustTapDistance = double.Parse(root.Element("DoubleJustTapDistance").Attribute("value").Value);
            this.EraseArea = double.Parse(root.Element("EraseArea").Attribute("value").Value);
            this.EraseHeight = double.Parse(root.Element("EraseHeight").Attribute("value").Value);
            this.FingerHeightWidhtRatio = double.Parse(root.Element("FingerHeightWidhtRatio").Attribute("value").Value);

            this.PressAndHoldInterval = int.Parse(root.Element("PressAndHoldInterval").Attribute("value").Value);
            this.ShakedThreshold = double.Parse(root.Element("ShakedThreshold").Attribute("value").Value);
            this.FiveFingerZoomChange = double.Parse(root.Element("FiveFingerZoomChange").Attribute("value").Value);
            this.FiveFingerAreaDistance = double.Parse(root.Element("FiveFingerAreaDistance").Attribute("value").Value);

            this.TwoHandsDistance = double.Parse(root.Element("TwoHandsDistance").Attribute("value").Value);
            this.TwoFingersDistance = double.Parse(root.Element("TwoFingersDistance").Attribute("value").Value);

            this.MultiFingerZoomThreshold = double.Parse(root.Element("MultiFingerZoomThreshold").Attribute("value").Value);
            this.MultiFingerZoomLimitDrag = double.Parse(root.Element("MultiFingerZoomLimitDrag").Attribute("value").Value);
            this.MultiFingerDragThreshold = double.Parse(root.Element("MultiFingerDragThreshold").Attribute("value").Value);

        }

        public double VaildSingleTapTimeDiff = 200;

        public int VaildDoubleTapTimeDiff = 500;

        public int FingersTouchTimeDiff = 2000000;//unit is 0.000001 seconds
        public double FiveFingersTouchDisLimit = 25;
        public double OneFingerSize = 25;

        public double DoubleTapDistance = 20;

        public double DoubleJustTapDistance = 10000;

        public bool IsInTouchWall = true;

        public double EraseArea = 4500;

        public double EraseHeight = 100;

        /// <summary>
        /// unit: millseconds
        /// </summary>
        public int PressAndHoldInterval = 800;

        public double FingerHeightWidhtRatio = 1.8;

        /// <summary>
        /// finger shake threshold
        /// </summary>
        public double ShakedThreshold = 18.0;

        public double FiveFingerZoomChange = 0.07;

        public double FiveFingerAreaDistance = 300;

        /// <summary>
        /// fingers blongs to two hands
        /// </summary>
        public double TwoHandsDistance = 400;

        /// <summary>
        /// fingers blongs to two hands
        /// </summary>
        public double TwoFingersDistance = 100;

        /// <summary>
        /// threshold of zoom gesture
        /// </summary>
        public double MultiFingerZoomThreshold = 0.08;


        /// <summary>
        /// threshold of drag gesture
        /// </summary>
        public double MultiFingerZoomLimitDrag = 150;
        /// <summary>
        /// threshold of drag gesture
        /// </summary>
        public double MultiFingerDragThreshold = 20;
    }

    public class GestureArg : EventArgs
    {
        public GestureResult Result { private set; get; }
        public object Tag { set; get; }
        public object Info { set; get; }
        public TouchDevice Device { private set; get; }
        public UIElement Sender { get; private set; }
        public GestureArg(UIElement sender, GestureResult result, TouchDevice device)
            : base()
        {
            this.Result = result;
            this.Device = device;
            this.Sender = sender;
        }
    }

    public enum GestureResult
    {
        UnKnown = 0,
        FiveFingersScratch = 1,
        Hold = 2,
        Drag = 3,
        Zoom = 4,
        Brush = 5,
        OneFinger = 6,
        DoubleTap = 7,
        MultipleFingers = 8,
        Tap = 9,
        End,
    }

    public class GestureSensitive
    {
        public GestureSensitive(double zoom, double drag)
        {
            this.ZoomFactor = zoom;
            this.DragFactor = drag;
            this.IsOnlyZoomWithTwoHands = false;
        }

        public GestureSensitive(double zoom, double drag, bool isOnlyZoomWithTwoHands)
        {
            this.ZoomFactor = zoom;
            this.DragFactor = drag;
            this.IsOnlyZoomWithTwoHands = isOnlyZoomWithTwoHands;
        }

        public double ZoomFactor { private set; get; }
        public double DragFactor { private set; get; }

        public bool IsOnlyZoomWithTwoHands { private set; get; }


    }


}
