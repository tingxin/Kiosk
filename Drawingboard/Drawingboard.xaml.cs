using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IKiosk;
using Drawingboard.controls;
using KComponents;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Ink;
using SpringRoll.Whiteboard.MendStrokes;
using Drawingboard.Helper;
using Drawingboard.controls.MendStrokes;
using StrokesAnimationEngine;
using Drawingboard.Controls;
using System.IO;
using Drawingboard.DataContracts;
namespace Drawingboard
{
    /// <summary>
    /// Interaction logic for Drawingboard.xaml
    /// </summary>
    public partial class Drawingboard : UserControl, IShapeHost
    {
        #region Members
        Lasso lasso;
        OptionSelector selector;
        StrokeInkAnalyzer analyzer = null;
        PenMenuItemChangedArgs lastAction;
        Color lastPenColor = Color.FromArgb(255, 179, 179, 179);
        StrokeCollection strokesWillDoAnimation;
        Point center = new Point(0.5, 0.5);

        internal PenMenuControl menu;
        DrawingboardData data;
        #endregion

        #region Properties
        public string ID { set; get; }
        private bool IsSmoothly
        {
            set
            {
                if (this.analyzer == null)
                {
                    this.analyzer = new StrokeInkAnalyzer(this.inkCanvas, this);
                }
                this.analyzer.IsWork = value;
            }
            get
            {
                if (analyzer == null)
                {
                    return false;
                }
                return this.analyzer.IsWork;
            }
        }
        public bool IsSaved
        {
            get
            {
                return this.data != null;
            }
        }
        public event EventHandler CloseEvent;
        #endregion

        public Drawingboard()
        {
            InitializeComponent();
            this.ID = DateTime.Now.ToFileTimeUtc().ToString();
            this.inkCanvas.DefaultDrawingAttributes.Color = Colors.Transparent;
            this.inkCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
            this.inkCanvas.IsManipulationEnabled = true;
            this.inkCanvas.DefaultDrawingAttributes.FitToCurve = true;
            this.inkCanvas.PreviewTouchDown += inkCanvas_PreviewTouchDown;
            this.inkCanvas.PreviewTouchUp += inkCanvas_PreviewTouchUp;
            this.inkCanvas.PreviewTouchMove += inkCanvas_PreviewTouchMove;

            this.lasso = new Lasso();
            this.lasso.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.lasso.VerticalAlignment = VerticalAlignment.Stretch;

            this.gdRoot.Children.Insert(1, this.lasso);
            this.lasso.Init(this.inkCanvas, this.gdRoot);

            this.InitMenuUI();

            this.Loaded += Drawingboard_Loaded;
            this.SizeChanged += Drawingboard_SizeChanged;
        }

        #region Private
        private void Drawingboard_Loaded(object sender, RoutedEventArgs e)
        {
            AttachGesture();

            if (this.data != null)
            {
                if (this.inkCanvas.Strokes != null)
                {
                    this.inkCanvas.Strokes.StrokesChanged -= Strokes_StrokesChanged;
                }
                this.inkCanvas.Strokes = StrokeBuilder.DataToStrokes(this.data.Strokes, new Size(this.inkCanvas.ActualWidth, this.inkCanvas.ActualHeight));
                this.inkCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
            }
        }

        private void Drawingboard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.lasso.Bound = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
        }

