using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interactivity;
using System.Linq;
using System.Windows.Threading;
using System.Diagnostics;

namespace KComponents
{
    public class GestureCheckBehavior : Behavior<FrameworkElement>
    {
        public event EventHandler<GestureArg> OnGestureDetector;


        double currentGestureRadius = 0.0;
        double initGestureRadius = 0.0;
        bool isGestureBehavior = false;
        int gestureCount = 0;
        object scrachObj = new object();

        Dictionary<int, TouchInfo> GestureStatus = new Dictionary<int, TouchInfo>();

        Point gestureInitCenterPoint = new Point();
        TouchInfo lastTouchUpInfo;
        DateTime logTime = DateTime.Now;
        DateTime gestTimeRecord = DateTime.Now;
        GestureResult currentGesture = GestureResult.UnKnown;
        Action<GestureArg, Vector, Vector> gestureDetalBehavior = null;
        Point currentCenterPoint = new Point();
        TouchDevice currentDevice = null;
        GestureSensitive sensitiveFactor = null;
        DispatcherTimer timer;
        public GestureCheckBehavior()
        {

        }

        #region Properties

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            this.timer = new DispatcherTimer();
            this.AssociatedObject.IsManipulationEnabled = true;
            // Insert code that you would want run when the Behavior is attached to an object.
            this.AssociatedObject.PreviewTouchDown += AssociatedObject_PreviewTouchDown;
            this.AssociatedObject.PreviewTouchMove += AssociatedObject_PreviewTouchMove;
            this.AssociatedObject.PreviewTouchUp += AssociatedObject_PreviewTouchUp;

