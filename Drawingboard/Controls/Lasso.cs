using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using KComponents;
using Microsoft.Surface.Presentation.Controls;

namespace Drawingboard.controls
{
    [TemplatePart(Name = "gdRoot", Type = typeof(Grid))]
    [TemplatePart(Name = "inkOperation", Type = typeof(InkCanvas))]
    [TemplatePart(Name = "cvsFeedback", Type = typeof(Canvas))]
    public class Lasso : Control
    {
        #region public events
        public event EventHandler<PositionEventArgs> DoubleTap;
        bool isAnimation = false;
        public bool IsAnimation
        {
            set
            {
                this.isAnimation = value;
                if (this.isAnimation)
                {
                    lassoColor = Color.FromArgb(255, 70, 192, 249);

                }
                else
                {
                    lassoColor = Color.FromArgb(255, 173, 255, 87);
                }
                if (this.mainSelectionBox != null)
                {
                    this.mainSelectionBox.BorderColor = this.lassoColor;
                    this.mainSelectionBox.IndicatorVisiable = !this.isAnimation;
                }
            }
            get
            {
                return this.isAnimation;
            }
        }
        #endregion

        #region Private

        #region Parts
        private InkCanvas inkOperation;
        private Canvas cvsFeedback;
        private Grid gdRoot;
        #endregion

        #region Sync Action
        Action<StrokesSelectedChangedArgs> selectedStrokesChangedAction;

        /// <summary>
        /// Call backs
        /// </summary>
        Action<SelectionChangeArgs> selectedMovingAction;
        Action<SelectionChangedArgs> selectedMovedAction;

        Action<SelectionChangeArgs> selectedScalingAction;
        Action<SelectionChangedArgs> selectedScaledAction;

        Action<StartDrawingLassoArg> startDrawingLassoFeedback;
        Action<DrawingLassoArg> drawingLassoFeedback;
        Action<DrawingLassoArg> endDrawingLassoFeedback;

        Action<ManipulationDeltaEventArgs> gestureDeltaAction;
        Action<ManipulationStartingEventArgs> gestureBeginAction;
        Action<ManipulationCompletedEventArgs> gestureDoneAction;

        Action<Point> scratchGesture;
        Action<Point> openMenuGesture;
        Action closeMenuGesture;
        #endregion

        Vector moveChangedOffset;
        Vector scaleChangedOffset;

        Point lastTouchPosition;

        Brush black = new SolidColorBrush(Colors.Black);
        Brush recFill = new SolidColorBrush(Color.FromArgb(20, 74, 193, 243));
        Color lassoColor = Color.FromArgb(255, 173, 255, 87);

        Guid selectionBehaviorId;
        Guid drawingLassoId;

        List<Vector> movingLassoData = new List<Vector>();
        Dictionary<Guid, TouchBehaviorStatus> touchBehaviorStatus = new Dictionary<Guid, TouchBehaviorStatus>();
        SelectionBox mainSelectionBox;

        StrokeCollection currentOperationStrokes = new StrokeCollection();
        int drawingLassoTouchId;

        int scaleSelectionCount = 0;
        int moveSelectionCount = 0;

        double minScale = 0.5;
        double maxScale = 4;

        bool isMovingSelection = false;
        bool isDrawingLasso = false;
        bool isScalingSelection = false;
        bool isGestureBehavior = false;

        object synActionObj = new object();

        const int SYNCDATACOUNT = 6;
        const int RECOFFSET = 14;
        #endregion