        private void inkCanvas_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            AnimationEngine.Resume();
        }

        private void inkCanvas_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            AnimationEngine.Pause();
        }


        private void inkCanvas_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (this.inkCanvas.EditingMode == SurfaceInkEditingMode.EraseByStroke || this.inkCanvas.EditingMode == SurfaceInkEditingMode.EraseByPoint)
            {
                TouchPoint point = e.TouchDevice.GetTouchPoint(this.inkCanvas);
                Canvas.SetLeft(this.recEraseFeedback, point.Position.X - this.recEraseFeedback.Width / 2);
                Canvas.SetTop(this.recEraseFeedback, point.Position.Y - this.recEraseFeedback.Height / 2);
            }

        }


        private void Strokes_StrokesChanged(object sender, System.Windows.Ink.StrokeCollectionChangedEventArgs e)
        {
            this.InitStrokes(e.Added, false);
        }

        private void AttachGesture()
        {
            this.inkCanvas.AttachGestureDetector(
                (gestureResult) =>
                {
                    if (gestureResult.Result == GestureResult.End)
                    {
                        while (this.inkCanvas.Strokes.Count > 0)
                        {
                            Stroke lastOne = this.inkCanvas.Strokes[this.inkCanvas.Strokes.Count - 1];
                            if (lastOne.DrawingAttributes.Color != Colors.Transparent) break;
                            this.inkCanvas.Strokes.RemoveAt(this.inkCanvas.Strokes.Count - 1);
                        };
                        this.inkCanvas.EditingMode = SurfaceInkEditingMode.Ink;
                        this.recEraseFeedback.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {

                        if (gestureResult.Result == GestureResult.OneFinger)
                        {
                            CloseMenu();
                            while (this.inkCanvas.Strokes.Count > 0)
                            {
                                Stroke lastOne = this.inkCanvas.Strokes[this.inkCanvas.Strokes.Count - 1];
                                if (lastOne.DrawingAttributes.Color != Colors.Transparent) break;
                                lastOne.DrawingAttributes.Color = lastPenColor;
                            };

                        }
                        else
                        {
                            while (this.inkCanvas.Strokes.Count > 0)
                            {
                                Stroke lastOne = this.inkCanvas.Strokes[this.inkCanvas.Strokes.Count - 1];
                                if (lastOne.DrawingAttributes.Color != Colors.Transparent) break;
                                this.inkCanvas.Strokes.RemoveAt(this.inkCanvas.Strokes.Count - 1);
                            };

                            if (gestureResult.Result == GestureResult.Hold)
                            {
                                var pos = this.inkCanvas.TouchesOver.ToList()[0].GetTouchPoint(this.gdRoot).Position;
                                OpenMenu(pos);
                            }
                            else
                            {
                                CloseMenu();

                                if (gestureResult.Result == GestureResult.Brush)
                                {
                                    this.inkCanvas.EditingMode = SurfaceInkEditingMode.EraseByStroke;
                                    Rect rec = (Rect)gestureResult.Tag;
                                    this.recEraseFeedback.Width = rec.Width;
                                    this.recEraseFeedback.Height = rec.Height;

                                    Canvas.SetLeft(this.recEraseFeedback, rec.Left - rec.Width / 2);
                                    Canvas.SetTop(this.recEraseFeedback, rec.Top - rec.Height / 2);
                                    this.recEraseFeedback.Visibility = System.Windows.Visibility.Visible;
                                }
                            }
                        }
                    }

                },
                (ex, exDetal, exSum) =>
                {
                });
        }

        private void InitStrokes(StrokeCollection strokes, bool isCollected)
        {
            foreach (Stroke stroke in strokes)
            {
                InitStroke(stroke, isCollected);
            }
        }

        private void InitStroke(Stroke stroke, bool isCollected)
        {
            if (stroke is ShapeStroke == false)
            {
                stroke.DrawingAttributes.Color = Colors.Transparent;
            }

            double pnThiness = this.inkCanvas.DefaultDrawingAttributes.IsHighlighter ? DrawingBoardParam.HighlighterPenThiness : DrawingBoardParam.MarkerPenThiness;
            stroke.DrawingAttributes.Width = pnThiness;
            stroke.DrawingAttributes.Height = pnThiness;

            stroke.SetID(Guid.NewGuid().ToString());
            stroke.SetPageNumber(0);
            stroke.SetIsCollected(isCollected);
            stroke.SetIsRemote(false);
        }


        private void BeginLasso()
        {
            this.lasso.Begin(
                (strokesSelectedArgs) =>
                {
                    //select stroke,sync this behaviour in here
                },
                (selectedMovingArg) =>
                {
                    //moveing selected stroke,sync this behaviour in here
                },
                (selectedMovedArg) =>
                {
                    //selected stroke moving stop,sync this behaviour in here
                },
                (selectedScalingArg) =>
                {
                    //scaling selected stroke,sync this behaviour in here
                },
                (selectedScaledArg) =>
                {
                    //scaling selected stroke,sync this behaviour in here
                },
                (startDrawingLasso) =>
                {
                    //start drawing lasso feedback
                    AnimationEngine.Pause();
                    this.txtAnimationTip.Visibility = System.Windows.Visibility.Collapsed;
                },
                (drawingLasso) =>
                {
                    //drawing lasso feedback
                },
                (endDrawing) =>
                {
                    //end drawing lasso feedback
                    if (this.lasso.IsAnimation)
                    {
                        this.AddAnimationSelector();
                    }

                },
                (gestureBegin) =>
                {
                },
                (gestureDelta) =>
                {
                },
                 (gestureDone) =>
                 {
                     if (this.lasso.CurrentGesture == GestureResult.End)
                     {
                     }
                 },
                 (pointAtMainWindow) =>
                 {
                 },
                 (menuPos) =>
                 {
                     OpenMenu(menuPos);
                 },
                 () =>
                 {
                 }
                );
        }

        private void SetPenHighlight(bool isHigh)
        {
            this.inkCanvas.DefaultDrawingAttributes.IsHighlighter = isHigh;
        }
        #endregion

        #region Menu

        private void InitMenuUI()
        {
            menu = new PenMenuControl() { Width = 325, Height = 325, RenderTransform = new TranslateTransform(), Visibility = Visibility.Hidden, IsEnabled = false };
            menu.MenuCommandFireEvent += menu_PenMenuCommandFireEvent;
            menuLayer.Children.Add(menu);

            selector = new OptionSelector();
            selector.SetVisible(false);
            selector.SelectedEvent += selector_SelectedEvent;
            menuLayer.Children.Add(selector);
        }

        private void OpenMenu(Point position)
        {
            menu.Visibility = Visibility.Visible;
            menu.IsHitTestVisible = true;
            menu.IsEnabled = true;
            Point pt = AdjustMenuPoint(position);

            menu.MoveMenuCenter(this.TranslatePoint(pt, this.menuLayer.Parent as UIElement));
            menu.Open();
        }

        private Point AdjustMenuPoint(Point position)
        {
            position.X = Math.Min(position.X, ActualWidth - (menu.ActualWidth / 2));
            position.X = Math.Max(position.X, menu.ActualWidth / 2);
            position.Y = Math.Min(position.Y, ActualHeight - (menu.ActualHeight / 2) + ((menu.ActualWidth - menu.ActualHeight) / 2));
            position.Y = Math.Max(position.Y, menu.ActualWidth / 2);

            return position;
        }

        private void menu_PenMenuCommandFireEvent(object sender, PenMenuItemChangedArgs ex)
        {
            ConfigByMenu(ex);

            CloseMenu();
        }


        public void ConfigByMenu(PenMenuItemChangedArgs ex, bool isNeedClose = true)
        {
            if (ex.Command != PenMenuCommand.Close)
            {
                this.lastAction = ex;
            }

            if (this.lastAction != null && this.lastAction.PenDetail != null)
            {
                this.lastPenColor = this.lastAction.PenDetail.PenColor;
            }
            if (ex.CommandType == PenMenuCommandType.Highlight)
            {
                this.SetPenHighlight(true);
                this.inkCanvas.EditingMode = SurfaceInkEditingMode.Ink;
            }
            if (ex.CommandType == PenMenuCommandType.Marker)
            {
                this.SetPenHighlight(false);
                this.inkCanvas.EditingMode = SurfaceInkEditingMode.Ink;
            }
            if (ex.Command == PenMenuCommand.Eraser)
            {
                this.inkCanvas.EditingMode = SurfaceInkEditingMode.EraseByStroke;
            }

            if (ex.Command == PenMenuCommand.SmoothlyPen)
            {
                this.IsSmoothly = true;
                this.inkCanvas.EditingMode = SurfaceInkEditingMode.Ink;
            }
            if (ex.Command == PenMenuCommand.FreeHandPen)
            {
                this.IsSmoothly = false;
                this.inkCanvas.EditingMode = SurfaceInkEditingMode.Ink;
            }

            if (ex.Command == PenMenuCommand.Close)
            {
                if (this.CloseEvent != null)
                {
                    this.CloseEvent(this, ex);
                }
                return;
            }

            this.txtAnimationTip.Visibility = System.Windows.Visibility.Collapsed;
            if (ex.CommandType == PenMenuCommandType.Tools && ex.Command == PenMenuCommand.LassoSelected)
            {
                this.inkCanvas.ReleaseAllTouchCaptures();
                this.lasso.IsAnimation = false;
                this.BeginLasso();
            }
            else if (ex.Command == PenMenuCommand.Animation)
            {
                this.inkCanvas.ReleaseAllTouchCaptures();
                this.lasso.IsAnimation = true;
                this.BeginLasso();
                ShowAnimationTip();
            }
            else
            {
                // this.ctrlWhiteBoard.IsEnabled = true;
                this.lasso.End();
                //(AttachedCollaborateObj as IUIAttachedObject).FocusLayer(true);
                this.lasso.ReleaseAllTouchCaptures();
            }

        }

        private void CloseMenu()
        {
            if (menu == null || !menu.IsEnabled)
            {
                return;
            }

            menu.IsEnabled = false;
            menu.IsHitTestVisible = false;
            menu.Close(() =>
            {
                menu.Visibility = Visibility.Hidden;
            });
        }

        private void ShowAnimationTip()
        {
            Point pos = this.menu.GetMenuCenterPosition();
            var x = pos.X;
            var y = pos.Y;
            if (x + this.txtAnimationTip.Width / 2 > this.ActualWidth)
            {
                x = this.ActualWidth - this.txtAnimationTip.Width;
            }
            else if (x - this.txtAnimationTip.Width / 2 < 0)
            {
                x = 0;
            }
            else
            {
                x = pos.X - this.txtAnimationTip.Width / 2;
            }


            if (y + this.txtAnimationTip.Height > this.ActualHeight)
            {
                y = this.ActualHeight - this.txtAnimationTip.Height;
            }

            Canvas.SetLeft(this.txtAnimationTip, x);
            Canvas.SetTop(this.txtAnimationTip, y);
            this.txtAnimationTip.Visibility = System.Windows.Visibility.Visible;
        }

        #endregion

        #region for animation
        void selector_SelectedEvent(object sender, OptionArg e)
        {
            AnimationEngine.Resume();
            this.lasso.ClearLassco();
            if (this.strokesWillDoAnimation != null)
            {
                this.SwithAnimation(e.Option);
            }
        }

        private void SwithAnimation(AnimationOption option)
        {
            switch (option)
            {
                case AnimationOption.RotateLeft:
                    this.CreateLRotateAnimation(this.strokesWillDoAnimation, center);
                    break;
                case AnimationOption.RotateRight:
                    this.CreateRRotateAnimation(this.strokesWillDoAnimation, center);
                    break;
                case AnimationOption.SqueezeHorizontal:
                    this.CreateHBlinkAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.SqueeezeVertical:
                    this.CreateVBlinkAnimation(this.strokesWillDoAnimation);
                    break;

                case AnimationOption.SwingTop:
                    this.CreateTSwingAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.SwingBottom:
                    this.CreateBSwingAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.SwingLeft:
                    this.CreateLSwingAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.SwingRight:
                    this.CreateRSwingAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.MoveTop:
                    this.CreateGoTopAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.MoveBottom:
                    this.CreateGoBottomAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.MoveLeft:
                    this.CreateGoLeftAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.MoveRight:
                    this.CreateGoRightAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.Jump:
                    this.CreateJumpAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.Flash:
                    this.CreateFlashAnimation(this.strokesWillDoAnimation);
                    break;
                case AnimationOption.Shake:
                    this.CreateShakeAnimation(this.strokesWillDoAnimation, AOrientation.Horizontal);
                    break;
                default:
                    break;
            }
            this.strokesWillDoAnimation = null;
        }

        private void AddAnimationSelector()
        {
            if (this.lasso.SelectionStrokes != null && this.lasso.SelectionStrokes.Count > 0)
            {
                this.strokesWillDoAnimation = this.lasso.SelectionStrokes;
                Rect rec = this.strokesWillDoAnimation.GetBounds();
                Point center = new Point(rec.Left + rec.Width / 2, rec.Top + rec.Height / 2);
                double leftPos = center.X;
                double topPos = center.Y;
                if (center.X + this.selector.Width / 2 > this.ActualWidth)
                {
                    leftPos = this.ActualWidth - this.selector.Width / 2;
                }
                if (rec.Top + this.selector.Height > this.ActualHeight)
                {
                    topPos = this.ActualHeight - this.selector.Height / 2;
                }

                this.selector.SetLeft(leftPos - this.selector.Width / 2);
                this.selector.SetTop(topPos - this.selector.Height / 2);

                this.selector.Show();
            }
        }

        AnimationObj CreateAnimationRomdom3(StrokeCollection strokes)
        {
            double x = 10;
            double y = 255;

            AnimationObj obj = AnimationEngine.AddPropertyAnimation(strokes, 5, 255, 0, (change) =>
            {
                byte r = (byte)change;
                foreach (Stroke stroke in strokes)
                {
                    stroke.DrawingAttributes.Color = Color.FromArgb(r, 255, 0, 0);
                }
            });
            return obj;
        }


        private void CreateShakeAnimation(StrokeCollection strokes, AOrientation oritation)
        {
            Rect bound = strokes.GetBounds();
            Point from = new Point(bound.Left + bound.Width / 2, bound.Top + bound.Height / 2);
            AnimationLibrary.Current.GetShakeAnimationSync(bound, oritation, 5, (to) =>
            {
                AnimationObj obj = AnimationEngine.AddSeriesActionsAnimation(strokes, AnimationActionType.Translate, center, from, to, true);
                obj.Begin();
            });
        }

        #region Rotate Animation
        private void CreateRotateAnimation(StrokeCollection strokes, Point centerOffset, Point from, Point to)
        {
            Rect bound = strokes.GetBounds();
            AnimationObj obj = AnimationEngine.AddDurationActionAnimation(strokes, AnimationActionType.Rotate, centerOffset, 5, from, to, true);
            obj.Begin();
        }

        private void CreateLRotateAnimation(StrokeCollection strokes, Point centerOffset)
        {
            Point from = new Point(0, 0);
            Point to = new Point(-360, 0);
            this.CreateRotateAnimation(strokes, centerOffset, from, to);
        }

        private void CreateRRotateAnimation(StrokeCollection strokes, Point centerOffset)
        {
            Point from = new Point(0, 0);
            Point to = new Point(360, 0);
            this.CreateRotateAnimation(strokes, centerOffset, from, to);
        }
        #endregion

        #region Swing Animation
        private void CreateSwingAnimation(StrokeCollection strokes, Point centerOffset)
        {
            Rect bound = strokes.GetBounds();
            Point from = new Point(0, 0);
            AnimationLibrary.Current.GetSwingAnimationSync(bound, 1, 30, (to) =>
            {
                AnimationObj obj = AnimationEngine.AddSeriesActionsAnimation(strokes, AnimationActionType.Rotate, centerOffset, from, to, true);
                obj.Begin();
            });
        }

        private void CreateTSwingAnimation(StrokeCollection strokes)
        {
            Point centerOffset = new Point(0.5, 0.1);
            this.CreateSwingAnimation(strokes, centerOffset);
        }

        private void CreateBSwingAnimation(StrokeCollection strokes)
        {
            Point centerOffset = new Point(0.5, 0.9);
            this.CreateSwingAnimation(strokes, centerOffset);
        }

        private void CreateLSwingAnimation(StrokeCollection strokes)
        {
            Point centerOffset = new Point(0.9, 0.5);
            this.CreateSwingAnimation(strokes, centerOffset);
        }

        private void CreateRSwingAnimation(StrokeCollection strokes)
        {
            Point centerOffset = new Point(0.1, 0.5);
            this.CreateSwingAnimation(strokes, centerOffset);
        }
        #endregion

        #region Blink
        private void CreateBlinkAnimation(StrokeCollection strokes, AOrientation oritation)
        {
            Rect bound = strokes.GetBounds();
            Point from = new Point(1, 1);
            AnimationLibrary.Current.GetBlinkAnimationSync(bound, oritation, 2, 0.2, (to) =>
            {
                AnimationObj obj = AnimationEngine.AddSeriesActionsAnimation(strokes, AnimationActionType.Scale, center, from, to, true);
                obj.Begin();
            });
        }

        private void CreateHBlinkAnimation(StrokeCollection strokes)
        {
            this.CreateBlinkAnimation(strokes, AOrientation.Horizontal);
        }

        private void CreateVBlinkAnimation(StrokeCollection strokes)
        {
            this.CreateBlinkAnimation(strokes, AOrientation.Vertical);
        }

        #endregion

        #region Jump and Flash
        void CreateJumpAnimation(StrokeCollection strokes)
        {
            Rect bound = strokes.GetBounds();
            Point from = new Point(bound.Left + bound.Width / 2, bound.Top + bound.Height / 2);
            AnimationLibrary.Current.GetJumpAnimationSync(bound, 5, (to) =>
            {
                AnimationObj obj = AnimationEngine.AddSeriesActionsAnimation(strokes, AnimationActionType.Translate, center, from, to);
                obj.Begin();
            });
        }

        void CreateFlashAnimation(StrokeCollection strokes)
        {
            Rect bound = strokes.GetBounds();
            Point from = new Point(bound.Left + bound.Width / 2, bound.Top + bound.Height / 2);
            AnimationLibrary.Current.GetFlashAnimationSync(bound, (to) =>
            {
                AnimationObj obj = AnimationEngine.AddSeriesActionsAnimation(strokes, AnimationActionType.Translate, center, from, to);
                obj.Begin();
            });
        }
        #endregion

        #region Go
        void GoAnimation(StrokeCollection strokes, ATowardto atoward)
        {
            Rect bound = strokes.GetBounds();
            Rect screenBound = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
            Point from = new Point(bound.Left + bound.Width / 2, bound.Top + bound.Height / 2);
            AnimationLibrary.Current.GetGoAnimationSync(bound, screenBound, atoward, false, (position) =>
            {

                AnimationObj ani1 = AnimationEngine.AddDurationActionAnimation(strokes, AnimationActionType.Translate, center, (int)position[0].Duration, from, position[0].To);
                AnimationObj ani2 = AnimationEngine.AddDurationActionAnimation(strokes, AnimationActionType.Translate, center, (int)position[2].Duration, position[1].To, position[2].To);


                ani1.AnimationCompleted += (sender, ex) =>
                {
                    Matrix maritx = new Matrix();
                    double tChange = 0;
                    switch (atoward)
                    {
                        case ATowardto.ToLeft:
                        case ATowardto.ToRight:
                            tChange = position[1].To.X - position[0].To.X;
                            maritx.Translate(tChange, 0);
                            break;
                        case ATowardto.ToTop:
                        case ATowardto.ToBottom:
                            tChange = position[1].To.Y - position[0].To.Y;
                            maritx.Translate(0, tChange);
                            break;
                        default:
                            break;
                    }

                    strokes.Transform(maritx, false);
                    ani2.Begin();
                };

                ani2.AnimationCompleted += (sender, ex) =>
                {
                    Matrix maritx = new Matrix();
                    double tChange = 0;
                    switch (atoward)
                    {
                        case ATowardto.ToLeft:
                        case ATowardto.ToRight:
                            tChange = position[1].To.X - position[0].To.X;
                            maritx.Translate(tChange, 0);
                            break;
                        case ATowardto.ToTop:
                        case ATowardto.ToBottom:
                            tChange = position[1].To.Y - position[0].To.Y;
                            maritx.Translate(0, tChange);
                            break;
                        default:
                            break;
                    }

                    strokes.Transform(maritx, false);
                    ani2.Begin();
                };

                ani1.Begin();




            });

        }


        void CreateGoLeftAnimation(StrokeCollection strokes)
        {
            this.GoAnimation(strokes, ATowardto.ToLeft);
        }

        void CreateGoRightAnimation(StrokeCollection strokes)
        {
            this.GoAnimation(strokes, ATowardto.ToRight);
        }

        void CreateGoTopAnimation(StrokeCollection strokes)
        {
            this.GoAnimation(strokes, ATowardto.ToTop);
        }

        void CreateGoBottomAnimation(StrokeCollection strokes)
        {
            this.GoAnimation(strokes, ATowardto.ToBottom);
        }
        #endregion
        #endregion

        #region public

        public void Add(ShapeStroke shape)
        {
            shape.DrawingAttributes.Color = this.lastPenColor;
            this.inkCanvas.Strokes.Add(shape);
        }


        public Image GetSnapshot()
        {
            Image img = new Image();
            img.BeginInit();
            img.Source = this.GetSnapshotBitmap();
            img.EndInit();

            return img;
        }

        public RenderTargetBitmap GetSnapshotBitmap()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(this.inkCanvas) { Stretch = Stretch.None };
                context.DrawRectangle(brush, null, new Rect(0, 0, this.inkCanvas.ActualWidth, this.inkCanvas.ActualHeight));
                context.Close();
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)this.inkCanvas.ActualWidth, (int)this.inkCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            return bitmap;
        }


        internal DrawingboardData GetModel()
        {
            DrawingboardData data = new DrawingboardData();
            var strokesData = StrokeBuilder.StrokesToData(inkCanvas.Strokes, new Size(inkCanvas.ActualWidth, inkCanvas.ActualHeight));
            data.ID = this.ID;
            data.Strokes = strokesData;

            return data;
        }

        internal void Load(DrawingboardData data)
        {
            this.ID = data.ID;
            this.data = data;
        }
        #endregion
    }
}