            this.sensitiveFactor = new GestureSensitive(1.0, 1.0);
            this.timer.Tick += new EventHandler(timer_Tick);
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, GestureConsts.Current.PressAndHoldInterval);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            // Insert code that you would want run when the Behavior is removed from an object.
            this.AssociatedObject.PreviewTouchDown -= AssociatedObject_PreviewTouchDown;
            this.AssociatedObject.PreviewTouchMove -= AssociatedObject_PreviewTouchMove;
            this.AssociatedObject.PreviewTouchUp -= AssociatedObject_PreviewTouchUp;

        }

        public void SetCallBack(Action<GestureArg, Vector, Vector> gestureDetalBehavior)
        {
            this.gestureDetalBehavior = gestureDetalBehavior;
        }

        public void SetSensitiveCallBack(GestureSensitive sensitiveFactor)
        {
            this.sensitiveFactor = sensitiveFactor;
        }

        #region Methods
        #region Touch Evnets
        void AssociatedObject_PreviewTouchUp(object sender, TouchEventArgs e)
        {

            if (this.GestureStatus.Count > 0)
            {
                lastTouchUpInfo = this.GestureStatus.Last().Value;
                this.RemoveGestureStatus(e.TouchDevice);
            }
            this.GestureEnd(null);
        }

        void AssociatedObject_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            this.GestureRender(e.TouchDevice);
        }

        void AssociatedObject_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            logTime = DateTime.Now;

            this.GestureStart(e.TouchDevice, e.Timestamp);
        }

        #endregion

        #region Gesture Analysis
        private void GestureStart(TouchDevice device, int timeStamp)
        {

            this.AddGestureStatus(device, timeStamp);

            IEnumerable<Point> points = this.GestureStatus.Select(item => item.Value.Position);

            if (points.Count() > this.gestureCount)
            {
                this.gestureCount = points.Count();
            }
            if (this.gestureCount == 1)
            {
                this.timer.Stop();
                this.timer.Start();
                this.gestTimeRecord = DateTime.Now;
            }
            else
            {
                this.timer.Stop();
            }

            this.gestureInitCenterPoint = this.GetCenterPoint(points);
            this.currentGestureRadius = this.GetRadius(points);
            this.initGestureRadius = this.currentGestureRadius;
            this.currentCenterPoint = gestureInitCenterPoint;
            TraceLog(string.Format("init center is {0}", this.gestureInitCenterPoint));

        }

        private void GestureEnd(TouchDevice device)
        {
            if (this.GestureStatus.Count <= 0 || this.AssociatedObject.TouchesOver.Count() <= 1 || this.isGestureBehavior)
            {
                Console.WriteLine("Tap time is " + (DateTime.Now - this.gestTimeRecord).TotalMilliseconds);
                if (this.isGestureBehavior == false && (DateTime.Now - this.gestTimeRecord).TotalMilliseconds < 350)
                {
                    this.isGestureBehavior = true; ;
                    this.currentGesture = GestureResult.Tap;
                    GestureArg arg = new GestureArg(this.AssociatedObject, this.currentGesture, device);
                    this.FireEvent(arg);

                    this.GestureEnd(device);
                }
                else
                {
                    this.isGestureBehavior = false;
                    this.currentGesture = GestureResult.End;

                    if (this.GestureStatus.Count > 0)
                    {
                        this.GestureStatus.Clear();
                    }
                    this.gestureCount = 0;
                    GestureArg arg = new GestureArg(this.AssociatedObject, this.currentGesture, device);
                    this.FireEvent(arg);
                }

            }
        }

        private void GestureRender(TouchDevice device)
        {
            this.RefreashTouchDeviceStatus(device);

            IEnumerable<Point> points = this.GestureStatus.Select(item => item.Value.Position);
            TraceLog("Check Gesture Count is" + points.Count());
            Point newCenter = this.GetCenterPoint(points);

            double newRadius = this.GetRadius(points);

            double transition = this.GetDis(newCenter, gestureInitCenterPoint);
            double changeRadius = Math.Abs(this.initGestureRadius - newRadius);

            TraceLog(string.Format("newRadius is {0} initGestureRadius is {1}  newCenter is {2} gestureInitCenterPoint is {3}  transition is {4} changeRadius is {5}", newRadius, initGestureRadius, newCenter, gestureInitCenterPoint, transition, changeRadius));


            if (this.isGestureBehavior)
            {
                if (this.currentGesture == GestureResult.Drag || this.currentGesture == GestureResult.Zoom || this.currentGesture == GestureResult.OneFinger)
                {
                    this.TheProcessAfterGestureDetermined(device, newCenter, newRadius);
                }
            }
            else
            {
                if (points.Count() > 1)
                {
                    double time = (DateTime.Now - logTime).TotalMilliseconds;
                    TraceLog("Cost time is " + time);
                    if (time < 50)
                    {
                        return;
                    }
                    bool isInTime = this.IsFingersTouchInTime();
                    if (isInTime)
                    {
                        bool isTwoHandFarZoom = false;

                        double dis = this.GetMaxFingerDistance();
                        isTwoHandFarZoom = dis > GestureConsts.Current.TwoHandsDistance;

                        if (isTwoHandFarZoom || transition < changeRadius * 2)
                        {
                            if ((changeRadius / this.currentGestureRadius) > GestureConsts.Current.MultiFingerZoomThreshold * this.sensitiveFactor.ZoomFactor)
                            {
                                Vector zoomArg = this.GetCumulativeZoomResultParamater(newRadius);

                                if (points.Count() >= 4 && zoomArg.Length < 1.414 && isTwoHandFarZoom == false)
                                {
                                    this.currentGesture = GestureResult.FiveFingersScratch;
                                }
                                else
                                {
                                    this.currentGesture = GestureResult.Zoom;
                                }
                                this.FireZoomOrScrachEvnet(device, zoomArg);
                            }
                        }
                        else
                        {
                            if (transition > GestureConsts.Current.MultiFingerDragThreshold * this.sensitiveFactor.DragFactor)
                            {
                                this.FireDragEvent(device, newCenter);
                            }
                        }
                    }
                }
                else
                {
                    if (gestureCount == 1)
                    {
                        if (transition > GestureConsts.Current.ShakedThreshold)
                        {
                            this.timer.Stop();
                        }
                        if (transition > GestureConsts.Current.MultiFingerDragThreshold)
                        {
                            this.FireDrawingEvent(device, newCenter);

                            TraceLog(string.Format("Gesture cost time is {0}", (DateTime.Now - logTime).TotalMilliseconds));
                        }
                    }
                }

            }
            this.currentGestureRadius = newRadius;
            this.currentCenterPoint = newCenter;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLineIf(true, "<Gesture Special Behavior> timer_Tick");
            if (this.GestureStatus.Count == 1 && this.isGestureBehavior == false)
            {
                this.FireHoldEvent(this.currentDevice, this.currentCenterPoint);
                this.timer.Stop();
            }
        }
        #endregion

        #region Fire Gesture Event
        private void FireZoomOrScrachEvnet(TouchDevice device, Vector zoomArg)
        {
            GestureArg arg = new GestureArg(this.AssociatedObject, this.currentGesture, device);
            arg.Tag = zoomArg;
            arg.Info = this.GestureStatus;
            this.FireEvent(arg);
            this.isGestureBehavior = true;
        }

        private void FireDrawingEvent(TouchDevice device, Point newCenter)
        {
            this.currentGesture = GestureResult.OneFinger;
            GestureArg arg = new GestureArg(this.AssociatedObject, this.currentGesture, device);
            Vector vector = GetTransitionResultParamater(newCenter);
            arg.Tag = vector;
            arg.Info = this.GestureStatus;
            this.FireEvent(arg);
            this.isGestureBehavior = true;
        }

        private void FireDragEvent(TouchDevice device, Point newCenter)
        {
            this.currentGesture = GestureResult.Drag;
            GestureArg arg = new GestureArg(this.AssociatedObject, this.currentGesture, device);
            Vector vector = this.GetCumulativeTransitionResultParamater(newCenter);
            arg.Tag = vector;
            arg.Info = this.GestureStatus;
            this.FireEvent(arg);
            this.isGestureBehavior = true; ;
        }

        private void FireHoldEvent(TouchDevice device, Point newCenter)
        {
            this.currentGesture = GestureResult.Hold;
            GestureArg arg = new GestureArg(this.AssociatedObject, this.currentGesture, device);
            Vector vector = this.GetCumulativeTransitionResultParamater(newCenter);
            arg.Tag = vector;
            arg.Info = this.GestureStatus;
            this.FireEvent(arg);
            this.isGestureBehavior = true; ;
        }
        #endregion

        private void TheProcessAfterGestureDetermined(TouchDevice device, Point newCenter, double newRadius)
        {
            if (this.gestureDetalBehavior != null)
            {
                GestureArg arg = new GestureArg(this.AssociatedObject, this.currentGesture, device);
                arg.Info = this.GestureStatus;
                if (this.currentGesture == GestureResult.Zoom)
                {
                    Vector zoomResult = GetZoomResultParamater(newRadius);
                    Vector zoomCumulativeResult = GetCumulativeZoomResultParamater(newRadius);

                    this.gestureDetalBehavior(arg, zoomCumulativeResult, zoomResult);

                }
                else
                {
                    Vector transition = GetTransitionResultParamater(newCenter);
                    Vector transitionCumulative = this.GetCumulativeTransitionResultParamater(newCenter);
                    this.gestureDetalBehavior(arg, transitionCumulative, transition);
                }
            }
        }

        private void RefreashTouchDeviceStatus(TouchDevice device)
        {
            foreach (var item in this.GestureStatus)
            {
                if (item.Key == device.Id)
                {
                    item.Value.RefreashPostion();
                }
            }
        }

        #region Get Gesture paramaters
        private Vector GetTransitionResultParamater(Point newCenter)
        {
            Vector vector = new Vector();
            vector.X = newCenter.X - this.currentCenterPoint.X;
            vector.Y = newCenter.Y - this.currentCenterPoint.Y;
            return vector;
        }

        private Vector GetCumulativeTransitionResultParamater(Point newCenter)
        {
            Vector vector = new Vector();
            vector.X = newCenter.X - this.gestureInitCenterPoint.X;
            vector.Y = newCenter.Y - this.gestureInitCenterPoint.Y;
            return vector;
        }

        private Vector GetZoomResultParamater(double newRadius)
        {
            Vector zoomResult = new Vector();
            zoomResult.X = newRadius / this.currentGestureRadius;
            zoomResult.Y = newRadius / this.currentGestureRadius;
            return zoomResult;
        }


        private Vector GetCumulativeZoomResultParamater(double newRadius)
        {
            Vector zoomResult = new Vector();
            zoomResult.X = newRadius / this.initGestureRadius;
            zoomResult.Y = newRadius / this.initGestureRadius;
            return zoomResult;
        }
        #endregion

        private Point GetGesureCenter()
        {
            IEnumerable<Point> points = this.GestureStatus.Select(item => item.Value.Position);

            Point center = this.GetCenterPoint(points);

            return center;
        }


        private bool IsInTargetArea(ManipulationDeltaEventArgs e, double radius)
        {
            if (e.Manipulators.Count() > 2)
            {
                double centerX = 0;
                double centerY = 0;
                List<Point> currentPoints = new List<Point>();
                for (int i = 1; i < e.Manipulators.Count(); i++)
                {
                    var manipullator = e.Manipulators.ElementAt(i);
                    TouchDevice device = manipullator as TouchDevice;
                    if (device != null)
                    {
                        TouchPoint point = device.GetTouchPoint(Application.Current.MainWindow);
                        currentPoints.Add(point.Position);
                        centerX += point.Position.X;
                        centerY += point.Position.Y;
                    }
                }
                centerX = centerX / currentPoints.Count;
                centerY = centerY / currentPoints.Count;
                Point center = new Point(centerX, centerY);
                foreach (Point point in currentPoints)
                {
                    double diff = this.GetDis(center, point);
                    if (diff > radius)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsFingersTouchInTime()
        {
            bool isOnTime = true;
            if (this.GestureStatus.Count > 1)
            {
                for (int i = 1; i < this.GestureStatus.Count; i++)
                {
                    int diff = (this.GestureStatus.ElementAt(i).Value.Timestamp - this.GestureStatus.ElementAt(i - 1).Value.Timestamp);
                    TraceLog("Time diff is " + diff);
                    if (diff > GestureConsts.Current.FingersTouchTimeDiff / 10000)
                    {
                        isOnTime = false;
                        break;
                    }
                }
            }
            return isOnTime;
        }

        private void AddGestureStatus(TouchDevice device, int timestamp)
        {
            if (this.GestureStatus.ContainsKey(device.Id) == false)
            {
                this.GestureStatus.Add(device.Id, new TouchInfo(device, Application.Current.MainWindow, timestamp));
                this.currentDevice = device;
            }
        }

        private void RemoveGestureStatus(TouchDevice device)
        {
            if (this.GestureStatus.ContainsKey(device.Id))
            {
                this.GestureStatus.Remove(device.Id);
            }
        }

        private double GetMaxFingerDistance()
        {
            double result = 0;
            if (this.GestureStatus.Count >= 2)
            {
                IEnumerable<Point> fingers = this.GestureStatus.Select(item => item.Value.Position);
                for (int i = 0; i < fingers.Count() - 1; i++)
                {
                    for (int j = i + 1; j < fingers.Count(); j++)
                    {
                        var v = fingers.ElementAt(i) - fingers.ElementAt(j);
                        double distance = Math.Abs(v.Length);
                        if (distance > result)
                        {
                            result = distance;
                        }
                    }
                }
            }
            return result;
        }

        #region Math Method
        private double GetDis(Point one, Point two)
        {
            double result = Math.Sqrt(Math.Pow((two.X - one.X), 2) + Math.Pow((two.Y - one.Y), 2));
            return result;
        }

        private Point GetCenterPoint(IEnumerable<Point> points)
        {
            if (points.Count() > 0)
            {
                if (points.Count() > 1)
                {
                    double maxX = double.MinValue;
                    double maxY = double.MinValue;
                    double minX = double.MaxValue;
                    double minY = double.MaxValue;
                    foreach (var point in points)
                    {
                        if (maxX < point.X)
                        {
                            maxX = point.X;
                        }
                        if (maxY < point.Y)
                        {
                            maxY = point.Y;
                        }
                        if (minX > point.X)
                        {
                            minX = point.X;
                        }
                        if (minY > point.Y)
                        {
                            minY = point.Y;
                        }
                    }

                    Point center = new Point((maxX + minX) / 2, (maxY + minY) / 2);
                    return center;
                }
                else
                {
                    return points.First();
                }
            }
            return new Point();
        }

        private double GetRadius(IEnumerable<Point> points)
        {
            if (points.Count() > 1)
            {
                double maxX = double.MinValue;
                double maxY = double.MinValue;
                double minX = double.MaxValue;
                double minY = double.MaxValue;
                foreach (var point in points)
                {
                    if (maxX < point.X)
                    {
                        maxX = point.X;
                    }
                    if (maxY < point.Y)
                    {
                        maxY = point.Y;
                    }
                    if (minX > point.X)
                    {
                        minX = point.X;
                    }
                    if (minY > point.Y)
                    {
                        minY = point.Y;
                    }
                }

                return MathMethods.GetDistanceBetTowPoints(maxX, maxY, minX, minY) / 2;
            }
            return 0;
        }
        #endregion

        private void FireEvent(GestureArg arg)
        {
            this.AssociatedObject.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (this.OnGestureDetector != null)
                {
                    this.OnGestureDetector(this.AssociatedObject, arg);
                }
            }));
        }

        public class TouchInfo
        {
            private FrameworkElement target;
            public TouchInfo(TouchDevice device, FrameworkElement target, int timeStamp)
            {
                this.Device = device;

                this.Timestamp = timeStamp;

                this.InitPosition = device.GetTouchPoint(Application.Current.MainWindow).Position;

                this.Position = this.InitPosition;

                this.target = target;
            }


            public TouchDevice Device { private set; get; }

            public int ID
            {
                get
                {
                    return this.Device.Id;
                }
            }

            public Point InitPosition { private set; get; }

            public Point Position { private set; get; }

            public int Timestamp { private set; get; }

            public void RefreashPostion()
            {
                this.Position = this.Device.GetTouchPoint(Application.Current.MainWindow).Position;
            }
        }

        void TraceLog(string message)
        {
            Debug.WriteLineIf(DebugParam.IsGestureTraceEnabled, "<Gesture Special Behavior> " + message);
        }

        #endregion
    }
}