        static Lasso()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Lasso), new FrameworkPropertyMetadata(typeof(Lasso)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.inkOperation = this.GetTemplateChild("inkOperation") as InkCanvas;
            this.cvsFeedback = this.GetTemplateChild("cvsFeedback") as Canvas;

            this.gdRoot = this.GetTemplateChild("gdRoot") as Grid;

            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                this.IsHitTestVisible = false;

                this.IsManipulationEnabled = true;
                this.inkOperation.IsManipulationEnabled = false;

                this.HandelGobalGesture();
            }
        }

        #region Public Properties
        public bool IsStart { private set; get; }
        public Rect Bound { set; get; }
        public StrokeCollection SelectionStrokes { private set; get; }

        public Panel OperatePanel { set; get; }

        public Color LassoColor
        {
            get
            {
                return this.lassoColor;
            }
            set
            {
                this.lassoColor = value;
            }
        }

        #region SelectID
        public static void SetSelectID(DependencyObject obj, Guid value)
        {
            obj.SetValue(SelectIDProperty, value);
        }

        public static Guid GetSelectID(DependencyObject obj)
        {
            return (Guid)obj.GetValue(SelectIDProperty);
        }


        public static readonly DependencyProperty SelectIDProperty = DependencyProperty.RegisterAttached("SelectID", typeof(Guid), typeof(Lasso), new PropertyMetadata(Guid.Empty));
        #endregion

        #region OriPosition
        public static void SetOriSize(DependencyObject obj, Vector value)
        {
            obj.SetValue(OriSizeProperty, value);
        }

        public static Vector GetOriSize(DependencyObject obj)
        {
            return (Vector)obj.GetValue(OriSizeProperty);
        }


        public static readonly DependencyProperty OriSizeProperty = DependencyProperty.RegisterAttached("OriPosition", typeof(Vector), typeof(Lasso));
        #endregion

        #region SurceInkCanvas
        public SurfaceInkCanvas SurfaceInkCanvasHost
        {
            private set;
            get;
        }


        #endregion

        public GestureResult CurrentGesture { private set; get; }
        public GestureResult LastOperationGesture { private set; get; }
        #endregion

        #region Public Method
        #region Init,Begin,End
        public void Init(SurfaceInkCanvas surfaceInkCanvasHost, Panel operatePanel)
        {
            if (this.SurfaceInkCanvasHost != null)
            {
                this.SurfaceInkCanvasHost.Strokes.StrokesChanged -= this.Source_StrokesChanged;
            }

            this.SurfaceInkCanvasHost = surfaceInkCanvasHost;
            if (this.SurfaceInkCanvasHost != null)
            {
                this.SurfaceInkCanvasHost.Strokes.StrokesChanged += this.Source_StrokesChanged;
            }

            this.OperatePanel = operatePanel;
        }

        /// <summary>
        /// Lasso behavior start
        /// </summary>
        /// <param name="selectedChangedAction">Callback for changing selected strokes</param>
        /// <param name="movingAction">Callback for moving stroke</param>
        /// <param name="movedAction">Callback for end moving stroke</param>
        /// <param name="startDrawingLassoFeedback">Callback for staring to draw lasso</param>
        /// <param name="drawingLassoFeedback">Callback for drawing lasso</param>
        /// <param name="endDrawingLassoFeedback">Callback for drawing lasso end</param>
        public void Begin(
                          Action<StrokesSelectedChangedArgs> selectedChangedAction,
                          Action<SelectionChangeArgs> movingAction,
                          Action<SelectionChangedArgs> movedAction,
                          Action<SelectionChangeArgs> scalingAction,
                          Action<SelectionChangedArgs> scaledAction,
                          Action<StartDrawingLassoArg> startDrawingLassoFeedback,
                          Action<DrawingLassoArg> drawingLassoFeedback,
                          Action<DrawingLassoArg> endDrawingLassoFeedback,
                          Action<ManipulationStartingEventArgs> gestureBeginAction,
                          Action<ManipulationDeltaEventArgs> gestureDeltaAction,
                          Action<ManipulationCompletedEventArgs> gestureDoneAction,
                          Action<Point> scratchGesture,
                          Action<Point> openMenuGesture,
                          Action closeMenuGesture
                        )
        {
            if (this.SurfaceInkCanvasHost != null && this.IsStart == false)
            {
                this.IsHitTestVisible = true;
                this.inkOperation.IsHitTestVisible = false;
                this.inkOperation.Visibility = Visibility.Visible;

                this.selectedStrokesChangedAction = selectedChangedAction;
                this.selectedMovingAction = movingAction;
                this.selectedMovedAction = movedAction;

                this.selectedScalingAction = scalingAction;
                this.selectedScaledAction = scaledAction;

                this.startDrawingLassoFeedback = startDrawingLassoFeedback;
                this.drawingLassoFeedback = drawingLassoFeedback;
                this.endDrawingLassoFeedback = endDrawingLassoFeedback;

                this.gestureDeltaAction = gestureDeltaAction;
                this.gestureBeginAction = gestureBeginAction;
                this.gestureDoneAction = gestureDoneAction;
                this.scratchGesture = scratchGesture;
                this.openMenuGesture = openMenuGesture;
                this.closeMenuGesture = closeMenuGesture;

                this.BindEvent();
                this.IsStart = true;
            }
        }

        public void End()
        {
            if (this.IsStart)
            {
                this.IsHitTestVisible = false;

                this.IsStart = false;
                this.inkOperation.IsManipulationEnabled = false;
                this.EndEvent();
            }
        }

        public void ClearLassco()
        {
            this.closeMenuGesture();
            this.RemoveCurrentSelectionStatus();
        }
        #endregion

        #region Sync up
        /// <summary>
        /// Sync up the strokes selected changed
        /// </summary>
        public void SyncStrokesSelected(Guid behaviorid, IEnumerable<string> synSelectStrokeIds, Guid oldBehaviorId, IEnumerable<string> synOldSelectStrokeIds)
        {
            if (oldBehaviorId != Guid.Empty)
            {
                this.ClearFeedbackSelectionBox(oldBehaviorId);
            }

            if (synOldSelectStrokeIds != null)
            {
                StrokeCollection collection = GetStrokeCollectionByIds(this.SurfaceInkCanvasHost.Strokes, synOldSelectStrokeIds);

                this.SetStrokesSelectedBySync(collection, false);
            }

            if (synSelectStrokeIds != null)
            {
                StrokeCollection collection = GetStrokeCollectionByIds(this.SurfaceInkCanvasHost.Strokes, synSelectStrokeIds);

                SelectionBox box = this.AddSelectionBox(behaviorid, collection.GetBounds(), synSelectStrokeIds, false);
                box.IsHitTestVisible = false;

                this.SetStrokesSelectedBySync(collection, true);
            }
        }

        /// <summary>
        ///  Sync up the "Moving behavior" come on
        /// </summary>
        public void SyncStrokesMoving(Guid behaviorid, IEnumerable<string> syncSelectStrokeIds, double offsetX, double offsetY)
        {
            if (this.SurfaceInkCanvasHost != null)
            {
                if (this.touchBehaviorStatus.ContainsKey(behaviorid) == false)
                {
                    this.touchBehaviorStatus.Add(behaviorid, TouchBehaviorStatus.MovingStart);
                }
                else
                {
                    this.touchBehaviorStatus[behaviorid] = TouchBehaviorStatus.Moving;
                }

                StrokeCollection collection = this.GetStrokeCollectionByIds(this.SurfaceInkCanvasHost.Strokes, syncSelectStrokeIds);
                if (this.touchBehaviorStatus[behaviorid] == TouchBehaviorStatus.MovingStart)
                {
                    this.AdjustCurrentSelection(behaviorid, syncSelectStrokeIds);

                    this.SetIsOperatingToStrokes(collection, true);
                }

                this.SyncPos(behaviorid, offsetX, offsetY, collection);
            }
        }


        /// <summary>
        /// Sync up the "Moving behavior" is end
        /// </summary>
        /// <summary>
        /// Sync up the "Moving behavior" is end
        /// </summary>
        public void SyncStrokesMoved(Guid behaviorid, IEnumerable<string> syncSelectStrokeIds, double offsetX, double offsetY)
        {
            if (this.SurfaceInkCanvasHost != null)
            {
                if (this.touchBehaviorStatus.ContainsKey(behaviorid))
                {
                    this.touchBehaviorStatus.Remove(behaviorid);
                }

                StrokeCollection collection = this.GetStrokeCollectionByIds(this.SurfaceInkCanvasHost.Strokes, syncSelectStrokeIds);

                this.SyncPos(behaviorid, offsetX, offsetY, collection);
                this.AdjustOriStrokes(syncSelectStrokeIds, offsetX, offsetY);
                Trace.WriteLine("Seting syn moving SyncStrokesMoved");
                this.SetIsOperatingToStrokes(collection, false);
            }
        }



        public void SyncStrokesScaling(Guid behaviorid, IEnumerable<string> syncSelectStrokeIds, double offsetX, double offsetY)
        {
            if (this.SurfaceInkCanvasHost != null)
            {
                if (this.touchBehaviorStatus.ContainsKey(behaviorid) == false)
                {
                    this.touchBehaviorStatus.Add(behaviorid, TouchBehaviorStatus.ScalingStart);
                }
                else
                {
                    this.touchBehaviorStatus[behaviorid] = TouchBehaviorStatus.Scaling;
                }

                StrokeCollection collection = this.GetStrokeCollectionByIds(this.SurfaceInkCanvasHost.Strokes, syncSelectStrokeIds);
                if (this.touchBehaviorStatus[behaviorid] == TouchBehaviorStatus.ScalingStart)
                {
                    this.AdjustCurrentSelection(behaviorid, syncSelectStrokeIds);

                    this.SetIsOperatingToStrokes(collection, true);
                }

                SelectionBox targetBox = this.GetSelectionBoxById(behaviorid);

                double newOffsetX = offsetX / targetBox.ActualWidth;
                double newOffsetY = offsetY / targetBox.ActualHeight;

                OnScaleSelection(newOffsetX, newOffsetX, targetBox, collection);
            }
        }


        public void SyncStrokesScaled(Guid behaviorid, IEnumerable<string> syncSelectStrokeIds, double offsetX, double offsetY)
        {
            if (this.SurfaceInkCanvasHost != null)
            {
                if (this.touchBehaviorStatus.ContainsKey(behaviorid))
                {
                    this.touchBehaviorStatus.Remove(behaviorid);
                }

                StrokeCollection collection = this.GetStrokeCollectionByIds(this.SurfaceInkCanvasHost.Strokes, syncSelectStrokeIds);
                SelectionBox targetBox = this.GetSelectionBoxById(behaviorid);

                double newOffsetX = offsetX / targetBox.ActualWidth;
                double newOffsetY = offsetY / targetBox.ActualHeight;

                OnScaleSelection(newOffsetX, newOffsetX, targetBox, collection);
                this.SetIsOperatingToStrokes(collection, false);
            }
        }

        /// <summary>
        /// Sync up start to draw lasso 
        /// </summary>
        public void SyncStartDrawingLasso(Guid behaviorid, double offsetX, double offsetY)
        {
            StylusPointCollection collection = new StylusPointCollection();
            collection.Add(new StylusPoint(offsetX, offsetY));
            Stroke lassoStroke = new DashStroke(collection);
            lassoStroke.DrawingAttributes.IsHighlighter = true;
            lassoStroke.DrawingAttributes.Color = this.lassoColor;

            lassoStroke.SetID(behaviorid.ToString());

            this.inkOperation.Strokes.Add(lassoStroke);

            this.AddLassoFeedback(behaviorid, offsetX, offsetY);
        }


        /// <summary>
        /// Sync up draing lasso
        /// </summary>
        public void SyncDrawingLasso(Guid behaviorid, IEnumerable<Vector> stylusPoints)
        {
            string targetId = behaviorid.ToString();
            Stroke lassoStroke = this.inkOperation.Strokes.FirstOrDefault(item => item.GetID() == targetId);
            if (lassoStroke != null)
            {
                foreach (var item in stylusPoints)
                {
                    lassoStroke.StylusPoints.Add(new StylusPoint(item.X, item.Y));
                }
            }
            Vector lastPos = stylusPoints.Last();
            this.MovingLassoFeedback(behaviorid, lastPos.X, lastPos.Y);
        }

        /// <summary>
        /// Sync up end for drawing lasso
        /// </summary>
        public void SyncEndDrawingLasso(Guid behaviorid)
        {
            string targetId = behaviorid.ToString();
            Stroke lassoStroke = this.inkOperation.Strokes.FirstOrDefault(item => item.GetID() == targetId);
            if (lassoStroke != null)
            {
                this.inkOperation.Strokes.Remove(lassoStroke);
                this.EndMovingLassoFeedback(behaviorid);
            }

        }



        #endregion

        #region public methods : ResetStates
        public void ResetStates()
        {
            this.RemoveCurrentSelectionStatus();
            this.RemoveFeedbackSelection();
            if (this.SurfaceInkCanvasHost != null)
            {
                this.SetIsOperatingToStrokes(this.SurfaceInkCanvasHost.Strokes, false);
            }
        }
        #endregion
        #endregion

        #region Private

        #region RegistEvent
        private void BindEvent()
        {

            this.TouchDown += BehaviorStart;
            this.TouchMove += BehaviorContine;
            //this.gdRoot.PreviewTouchUp += PreviewBehaviorEnd;
            this.TouchUp += BehaviorEnd;

            this.ManipulationStarting += this.GestureManipulationStarting;
            this.ManipulationDelta += this.GestureManipulationDelta;
            this.ManipulationCompleted += GestureManipulationCompleted;
        }

        private void EndEvent()
        {

            this.TouchDown -= BehaviorStart;
            this.TouchUp -= BehaviorEnd;
            this.TouchMove -= BehaviorContine;
            this.ManipulationDelta -= this.GestureManipulationDelta;
            this.ManipulationCompleted -= this.GestureManipulationCompleted;
        }
        #endregion

        #region Operation Strokes
        #region Gesture
        void GestureManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            this.LastOperationGesture = GestureResult.UnKnown;
            this.gestureBeginAction(e);
        }


        void GestureManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            this.gestureDoneAction(e);
        }


        void GestureManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            int touchCount = this.gdRoot.TouchesOver.Count();
            int touchSelectionCount = this.mainSelectionBox == null ? 0 : this.mainSelectionBox.TouchesOver.Count();
            if (touchCount != touchSelectionCount)
            {
                if (this.isMovingSelection == false && this.isScalingSelection == false)
                {
                    this.gestureDeltaAction(e);
                }
            }
        }
        #endregion

        public void BehaviorStart(object sender, TouchEventArgs e)
        {
            lock (synActionObj)
            {
                int touchCount = this.gdRoot.TouchesOver.Count();

                this.lastTouchPosition = e.GetTouchPoint(this).Position;

                if (touchCount > 1)
                {
                    //Gesture behavior
                    this.BehaviorEndDrawingLasso(e.TouchDevice.Id);
                    return;
                }
                else if (touchCount == 1)
                {
                    this.closeMenuGesture();
                    this.RemoveCurrentSelectionStatus();
                }

            }
        }


        /// <summary>
        /// when stylus moing,if "touch down" on "Select Rectangle",then start to moving all the stroke and annocing it to other cilent,or start to "Select behavior"(drawing lasso,and annocing it to other clinet)
        /// </summary>
        public void BehaviorContine(object sender, TouchEventArgs e)
        {
            int touchCount = this.gdRoot.TouchesOver.Count();
            int touchOnSelectionOnBox = this.mainSelectionBox == null ? 0 : this.mainSelectionBox.TouchesOver.Count();

            if (touchCount == touchOnSelectionOnBox)
            {
                return;
            }

            if (this.isGestureBehavior && this.isDrawingLasso)
            {
                this.BehaviorEndDrawingLasso(e.TouchDevice.Id);
                return;
            }

            TouchPoint point = e.GetTouchPoint(this);
            if (this.IsStart && this.isMovingSelection == false)
            {

                if (this.isDrawingLasso && e.TouchDevice.Id == this.drawingLassoTouchId)
                {
                    var interPoints = e.GetIntermediateTouchPoints(this);
                    foreach (var ip in interPoints)
                    {
                        movingLassoData.Add(new Vector(ip.Position.X, ip.Position.Y));
                    }

                    //Announce the other client that Lassco is been drawing
                    //movingLassoData.Add(new Vector(point.Position.X, point.Position.Y));

                    if (movingLassoData.Count > SYNCDATACOUNT)
                    {
                        DrawingLassoArg args = new DrawingLassoArg(this.drawingLassoId, movingLassoData);
                        this.drawingLassoFeedback(args);

                        this.DrawingLasso(this.drawingLassoId, movingLassoData);
                        movingLassoData.Clear();
                    }
                }

            }

        }

        public void BehaviorEnd(object sender, TouchEventArgs e)
        {
            Debug.WriteLine("Lassco BehaviorEnd");
            this.BehaviorEndDrawingLasso(e.TouchDevice.Id);
        }

        private void HandelGobalGesture()
        {
            this.AttachGestureDetector((arg) =>
            {
                if (arg.Result == GestureResult.Hold)
                {
                    this.isGestureBehavior = false;
                    if (this.mainSelectionBox != null && this.mainSelectionBox.TouchesOver.Count() > 0)
                    {
                        return;
                    }
                    Point oriPoint = this.TouchesOver.First().GetTouchPoint(this).Position;
                    this.openMenuGesture(oriPoint);
                }
                else if (arg.Result == GestureResult.FiveFingersScratch)
                {
                    this.isGestureBehavior = true;
                    this.scratchGesture(arg.Device.GetTouchPoint(Application.Current.MainWindow).Position);
                }
                else if (arg.Result == GestureResult.End)
                {
                    this.isGestureBehavior = false;

                    this.BehaviorEndMovingSelection();
                    this.BehaviorEndScalingSelection();
                }
                else if (arg.Result == GestureResult.OneFinger || arg.Result == GestureResult.Drag || arg.Result == GestureResult.Zoom)
                {
                    this.HandelGestureOnSelectionBox(arg);

                    this.HandelGesureOnDrawingLasso(arg);
                }

                if (arg.Result != GestureResult.End)
                {
                    this.LastOperationGesture = arg.Result;
                }
                this.CurrentGesture = arg.Result;

            },
              (arg, detailArg, cumulativeArg) =>
              {
                  this.HandGesturDetailOnSelectionBox(arg, detailArg, cumulativeArg);
              }
            );
            this.ManipulationStarting += new EventHandler<ManipulationStartingEventArgs>(Lasso_ManipulationStarting);
        }

        void Lasso_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this.OperatePanel;
        }

        private void HandelGesureOnDrawingLasso(GestureArg arg)
        {
            if (arg.Result == GestureResult.OneFinger)
            {
                int touchCount = this.gdRoot.TouchesOver.Count();
                int touchSelectionCount = this.mainSelectionBox == null ? 0 : this.mainSelectionBox.TouchesOver.Count();

                if (touchCount == 1 && touchCount != touchSelectionCount)
                {
                    if (this.isDrawingLasso == false)
                    {
                        this.drawingLassoTouchId = arg.Device.Id;
                        this.LocalPrepareToDrawingLasso();
                        this.isDrawingLasso = true;
                    }

                }
            }
        }

        private void HandGesturDetailOnSelectionBox(GestureArg arg, System.Windows.Input.ManipulationDelta detailArg, System.Windows.Input.ManipulationDelta cumulativeArg)
        {
            if (this.mainSelectionBox != null)
            {
                int touchCount = this.gdRoot.TouchesOver.Count();
                int touchSelectionCount = this.mainSelectionBox.TouchesOver.Count();
                if (touchCount == touchSelectionCount)
                {
                    if (arg.Result == GestureResult.Zoom)
                    {
                        this.ScaleSelection(detailArg);
                    }
                    else if (arg.Result == GestureResult.Drag || arg.Result == GestureResult.OneFinger)
                    {
                        this.MovingSelection(detailArg);
                    }
                }
            }
        }


        private void HandelGestureOnSelectionBox(GestureArg arg)
        {
            if (this.mainSelectionBox != null)
            {
                int touchCount = this.gdRoot.TouchesOver.Count();
                int touchSelectionCount = this.mainSelectionBox.TouchesOver.Count();

                if (touchCount == touchSelectionCount)
                {
                    if (this.SelectionStrokes != null && this.SelectionStrokes.Count > 0)
                    {
                        IEnumerable<string> ids = this.SelectionStrokes.Select(item => item.GetID());

                        if (arg.Result == GestureResult.Zoom)
                        {
                            this.isScalingSelection = true;
                            this.scaleSelectionCount = 0;
                        }
                        else
                        {
                            this.AdjustSelectionBox(this.selectionBehaviorId, ids, this.mainSelectionBox);
                            this.SetIsOperatingToStrokes(this.SelectionStrokes, true);

                            this.isMovingSelection = true;
                            this.moveSelectionCount = 0;
                        }
                    }
                }
                else
                {
                    if (arg.Result == GestureResult.Drag)
                    {
                        this.isGestureBehavior = true;
                    }
                }
            }
            else
            {
                if (arg.Result == GestureResult.Drag)
                {
                    this.isGestureBehavior = true;
                }
            }
        }

        private void BehaviorEndDrawingLasso(int touchId)
        {
            if (this.isDrawingLasso && (touchId < 0 || touchId == this.drawingLassoTouchId))
            {
                this.EndDrawingLasso(this.drawingLassoId);

                this.isDrawingLasso = false;

                DrawingLassoArg args = new DrawingLassoArg(this.drawingLassoId, null);
                this.endDrawingLassoFeedback(args);

            }
        }

        private void BehaviorEndMovingSelection()
        {
            if (this.isMovingSelection)
            {
                this.EndMovingSelection();

                this.isMovingSelection = false;
            }
        }

        private void BehaviorEndScalingSelection()
        {
            if (this.isScalingSelection)
            {
                this.EndScaleSelection();

                this.isScalingSelection = false;
            }
        }

        void EndMovingSelection()
        {
            if (IsStart && this.selectedMovedAction != null)
            {
                Trace.WriteLine("EndMovingSelection");
                StrokeCollection collection = this.SelectionStrokes;
                if (collection != null && collection.Count > 0)
                {
                    this.SetIsOperatingToStrokes(collection, false);
                    IEnumerable<string> ids = collection.Select(item => item.GetID());

                    SelectionChangedArgs arg = new SelectionChangedArgs(this.selectionBehaviorId, collection, this.moveChangedOffset);
                    this.selectedMovedAction(arg);
                }
            }
        }

        void EndScaleSelection()
        {
            if (IsStart && this.selectedScaledAction != null)
            {
                Trace.WriteLine("EndMovingSelection");
                StrokeCollection collection = this.SelectionStrokes;
                if (collection.Count > 0)
                {
                    this.SetIsOperatingToStrokes(collection, false);
                    this.RaiseScaleChanged();
                }
            }
        }
        #endregion

        #region Update Strokes Postion
        /// <summary>
        /// Update the position of source collection
        /// </summary>
        void AdjustOriStrokes(IEnumerable<string> ids, double offsetX, double offsetY)
        {
            IEnumerable<Stroke> sourceStrokes = this.currentOperationStrokes.Where(item => ids.Contains(item.GetID()));

            if (sourceStrokes != null)
            {
                StrokeCollection collection = new StrokeCollection(sourceStrokes);
                this.MoveStrokes(collection, offsetX, offsetY);
            }
        }


        /// <summary>
        /// Update the position of collection
        /// </summary>
        void MoveStrokes(StrokeCollection collection, double offsetX, double offsetY)
        {
            Matrix renderMatrix = new Matrix();
            renderMatrix.Translate(offsetX, offsetY);

            collection.Transform(renderMatrix, false);
        }


        #endregion

        #region SelectionBox
        /// <summary>
        /// Create the feedback rectangle for stroke collection which were selected
        /// </summary>
        SelectionBox CreateSelectionBox(Rect bound)
        {
            if (bound != Rect.Empty)
            {
                SelectionBox box = new SelectionBox();
                this.SetSelectionBox(box, bound);
                box.BorderColor = this.lassoColor;
                return box;
            }
            return null;
        }

        void SetSelectionBox(SelectionBox box, Rect bound)
        {
            box.Width = bound.Width + RECOFFSET;
            box.Height = bound.Height + RECOFFSET;
            Canvas.SetTop(box, bound.Top - RECOFFSET / 2);
            Canvas.SetLeft(box, bound.Left - RECOFFSET / 2);

            Lasso.SetOriSize(box, new Vector(box.Width, box.Height));
        }

        /// <summary>
        ///Add the feedback rectangle for target behavior
        /// </summary>
        SelectionBox AddSelectionBox(Guid behaviorId, Rect bound, IEnumerable<string> attachids, bool isMovable = true)
        {
            SelectionBox selectionBox = this.CreateSelectionBox(bound);

            if (selectionBox != null)
            {
                Lasso.SetSelectID(selectionBox, behaviorId);
                selectionBox.Opacity = isMovable ? 1 : 0.4;
                selectionBox.IsMovable = isMovable;
                Canvas.SetZIndex(selectionBox, isMovable == true ? 100 : 0);
                this.cvsFeedback.Children.Add(selectionBox);
                foreach (var item in attachids)
                {
                    this.SetSelectionBoxAttachStrokes(selectionBox, item);
                }
            }
            return selectionBox;
        }


        /// <summary>
        /// Clear the rectangle for target behavior
        /// </summary>
        /// <param name="behaviorId"></param>
        void ClearFeedbackSelectionBox(Guid behaviorId)
        {
            SelectionBox targetBox = null;
            foreach (var item in this.cvsFeedback.Children)
            {
                SelectionBox box = item as SelectionBox;
                if (box != null)
                {
                    Guid recId = Lasso.GetSelectID(box);
                    if (recId == behaviorId)
                    {
                        targetBox = box;
                        if (targetBox != this.mainSelectionBox)
                        {
                            break;
                        }
                    }
                }
            }
            if (targetBox != null)
            {
                this.cvsFeedback.Children.Remove(targetBox);
            }
        }

        /// <summary>
        /// Update the position of selected rectange by behavior id
        /// </summary>
        void MovingSelectionBoxById(Guid behaviorId, double offsetX, double offsetY)
        {
            SelectionBox box = this.GetSelectionBoxById(behaviorId);
            if (box != null)
            {
                this.MovingSelectionBox(box, offsetX, offsetY);
            }
        }



        void MovingSelectionBox(SelectionBox box, double offsetX, double offsetY)
        {
            Vector currentPos = new Vector(box.GetLeft(), box.GetTop());
            Canvas.SetLeft(box, currentPos.X + offsetX);
            Canvas.SetTop(box, currentPos.Y + offsetY);
        }


        void CheckNewPos(double offsetX, double offsetY, Vector currentPos, double areaWidth, double areaHeight, out double targetXPos, out double targetYPos)
        {
            targetYPos = 1.0;
            targetXPos = 1.0;
        }



        /// <summary>
        /// adjust the target stroke belong to the rectangle,if no stroke blong to this.rectangle,remove this rectangle too
        /// </summary>
        void AdjustSelectionBox(Guid behaviorId, IEnumerable<string> strokeids, SelectionBox exceptionBox = null)
        {
            List<SelectionBox> boxs = new List<SelectionBox>();
            foreach (var item in this.cvsFeedback.Children)
            {
                SelectionBox box = item as SelectionBox;
                if (box != null)
                {
                    if (behaviorId != Guid.Empty && Lasso.GetSelectID(box) == behaviorId)
                    {
                        continue;
                    }
                    boxs.Add(box);
                }
            }

            foreach (var strokeId in strokeids)
            {
                foreach (SelectionBox box in boxs)
                {
                    if (exceptionBox != null && box == exceptionBox)
                    {
                        continue;
                    }
                    this.ClearAttachStrokesOnSelectionBox(box, strokeId);
                }
            }

        }

        void AdjustCurrentSelection(Guid behaviorid, IEnumerable<string> syncSelectStrokeIds)
        {
            if (this.SelectionStrokes != null && this.SelectionStrokes.Count > 0)
            {
                IEnumerable<Stroke> leastSelectedStrokes = this.SelectionStrokes.Where(item => syncSelectStrokeIds.Contains(item.GetID().ToString()) == false);
                int changeCount = this.SelectionStrokes.Count - leastSelectedStrokes.Count();
                if (changeCount > 0)
                {
                    StrokeCollection leastSelected = new StrokeCollection();

                    foreach (var item in leastSelectedStrokes)
                    {
                        leastSelected.Add(item);
                    }
                    this.SelectionStrokesChanged(leastSelected);
                }
            }
            SelectionBox box = this.GetSelectionBoxById(behaviorid);
            if (box != null)
            {
                //If the behavior status is "start",check the selected strokes if which were selected by other cilent,if so,clear the "control right" on other client 
                this.AdjustSelectionBox(behaviorid, syncSelectStrokeIds, box);
            }
        }

        /// <summary>
        /// Make the stroke refrence attach the target rectangle
        /// </summary>
        void SetSelectionBoxAttachStrokes(SelectionBox selectionBox, string strokeId)
        {
            List<string> attachStrokeIds = GetSelectionBoxAttachedStrokes(selectionBox);
            attachStrokeIds.Add(strokeId);
        }

        /// <summary>
        /// Remove the target stroke belong to the rectangle,if no stroke blong to this.rectangle,remove this rectangle too
        /// </summary>
        void ClearAttachStrokesOnSelectionBox(SelectionBox box, string strokeId)
        {
            List<string> attachStrokes = GetSelectionBoxAttachedStrokes(box);

            if (attachStrokes.Count > 0)
            {
                if (attachStrokes.Contains(strokeId))
                {
                    attachStrokes.Remove(strokeId);
                    if (attachStrokes.Count > 0)
                    {
                        StrokeCollection collection = this.GetStrokeCollectionByIds(this.SurfaceInkCanvasHost.Strokes, attachStrokes);
                        Rect bound = collection.GetBounds();
                        if (bound != Rect.Empty)
                        {
                            Vector newOriPos = new Vector(bound.X, bound.Y);
                            box.Width = bound.Width + RECOFFSET;
                            box.Height = bound.Height + RECOFFSET;
                            Canvas.SetLeft(box, newOriPos.X - RECOFFSET / 2);
                            Canvas.SetTop(box, newOriPos.Y - RECOFFSET / 2);
                        }
                        else
                        {
                            this.cvsFeedback.Children.Remove(box);
                        }
                    }
                    else
                    {
                        this.cvsFeedback.Children.Remove(box);
                    }
                }
            }

        }

        /// <summary>
        /// Get the Stroke Id list of feedback rectangle
        /// </summary>
        List<string> GetSelectionBoxAttachedStrokes(SelectionBox box)
        {
            List<string> attachStrokeIds = null;
            if (box.Tag == null)
            {
                attachStrokeIds = new List<string>();
                box.Tag = attachStrokeIds;
            }
            else
            {
                attachStrokeIds = box.Tag as List<string>;
                Debug.Assert(attachStrokeIds != null, "the tag of rectangle feedback shold be StrokeCollection type");

            }
            return attachStrokeIds;
        }

        void ClearAllAttachStrokesOnSelectionBox(SelectionBox box)
        {
            List<string> attachStrokeIds = this.GetSelectionBoxAttachedStrokes(box);
            if (attachStrokeIds != null)
            {
                attachStrokeIds.Clear();
            }
        }
        #endregion

        #region Drawing Lasso

        void LocalPrepareToDrawingLasso()
        {

            //Annocing other cilent that "I am draing lasso"
            this.drawingLassoId = Guid.NewGuid();

            movingLassoData.Clear();

            this.StartDrawingLasso(this.drawingLassoId, this.lastTouchPosition.X, this.lastTouchPosition.Y);

            StartDrawingLassoArg args = new StartDrawingLassoArg(this.drawingLassoId, new Vector(this.lastTouchPosition.X, this.lastTouchPosition.Y));
            this.startDrawingLassoFeedback(args);

        }

        void StartDrawingLasso(Guid behaviorid, double offsetX, double offsetY)
        {
            StylusPointCollection collection = new StylusPointCollection();
            collection.Add(new StylusPoint(offsetX, offsetY));
            Stroke lassoStroke = new DashStroke(collection);

            lassoStroke.DrawingAttributes.Color = this.lassoColor;

            lassoStroke.SetID(behaviorid.ToString());

            this.inkOperation.Strokes.Add(lassoStroke);
        }

        void DrawingLasso(Guid behaviorid, IEnumerable<Vector> stylusPoints)
        {
            string targetId = behaviorid.ToString();
            Stroke lassoStroke = this.inkOperation.Strokes.FirstOrDefault(item => item.GetID() == targetId);
            if (lassoStroke != null)
            {
                foreach (var item in stylusPoints)
                {
                    lassoStroke.StylusPoints.Add(new StylusPoint(item.X, item.Y));
                }
            }
        }

        void EndDrawingLasso(Guid behaviorid)
        {
            string targetId = behaviorid.ToString();
            Stroke lassoStroke = this.inkOperation.Strokes.FirstOrDefault(item => item.GetID() == targetId);
            if (lassoStroke != null)
            {
                StrokeCollection selectedCollection = GetSelectionStrokes(lassoStroke);

                this.SelectionStrokesChanged(selectedCollection);

                this.MovingSelectionStart();

                this.inkOperation.Strokes.Remove(lassoStroke);
            }
        }

        StrokeCollection GetSelectionStrokes(Stroke lassoStroke)
        {
            IEnumerable<Point> test = lassoStroke.StylusPoints.Select(item => item.ToPoint());

            StrokeCollection selectedCollection = new StrokeCollection();
            foreach (Stroke stroke in this.SurfaceInkCanvasHost.Strokes)
            {
                if (stroke.GetIsOperating() == false && stroke.GetIsEnabled() == true && stroke.HitTest(test, 80))
                {
                    selectedCollection.Add(stroke);
                }
            }
            return selectedCollection;
        }

        void SelectionStrokesChanged(StrokeCollection selectedCollection)
        {
            if (this.selectedStrokesChangedAction != null)
            {
                IEnumerable<string> oldIds = null;
                Guid oldSelectedId = Guid.Empty;

                IEnumerable<string> newIds = null;
                Guid newSelectedId = Guid.Empty;

                if (this.SelectionStrokes != null && this.selectionBehaviorId != Guid.Empty)
                {
                    oldIds = this.SelectionStrokes.Select(item => item.GetID());
                    oldSelectedId = this.selectionBehaviorId;

                }

                SelectionStrokes = selectedCollection;

                if (SelectionStrokes != null && SelectionStrokes.Count > 0)
                {
                    this.selectionBehaviorId = Guid.NewGuid();
                    newSelectedId = this.selectionBehaviorId;

                    newIds = this.SelectionStrokes.Select(item => item.GetID());

                }

                if ((oldSelectedId != Guid.Empty && oldIds != null && oldIds.Count() > 0) || (newIds != null && newIds != null && newIds.Count() > 0))
                {
                    StrokesSelectedChangedArgs args = new StrokesSelectedChangedArgs(this.selectionBehaviorId, oldSelectedId, newIds, oldIds);
                    this.selectedStrokesChangedAction(args);

                }
            }
        }
        #endregion

        #region Moving Selection
        private void MovingSelectionStart()
        {
            //Add the selection box to attatch the selection strokes
            if (this.SelectionStrokes != null && this.SelectionStrokes.Count > 0)
            {
                IEnumerable<string> ids = this.SelectionStrokes.Select(item => item.GetID());
                Rect bound = this.SelectionStrokes.GetBounds();

                if (this.mainSelectionBox == null)
                {
                    this.mainSelectionBox = this.AddSelectionBox(this.selectionBehaviorId, bound, ids);
                    Canvas.SetZIndex(this.mainSelectionBox, 100);
                }
                else
                {
                    this.mainSelectionBox.Visibility = Visibility.Visible;
                    if (this.mainSelectionBox.Parent == null)
                    {
                        this.cvsFeedback.Children.Add(this.mainSelectionBox);
                    }
                    this.SetSelectionBox(this.mainSelectionBox, bound);
                    this.ClearAllAttachStrokesOnSelectionBox(this.mainSelectionBox);
                    foreach (var item in ids)
                    {
                        this.SetSelectionBoxAttachStrokes(this.mainSelectionBox, item);
                    }
                }

            }
            else
            {
                if (this.mainSelectionBox != null)
                {
                    this.mainSelectionBox.Visibility = Visibility.Collapsed;
                }
            }
        }


        private void MovingSelection(System.Windows.Input.ManipulationDelta detailArg)
        {
            if (this.mainSelectionBox.IsVisible)
            {
                if (this.SelectionStrokes != null)
                {
                    IEnumerable<string> ids = this.SelectionStrokes.Select(item => item.GetID());
                    MatrixTransform currentTransform = this.OperatePanel.RenderTransform as MatrixTransform;

                    if (currentTransform != null)
                    {
                        double offsetX = detailArg.Translation.X / currentTransform.Matrix.M11;
                        double offsetY = detailArg.Translation.Y / currentTransform.Matrix.M22;

                        this.MovingSelectionBox(this.mainSelectionBox, offsetX, offsetY);
                        this.MoveStrokes(this.SelectionStrokes, offsetX, offsetY);

                        this.moveSelectionCount++;
                        if (this.moveSelectionCount > SYNCDATACOUNT)
                        {
                            this.moveChangedOffset = new Vector(this.mainSelectionBox.GetLeft(), this.mainSelectionBox.GetTop());

                            SelectionChangeArgs arg = new SelectionChangeArgs(this.selectionBehaviorId, ids, this.moveChangedOffset);
                            this.selectedMovingAction(arg);
                            this.moveSelectionCount = 0;
                        }
                    }
                }
                else
                {
                    Debug.Assert(false, "SelectionStrokes Should not be null");
                    this.ResetStates();
                }
            }
        }
        #endregion

        #region ScaleSelection
        private void ScaleSelection(System.Windows.Input.ManipulationDelta detailArg)
        {
            Vector targetScaleValue = GetMendedScaleValue(detailArg);

            if (targetScaleValue.X != 1 && targetScaleValue.Y != 1)
            {
                this.OnScaleSelection(targetScaleValue.X, targetScaleValue.Y, this.mainSelectionBox, this.SelectionStrokes);

                this.scaleSelectionCount++;
                if (this.scaleSelectionCount > SYNCDATACOUNT)
                {
                    this.RaiseScaleChanging();
                    this.scaleSelectionCount = 0;
                }
            }
        }

        private void RaiseScaleChanging()
        {
            IEnumerable<string> ids = this.SelectionStrokes.Select(item => item.GetID());

            this.scaleChangedOffset = new Vector(this.mainSelectionBox.Width, this.mainSelectionBox.Height);
            SelectionChangeArgs arg = new SelectionChangeArgs(this.selectionBehaviorId, ids, this.scaleChangedOffset);
            this.selectedScalingAction(arg);
        }

        private void RaiseScaleChanged()
        {
            IEnumerable<string> ids = this.SelectionStrokes.Select(item => item.GetID());

            this.scaleChangedOffset = new Vector(this.mainSelectionBox.Width, this.mainSelectionBox.Height);
            SelectionChangedArgs arg = new SelectionChangedArgs(this.selectionBehaviorId, this.SelectionStrokes, this.scaleChangedOffset);
            this.selectedScaledAction(arg);
        }

        private void OnScaleSelection(double offsetX, double offsetY, SelectionBox box, StrokeCollection targetStrokes)
        {

            double height = box.Height;
            double width = box.Width;

            box.Height = height * offsetY;
            box.Width = width * offsetX;

            double diffHeight = box.Height - height;
            double diffWidth = box.Width - width;

            double currentX = box.GetLeft();
            double currentY = box.GetTop();

            box.SetLeft(currentX - diffWidth / 2);
            box.SetTop(currentY - diffHeight / 2);

            Matrix inkTransform = new Matrix();
            Rect inkBounds = targetStrokes.GetBounds();
            Point center = new Point(0.5f * (inkBounds.Left + inkBounds.Right),
                                       0.5f * (inkBounds.Top + inkBounds.Bottom));

            inkTransform.ScaleAt(offsetX, offsetY, center.X, center.Y);
            targetStrokes.Transform(inkTransform, false);

        }

        private Vector GetMendedScaleValue(System.Windows.Input.ManipulationDelta detailArg)
        {
            Vector targetScaleValue = new Vector(1, 1);
            Vector oriSize = Lasso.GetOriSize(this.mainSelectionBox);
            Vector scaleValue = new Vector(this.mainSelectionBox.Width / oriSize.X, this.mainSelectionBox.Height / oriSize.Y);
            bool isXInBound = scaleValue.X > minScale && scaleValue.X < maxScale;
            bool isYInBound = scaleValue.Y > minScale && scaleValue.Y < maxScale;
            if ((scaleValue.X <= minScale && detailArg.Scale.X > 1)
                || (scaleValue.X >= maxScale && detailArg.Scale.X < 1)
                || isXInBound)
            {
                targetScaleValue.X = detailArg.Scale.X;
            }

            if ((scaleValue.Y <= minScale && detailArg.Scale.Y > 1)
                || (scaleValue.Y >= maxScale && detailArg.Scale.Y < 1)
            || isYInBound)
            {
                targetScaleValue.Y = detailArg.Scale.Y;
            }
            return targetScaleValue;
        }
        #endregion

        #region Lasso Feedback
        void AddLassoFeedback(Guid behaviorid, double offsetX, double offsetY)
        {
            DrawingFeedback feedback = new DrawingFeedback();
            Lasso.SetSelectID(feedback, behaviorid);
            //feedback.Width = 190;
            //feedback.Height = 184;

            feedback.FeedbackType = PenMenuCommand.LassoSelected;


            MatrixTransform currentTransform = this.OperatePanel.RenderTransform as MatrixTransform;

            if (currentTransform != null)
            {
                double scaleRage = 1 / currentTransform.Matrix.M11;
                Matrix matri = new Matrix(scaleRage, 0, 0, 1.0, 0, 0);
                feedback.RenderTransformOrigin = new Point(0.0, 0.0);
                ScaleTransform scl = new ScaleTransform(scaleRage, scaleRage);
                feedback.RenderTransform = scl;

                Style style = Application.Current.Resources["LassoDrawingFeedback"] as Style;
                feedback.Style = style;

            }

            Canvas.SetTop(feedback, offsetY);
            Canvas.SetLeft(feedback, offsetX);

            this.cvsFeedback.Children.Add(feedback);
        }

        void MovingLassoFeedback(Guid behaviorid, double offsetX, double offsetY)
        {
            DrawingFeedback targetFeedback = FindLassoFeedback(behaviorid);

            if (targetFeedback != null)
            {
                Canvas.SetTop(targetFeedback, offsetY);
                Canvas.SetLeft(targetFeedback, offsetX);
            }
        }

        void EndMovingLassoFeedback(Guid behaviorid)
        {
            DrawingFeedback targetFeedback = FindLassoFeedback(behaviorid);
            if (targetFeedback != null)
            {
                this.cvsFeedback.Children.Remove(targetFeedback);
            }
        }

        DrawingFeedback FindLassoFeedback(Guid behaviorid)
        {
            DrawingFeedback targetFeedback = null;
            foreach (var item in this.cvsFeedback.Children)
            {
                DrawingFeedback feedback = item as DrawingFeedback;
                if (feedback != null && Lasso.GetSelectID(feedback) == behaviorid)
                {
                    targetFeedback = feedback;
                    break;
                }

            }
            return targetFeedback;
        }
        #endregion

        #region Source Strokes Changed
        /// <summary>
        /// When the source changed,update the lasso data(add or romove stroke to operation layer)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Source_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            List<string> removedStrokesIds = new List<string>();
            foreach (var stroke in e.Removed)
            {
                string id = stroke.GetID();
                Stroke existStroke = this.currentOperationStrokes.FirstOrDefault(item => id == item.GetID());
                if (existStroke != null)
                {
                    this.currentOperationStrokes.Remove(existStroke);
                    removedStrokesIds.Add(existStroke.GetID());

                    stroke.StylusPointsChanged -= new EventHandler(stroke_StylusPointsChanged);
                    stroke.DrawingAttributesReplaced -= stroke_DrawingAttributesReplaced;
                }
            }

            this.AdjustSelectionBox(Guid.Empty, removedStrokesIds, null);

            foreach (var stroke in e.Added)
            {
                string id = stroke.GetID();
                stroke.StylusPointsChanged += new EventHandler(stroke_StylusPointsChanged);
                stroke.DrawingAttributesReplaced += stroke_DrawingAttributesReplaced;
                stroke.DrawingAttributesChanged += stroke_DrawingAttributesChanged;
                Stroke copyOne = stroke.Clone();
                copyOne.SetID(id);
                this.currentOperationStrokes.Add(copyOne);
            }
        }

        void stroke_DrawingAttributesChanged(object sender, PropertyDataChangedEventArgs e)
        {
            ChangeDarwingAttribute(sender);
        }

        void stroke_DrawingAttributesReplaced(object sender, DrawingAttributesReplacedEventArgs e)
        {
            ChangeDarwingAttribute(sender);
        }

        private void ChangeDarwingAttribute(object sender)
        {
            Stroke stroke = sender as Stroke;
            string id = stroke.GetID();
            Stroke targetStroke = this.currentOperationStrokes.FirstOrDefault(item => item.GetID() == id);
            if (targetStroke != null)
            {
                targetStroke.DrawingAttributes = stroke.DrawingAttributes;
            }
        }


        void stroke_StylusPointsChanged(object sender, EventArgs e)
        {
            Stroke stroke = sender as Stroke;
            string id = stroke.GetID();
            Stroke targetStroke = this.currentOperationStrokes.FirstOrDefault(item => item.GetID() == id);
            if (targetStroke != null)
            {
                if (stroke.StylusPoints.Count > targetStroke.StylusPoints.Count)
                {
                    for (int i = targetStroke.StylusPoints.Count; i < stroke.StylusPoints.Count; i++)
                    {
                        StylusPoint point = stroke.StylusPoints[i];
                        targetStroke.StylusPoints.Add(point);
                    }
                }
            }
        }

        #endregion

        #region Helper Method
        void SetStrokesSelectedBySync(StrokeCollection sourceCollection, bool isSelected)
        {
            foreach (var item in sourceCollection)
            {
                item.SetIsSelected(isSelected);
            }
        }

        void SetIsOperatingToStrokes(StrokeCollection sourceCollection, bool isOperating)
        {
            foreach (var item in sourceCollection)
            {
                item.SetIsOperating(isOperating);
            }
        }

        /// <summary>
        /// Get the stroke list by id in source list
        /// </summary>
        StrokeCollection GetStrokeCollectionByIds(StrokeCollection source, IEnumerable<string> targetIds)
        {
            StrokeCollection collection = new StrokeCollection();
            IEnumerable<Stroke> strokes = source.Where(item => targetIds.Contains(item.GetID()));
            foreach (var item in strokes)
            {
                collection.Add(item);
            }
            return collection;
        }


        SelectionBox GetSelectionBoxById(Guid behaviorId)
        {
            foreach (var item in this.cvsFeedback.Children)
            {
                SelectionBox box = item as SelectionBox;
                if (box != null)
                {
                    Guid recId = Lasso.GetSelectID(box);
                    if (recId == behaviorId)
                    {
                        return box;
                    }
                }
            }
            return null;
        }

        private void SyncPos(Guid behaviorid, double offsetX, double offsetY, StrokeCollection collection)
        {
            SelectionBox targetBox = this.GetSelectionBoxById(behaviorid);
            if (targetBox != null)
            {
                double currentPosX = targetBox.GetLeft();
                double currentPosY = targetBox.GetTop();

                double newOffsetX = offsetX - currentPosX;
                double newOffsetY = offsetY - currentPosY;

                this.MovingSelectionBoxById(behaviorid, newOffsetX, newOffsetY);

                this.MoveStrokes(collection, newOffsetX, newOffsetY);
            }
        }

        private void RemoveCurrentSelectionStatus()
        {
            if (this.mainSelectionBox != null && this.mainSelectionBox.IsVisible)
            {
                int touchSelectionCount = this.mainSelectionBox.TouchCount;
                if (touchSelectionCount == 0)
                {
                    this.mainSelectionBox.SetVisible(false);
                    this.ClearAllAttachStrokesOnSelectionBox(this.mainSelectionBox);
                    this.SelectionStrokesChanged(null);
                }
            }
        }

        private void RemoveFeedbackSelection()
        {
            if (this.cvsFeedback != null)
            {
                this.cvsFeedback.Children.Clear();
                if (this.mainSelectionBox != null)
                {
                    this.cvsFeedback.Children.Add(this.mainSelectionBox);
                }
            }
        }

        #endregion

        #endregion
    }
}
