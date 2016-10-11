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
    public class GestureDetectorBehavior : Behavior<FrameworkElement>
    {
        public event EventHandler<GestureArg> OnGestureDetector;
        #region Member
        Dictionary<int, int> manipulationStatus = new Dictionary<int, int>();

        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
        DateTime gestureTime = DateTime.Now;
        TouchTapInfo lastTouchUpInfo = null;
        TouchTapInfo preTouchUpInfo = null;
        GestureSensitive sensitiveFactor = null;

        bool isHold = false;
        bool isGestureBehavior = false;
        bool IsPreManipulationEnabled = false;

        object scrachObj = new object();
        int gestureFingerCount = 0;

        GestureResult currentResult = GestureResult.UnKnown;
        Action<GestureArg, ManipulationDelta, ManipulationDelta> gestureDetalBehavior = null;
        Action<GestureArg> gestureStatusBehavior = null;
        //EventHandler<ManipulationStartingEventArgs> startingHandler;
        EventHandler<ManipulationStartedEventArgs> startedHandler;
        EventHandler<ManipulationDeltaEventArgs> deltaHandler;
        EventHandler<ManipulationCompletedEventArgs> completedHandler;
        #endregion

        public GestureDetectorBehavior()
        {
            this.sensitiveFactor = new GestureSensitive(1.0, 1.0);
        }

        #region Properties

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            // Insert code that you would want run when the Behavior is attached to an object.
            this.startedHandler = new EventHandler<ManipulationStartedEventArgs>(this.ManipulationStarted);
            this.deltaHandler = new EventHandler<ManipulationDeltaEventArgs>(this.ManipulationDelta);
            this.completedHandler = new EventHandler<ManipulationCompletedEventArgs>(this.ManipulationCompleted);
            //this.startingHandler = new EventHandler<ManipulationStartingEventArgs>(this.ManipulationStarting);

            this.IsPreManipulationEnabled = this.AssociatedObject.IsManipulationEnabled;
            this.AssociatedObject.IsManipulationEnabled = true;

            //this.AssociatedObject.AddHandler(UIElement.ManipulationStartingEvent, this.startingHandler, true);
            this.AssociatedObject.AddHandler(UIElement.ManipulationStartedEvent, this.startedHandler, true);
            this.AssociatedObject.AddHandler(UIElement.ManipulationDeltaEvent, this.deltaHandler, true);
            this.AssociatedObject.AddHandler(UIElement.ManipulationCompletedEvent, this.completedHandler, true);


            this.AssociatedObject.PreviewTouchDown += AssociatedObject_PreviewTouchDown;
            this.AssociatedObject.PreviewTouchUp += AssociatedObject_PreviewTouchUp;

            timer.Interval = new TimeSpan(0, 0, 0, 0, GestureConsts.Current.PressAndHoldInterval);
            this.timer.Tick += new EventHandler(Tmer_Hold);

        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            // Insert code that you would want run when the Behavior is removed from an object.

            this.AssociatedObject.IsManipulationEnabled = this.IsPreManipulationEnabled;
            //this.AssociatedObject.RemoveHandler(UIElement.ManipulationStartingEvent, this.startingHandler);
            this.AssociatedObject.RemoveHandler(UIElement.ManipulationStartedEvent, this.startedHandler);
            this.AssociatedObject.RemoveHandler(UIElement.ManipulationDeltaEvent, this.deltaHandler);
            this.AssociatedObject.RemoveHandler(UIElement.ManipulationCompletedEvent, this.completedHandler);
            this.AssociatedObject.PreviewTouchDown -= AssociatedObject_PreviewTouchDown;
            this.AssociatedObject.PreviewTouchUp -= AssociatedObject_PreviewTouchUp;
            this.timer.Stop();
        }

        public void SetContinueCallBack(Action<GestureArg, ManipulationDelta, ManipulationDelta> gestureDetalBehavior)
        {
            this.gestureDetalBehavior = gestureDetalBehavior;
        }

        public void SetStatusCallBack(Action<GestureArg> gestureStatusBehavior)
        {
            this.gestureStatusBehavior = gestureStatusBehavior;
        }

        public void SetSensitiveCallBack(GestureSensitive sensitiveFactor)
        {
            this.sensitiveFactor = sensitiveFactor;
        }

        #region Methods
        #region Core
        private void ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            gestureTime = DateTime.Now;
            this.gestureFingerCount = 0;
            this.CheckOnMultiplefingersTouch();
            this.StartCheckHoldEvent(e.Manipulators.Count(), 0, e);
            this.AddManipluations(e);
        }

        private void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.Manipulators != null && e.Manipulators.Count() > 0)
            {
                TouchDevice device = e.Manipulators.First() as TouchDevice;
                if (device == null)
                {
                    return;
                }

                if (this.isGestureBehavior)
                {
                    this.isHold = false;
                    if ((this.currentResult == GestureResult.Drag || this.currentResult == GestureResult.Zoom || this.currentResult == GestureResult.OneFinger) && this.gestureDetalBehavior != null)
                    {
                        GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, device);
                        this.gestureDetalBehavior(arg, e.DeltaManipulation, e.CumulativeManipulation);
                    }
                    return;
                }

                this.CheckOnMultiplefingersTouch();

                this.CheckIsBreakHold(e);

                this.AddManipluations(e);

                //Check other gesturevp
                if (this.CheckIsAreaGesture(e) || this.CheckFiveFingersScrath(e) || this.CheckZoomOrDragGesture(e) || this.CheckIsSimpleZoom(e) || this.CheckIsSimpleDrag(e))
                {
                    return;
                }
            }
        }

        private void ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (isHold)
            {
                timer.Stop();
                this.isHold = false;
            }

            this.manipulationStatus.Clear();
            this.gestureFingerCount = 0;
            if (this.OnGestureDetector != null)
            {
                this.OnGestureDetector(this.AssociatedObject, new GestureArg(this.AssociatedObject, GestureResult.End, null));
            }

            this.isGestureBehavior = false;
        }
        #endregion

        #region Tap
        private void AssociatedObject_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            this.CheckIsDoubleTap(e.TouchDevice);
        }

        private void AssociatedObject_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            this.CheckDoubleTapInfo(e.TouchDevice);
        }

        private void CheckDoubleTapInfo(TouchDevice device)
        {

            if (this.AssociatedObject.TouchesOver.Count() == 1)
            {
                if (this.preTouchUpInfo == null)
                {
                    this.preTouchUpInfo = new TouchTapInfo();
                    this.preTouchUpInfo.Id = device.Id;
                    this.preTouchUpInfo.TouchPoint = device.GetTouchPoint(Application.Current.MainWindow).Position;
                    this.preTouchUpInfo.TouchTime = DateTime.Now;
                }
                else
                {
                    if (lastTouchUpInfo == null)
                    {
                        lastTouchUpInfo = new TouchTapInfo();
                        lastTouchUpInfo.Id = device.Id;
                        lastTouchUpInfo.TouchPoint = device.GetTouchPoint(Application.Current.MainWindow).Position;
                        lastTouchUpInfo.TouchTime = DateTime.Now;
                    }
                }
            }


        }

        private bool CheckIsDoubleTap(TouchDevice device)
        {
            if (this.AssociatedObject.TouchesOver.Count() == 1)
            {
                if (this.preTouchUpInfo != null && this.lastTouchUpInfo == null)
                {
                    bool isVaild = this.preTouchUpInfo.IsVaild(DateTime.Now);
                    if (isVaild == false)
                    {
                        this.preTouchUpInfo = null;
                        return false;
                    }

                }
                else if (this.lastTouchUpInfo != null)
                {
                    bool isVaild = this.lastTouchUpInfo.IsVaild(DateTime.Now);
                    if (isVaild == false)
                    {
                        this.preTouchUpInfo = null;
                        this.lastTouchUpInfo = null;

                        return false;
                    }
                    else
                    {
                        double diffTime = (this.lastTouchUpInfo.TouchTime - this.preTouchUpInfo.TouchTime).TotalMilliseconds;
                        if (diffTime < GestureConsts.Current.VaildDoubleTapTimeDiff)
                        {
                            double diff = this.GetPowDis(this.lastTouchUpInfo.TouchPoint, this.preTouchUpInfo.TouchPoint);
                            if (diff < GestureConsts.Current.DoubleTapDistance)
                            {
                                this.currentResult = GestureResult.DoubleTap;
                                GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, device);
                                List<Point> posInfo = new List<Point>();
                                posInfo.Add(this.preTouchUpInfo.TouchPoint);
                                posInfo.Add(this.lastTouchUpInfo.TouchPoint);
                                arg.Tag = posInfo;
                                if (this.OnGestureDetector != null)
                                {
                                    this.OnGestureDetector(this.AssociatedObject, arg);
                                }
                                this.isGestureBehavior = true;
                                this.preTouchUpInfo = null;
                                this.lastTouchUpInfo = null;
                                return true;
                            }
                        }
                        this.preTouchUpInfo = this.lastTouchUpInfo;
                        this.lastTouchUpInfo = null;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Erase or one finger,or one finger drag
        private bool CheckIsAreaGesture(ManipulationDeltaEventArgs e)
        {
            int fingerCount = this.AssociatedObject.TouchesOver.Count();
            if (fingerCount > this.gestureFingerCount)
            {
                this.gestureFingerCount = fingerCount;
            }

            TouchDevice device = (TouchDevice)e.Manipulators.First();
            TouchPoint point = device.GetTouchPoint(Application.Current.MainWindow);
            double mendedHeight = GestureDetector.GetMendedHeight(point.Size.Width, point.Size.Height);
            if (GestureDetector.IsErase(point.Size.Width, mendedHeight))
            {
                if (this.OnGestureDetector != null)
                {
                    this.currentResult = GestureResult.Brush;
                    GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, device);
                    arg.Tag = new Rect(point.Position.X, point.Position.Y, point.Size.Width, mendedHeight);
                    this.OnGestureDetector(this.AssociatedObject, arg);
                }
                this.isGestureBehavior = true;
                return true;
            }
            else
            {
                if (this.gestureFingerCount == 1)
                {
                    double checkWidht = point.Size.Width;
                    double checkHeight = mendedHeight;
                    double checkRadio = checkHeight / checkWidht;
                    bool isZoom = this.IsZoom(e.CumulativeManipulation.Scale);
                    if (
                        (checkRadio > GestureConsts.Current.FingerHeightWidhtRatio || checkWidht < (1 / GestureConsts.Current.FingerHeightWidhtRatio))
                        && (Math.Min(checkWidht, checkHeight) > GestureConsts.Current.OneFingerSize)
                        )
                    {
                        if (this.gestureStatusBehavior != null)
                        {
                            this.gestureStatusBehavior(new GestureArg(this.AssociatedObject, GestureResult.MultipleFingers, device));
                        }
                        if (e.CumulativeManipulation.Translation.Length > GestureConsts.Current.MultiFingerDragThreshold && isZoom == false)
                        {
                            if (this.OnGestureDetector != null)
                            {
                                this.currentResult = GestureResult.Drag;
                                GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, device);
                                arg.Tag = e.CumulativeManipulation.Translation;
                                this.OnGestureDetector(this.AssociatedObject, arg);
                            }
                            this.isGestureBehavior = true;
                            return true;
                        }
                    }

                    else if (e.CumulativeManipulation.Translation.Length > GestureConsts.Current.MultiFingerDragThreshold && isZoom == false)
                    {

                        if (this.OnGestureDetector != null)
                        {
                            this.currentResult = GestureResult.OneFinger;
                            this.OnGestureDetector(this.AssociatedObject, new GestureArg(this.AssociatedObject, this.currentResult, device));
                        }
                        this.isGestureBehavior = true;
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Check CheckFiveFingersScrath
        private bool CheckFiveFingersScrath(ManipulationDeltaEventArgs e)
        {
            if (IsScrachTrend(e))
            {
                Size area = this.FingersTouchArea(e);
                double ratio = area.Height / area.Width;
                TraceLog("Five Ratio is " + ratio);
                if (ratio < 2 && ratio > 0.5)
                {
                    bool isOnTime = IsFingersTouchInTime();
                    double maxFingerDistance = this.GetMaxFingerDistance(e);
                    bool isInArea = maxFingerDistance < GestureConsts.Current.FiveFingerAreaDistance;
                    if (this.OnGestureDetector != null && isOnTime && isInArea)
                    {
                        this.currentResult = GestureResult.FiveFingersScratch;
                        this.OnGestureDetector(this.AssociatedObject, new GestureArg(this.AssociatedObject, this.currentResult, (TouchDevice)e.Manipulators.First()));
                        this.isGestureBehavior = true;
                        return true;
                    }
                }

            }

            return false;
        }

        private bool IsFingersTouchInTime()
        {
            bool isOnTime = true;
            if (this.manipulationStatus.Count > 1)
            {
                for (int i = 1; i < this.manipulationStatus.Count; i++)
                {

                    double diff = (this.manipulationStatus.ElementAt(i).Value - this.manipulationStatus.ElementAt(i - 1).Value);
                    TraceLog("Time diff is " + diff);
                    if (diff > GestureConsts.Current.FingersTouchTimeDiff * 2)
                    {
                        isOnTime = false;
                        break;
                    }
                }
            }
            return isOnTime;
        }

        private bool IsScrachTrend(ManipulationDeltaEventArgs e)
        {
            bool isCountOk = (this.gestureFingerCount >= 4);
            bool isScaleOk = ((e.CumulativeManipulation.Scale.X < (1 - GestureConsts.Current.FiveFingerZoomChange)) && (e.CumulativeManipulation.Scale.Y < (1 - GestureConsts.Current.FiveFingerZoomChange)));
            bool isExpendsiton = ((e.CumulativeManipulation.Expansion.X < 0) && (e.CumulativeManipulation.Expansion.Y < 0));


            bool isTranslationOK = (Math.Abs(e.CumulativeManipulation.Translation.X) < GestureConsts.Current.FiveFingersTouchDisLimit && Math.Abs(e.CumulativeManipulation.Translation.Y) < GestureConsts.Current.FiveFingersTouchDisLimit);
            return isCountOk && isScaleOk && isExpendsiton && isTranslationOK;
        }

        private Size FingersTouchArea(ManipulationDeltaEventArgs e)
        {
            double left = double.MaxValue;
            double top = double.MaxValue;
            double right = double.MinValue;
            double bottom = double.MinValue;
            foreach (var manipullator in e.Manipulators)
            {
                TouchDevice device = manipullator as TouchDevice;
                TouchPoint point = device.GetTouchPoint(Application.Current.MainWindow);
                Point pos = point.Position;
                if (pos.X < left)
                {
                    left = pos.X;
                }
                if (pos.X > right)
                {
                    right = pos.X;
                }
                if (pos.Y < top)
                {
                    top = pos.Y;
                }
                if (pos.Y > bottom)
                {
                    bottom = pos.Y;
                }

            }
            return new Size(right - left, bottom - top);
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
                    double diff = this.GetPowDis(center, point);
                    if (diff > radius)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion

        #region Drag or Zoom
        private bool CheckZoomOrDragGesture(ManipulationDeltaEventArgs e)
        {
            double costTime = (DateTime.Now - this.gestureTime).TotalMilliseconds;
            TraceLog("Cost time:" + costTime);
            if (costTime > 60)
            {
                GestureResult result = this.GetDragOrZoomGestureResult(e);
                if (result == GestureResult.Drag)
                {
                    if (this.OnGestureDetector != null)
                    {
                        this.currentResult = GestureResult.Drag;
                        GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, (TouchDevice)e.Manipulators.First());
                        arg.Tag = e.CumulativeManipulation.Translation;
                        this.OnGestureDetector(this.AssociatedObject, arg);
                    }
                    this.isGestureBehavior = true;
                    return true;
                }
                else if (result == GestureResult.Zoom)
                {
                    if (this.OnGestureDetector != null)
                    {
                        this.currentResult = GestureResult.Zoom;
                        GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, (TouchDevice)e.Manipulators.First());
                        arg.Tag = e.DeltaManipulation.Scale;
                        this.OnGestureDetector(this.AssociatedObject, arg);
                    }
                    this.isGestureBehavior = true;
                    return true;
                }
            }

            return false;
        }

        private bool CheckIsSimpleZoom(ManipulationDeltaEventArgs e)
        {
            if ((DateTime.Now - this.gestureTime).TotalMilliseconds > 150)
            {
                bool isInTime = this.IsFingersTouchInTime();
                if (this.IsZoom(e.CumulativeManipulation.Scale) && isInTime)
                {
                    double distance = this.GetMinFingerDistance(e);
                    if (distance > GestureConsts.Current.OneFingerSize && e.CumulativeManipulation.Translation.Length < GestureConsts.Current.MultiFingerZoomLimitDrag * 2)
                    {
                        if (this.OnGestureDetector != null)
                        {
                            this.currentResult = GestureResult.Zoom;
                            GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, (TouchDevice)e.Manipulators.First());
                            arg.Tag = e.DeltaManipulation.Scale;
                            this.OnGestureDetector(this.AssociatedObject, arg);
                        }
                        this.isGestureBehavior = true;
                    }
                    return true;
                }
            }

            return false;
        }

        private bool CheckIsSimpleDrag(ManipulationDeltaEventArgs e)
        {
            if ((DateTime.Now - this.gestureTime).TotalMilliseconds > 150)
            {
                bool isSimpleDrag = this.IsSimpleDrag(e, this.sensitiveFactor.DragFactor);
                bool isInTime = this.IsFingersTouchInTime();
                if (e.Manipulators.Count() == 2 && isSimpleDrag && isInTime)
                {
                    if (this.OnGestureDetector != null)
                    {
                        this.currentResult = GestureResult.Drag;
                        GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, (TouchDevice)e.Manipulators.First());
                        arg.Tag = e.DeltaManipulation.Translation;
                        this.OnGestureDetector(this.AssociatedObject, arg);
                    }
                    this.isGestureBehavior = true;
                    return true;
                }
            }

            return false;
        }

        private GestureResult GetDragOrZoomGestureResult(ManipulationDeltaEventArgs e)
        {
            bool isInTime = this.IsFingersTouchInTime();
            if (isInTime)
            {
                if (e.Manipulators.Count() > 1)
                {
                    bool isZoom = this.IsZoom(e.CumulativeManipulation.Scale);
                    double maxDistance = this.GetMaxFingerDistance(e);
                    if (maxDistance > GestureConsts.Current.TwoHandsDistance)
                    {
                        if (e.CumulativeManipulation.Translation.Length < GestureConsts.Current.MultiFingerZoomLimitDrag)
                        {
                            if (isZoom)
                            {
                                return GestureResult.Zoom;
                            }
                        }
                    }
                    else
                    {
                        double scaleOffset = e.DeltaManipulation.Scale.X / e.CumulativeManipulation.Scale.X;
                        double translationOffset = e.DeltaManipulation.Translation.Length / e.CumulativeManipulation.Translation.Length;

                        double justValue = (e.Manipulators.Count() - 1) * GestureConsts.Current.OneFingerSize;

                        if (maxDistance >= justValue
                            && maxDistance < this.gestureFingerCount * GestureConsts.Current.OneFingerSize * 2
                            )
                        {
                            if (IsDrag(isZoom, e))
                            {
                                return GestureResult.Drag;
                            }
                        }
                        else
                        {
                            if (isZoom
                                && scaleOffset >= translationOffset
                                && e.DeltaManipulation.Translation.Length < GestureConsts.Current.MultiFingerDragThreshold
                                && e.CumulativeManipulation.Translation.Length < GestureConsts.Current.MultiFingerZoomLimitDrag)
                            {
                                return GestureResult.Zoom;
                            }
                            else
                            {

                                if (IsDrag(isZoom, e))
                                {
                                    return GestureResult.Drag;
                                }
                            }
                        }
                    }
                }
            }
            return GestureResult.UnKnown;
        }

        private bool IsDrag(bool isZoom, ManipulationDeltaEventArgs e)
        {
            bool isSimpleDrag = this.IsSimpleDrag(e, this.sensitiveFactor.DragFactor);
            return (e.Manipulators.Count() != 5)
                && isSimpleDrag
                && (isZoom == false);
        }

        private bool IsSimpleDrag(ManipulationDeltaEventArgs e, double ratio)
        {
            double maxDistance = this.GetMaxFingerDistance(e);
            return (e.CumulativeManipulation.Translation.Length > GestureConsts.Current.MultiFingerDragThreshold * ratio)
                && maxDistance < GestureConsts.Current.TwoFingersDistance * e.Manipulators.Count();
        }

        private bool IsZoom(Vector v)
        {
            if (Math.Abs(v.X - 1) > GestureConsts.Current.MultiFingerZoomThreshold * this.sensitiveFactor.ZoomFactor
                || Math.Abs(v.Y - 1) > GestureConsts.Current.MultiFingerZoomThreshold * this.sensitiveFactor.ZoomFactor)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Check Hold
        private void StartCheckHoldEvent(int touchCount, double offset, ManipulationStartedEventArgs e)
        {
            if (e.Manipulators != null && e.Manipulators.Count() > 0)
            {
                TouchDevice device = e.Manipulators.First() as TouchDevice;
                if (device != null)
                {
                    TouchPoint point = device.GetTouchPoint(Application.Current.MainWindow);
                    if (touchCount == 1 && offset < GestureConsts.Current.ShakedThreshold)
                    {
                        if (this.isHold == false)
                        {
                            this.timer.Start();
                            this.timer.Tag = point.Position;
                            this.isHold = true;
                        }
                    }
                    else
                    {
                        if (this.isHold)
                        {
                            isHold = false;
                            timer.Stop();
                        }
                    }
                }
            }
        }

        private void CheckIsBreakHold(ManipulationDeltaEventArgs e)
        {
            if (e.Manipulators.Count() == 1)
            {
                if (e.CumulativeManipulation.Translation.Length > GestureConsts.Current.MultiFingerDragThreshold)
                {
                    this.isHold = false;
                }
            }
            else
            {
                this.isHold = false;
            }
            if (this.isHold == false)
            {
                this.timer.Stop();
            }

        }


        /// <summary>
        /// Fire the Hold event 
        /// </summary>
        void Tmer_Hold(object sender, EventArgs e)
        {
            timer.Stop();
            if (this.AssociatedObject.TouchesOver.Count() > 0)
            {
                TouchDevice device = this.AssociatedObject.TouchesOver.First();
                TouchPoint point = device.GetTouchPoint(Application.Current.MainWindow);
                Point startPos = (Point)timer.Tag;
                if (point.Position.Sub(startPos).Length() < GestureConsts.Current.ShakedThreshold)
                {
                    if (this.isHold && this.OnGestureDetector != null)
                    {

                        this.currentResult = GestureResult.Hold;
                        GestureArg arg = new GestureArg(this.AssociatedObject, this.currentResult, null);
                        arg.Tag = this.timer.Tag;
                        this.OnGestureDetector(this.AssociatedObject, arg);
                        this.isGestureBehavior = true;
                        this.isHold = false;
                    }
                }
            }
        }
        #endregion

        #region Check if multiple finger start touch  status
        private void CheckOnMultiplefingersTouch()
        {
            IEnumerable<TouchDevice> devices = this.AssociatedObject.TouchesOver;
            if (this.gestureFingerCount >= 2 || devices.Count() >= 2)
            {
                if (this.gestureStatusBehavior != null)
                {
                    if (devices.Count() > 0)
                    {
                        GestureArg arg = new GestureArg(this.AssociatedObject, GestureResult.MultipleFingers, devices.First());
                        arg.Info = devices;
                        this.gestureStatusBehavior(arg);
                    }
                }
            }
        }
        #endregion

        #region Helper
        private bool FingersAreFar(ManipulationDeltaEventArgs e)
        {
            if (e.Manipulators.Count() >= 2)
            {
                foreach (var finger in e.Manipulators)
                {
                    var v = finger.GetPosition(e.ManipulationContainer) - e.ManipulationOrigin;
                    if (Math.Abs(v.Length) < GestureConsts.Current.TwoHandsDistance)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private double GetMaxFingerDistance(ManipulationDeltaEventArgs e)
        {
            double result = 0;
            if (e.Manipulators.Count() >= 2)
            {
                List<IManipulator> fingers = e.Manipulators.ToList();
                for (int i = 0; i < fingers.Count - 1; i++)
                {
                    for (int j = i + 1; j < fingers.Count; j++)
                    {
                        var v = fingers[i].GetPosition(Application.Current.MainWindow) - fingers[j].GetPosition(Application.Current.MainWindow);
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

        private double GetMinFingerDistance(ManipulationDeltaEventArgs e)
        {
            double result = 2000;
            if (e.Manipulators.Count() >= 2)
            {
                List<IManipulator> fingers = e.Manipulators.ToList();
                for (int i = 0; i < fingers.Count - 1; i++)
                {
                    for (int j = i + 1; j < fingers.Count; j++)
                    {
                        var v = fingers[i].GetPosition(Application.Current.MainWindow) - fingers[j].GetPosition(Application.Current.MainWindow);
                        double distance = Math.Abs(v.Length);
                        if (distance < result)
                        {
                            result = distance;
                        }
                    }
                }
            }
            return result;
        }
        private void AddManipluations(ManipulationStartedEventArgs e)
        {
            foreach (var manipulator in e.Manipulators)
            {
                TouchDevice device = manipulator as TouchDevice;
                if (device != null && this.manipulationStatus.ContainsKey(device.Id) == false)
                {
                    this.manipulationStatus.Add(device.Id, e.Timestamp);
                    TraceLog("Add new touch at " + e.Timestamp.ToString());
                }
            }
        }
        private void AddManipluations(ManipulationDeltaEventArgs e)
        {
            foreach (var manipulator in e.Manipulators)
            {
                TouchDevice device = manipulator as TouchDevice;
                if (this.manipulationStatus.ContainsKey(device.Id) == false)
                {
                    this.manipulationStatus.Add(device.Id, e.Timestamp);
                }
            }
        }

        private int GetMendedFingerCount(IEnumerable<IManipulator> manipulators)
        {
            int sum = 0;
            foreach (var mani in manipulators)
            {
                TouchDevice device = (TouchDevice)mani;
                TouchPoint point = device.GetTouchPoint(Application.Current.MainWindow);
                double width = point.Size.Width;
                double height = GestureDetector.GetMendedHeight(point.Size.Width, point.Size.Height);
                double ratio = height / width;
                if (ratio > GestureConsts.Current.FingerHeightWidhtRatio || ratio < (1 / GestureConsts.Current.FingerHeightWidhtRatio))
                {
                    sum += 2;
                }
                else
                {
                    sum += 1;
                }
            }
            return sum;
        }

        private double GetPowDis(Point one, Point two)
        {
            double result = Math.Sqrt(Math.Pow((two.X - one.X), 2) + Math.Pow((two.Y - one.Y), 2));
            return result;
        }

        private bool IsInBoudOnTouchWall(Point pos)
        {
            if (pos.X < 300 || pos.X > System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - 300)
            {
                return true;
            }
            return false;
        }


        private void TraceLog(string message)
        {
            Debug.WriteLineIf(DebugParam.IsGestureTraceEnabled, "<Gesture Common Behavior> " + message);
        }
        #endregion
        #endregion
    }

    public class TouchTapInfo
    {

        public int Id { set; get; }
        public Point TouchPoint { set; get; }
        public DateTime TouchTime { set; get; }
        public bool IsVaild(DateTime leaveTime)
        {
            double diff = (leaveTime - TouchTime).TotalMilliseconds;
            if (diff < GestureConsts.Current.VaildSingleTapTimeDiff)
            {
                return true;
            }
            return false;
        }

    }
}