using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using _3DTools;
using IKiosk;
using KComponents;
using KioskArea.Controls;
using KioskArea.Logic;

namespace KioskArea
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        #region Private Members
        const double viewWidth = 2.4;
        const double viewHeight = 1.35;
        const double viewActualWidth = 1200;
        const double viewActualHeight = 675;
        const double opacityCover = 0.2;
        const int animationTime = 200;

        bool isPrensening = false;
        bool isAnimation = false;
        bool needStartApp = false;
        bool isDebug = false;

        int wantToPresentationIndex = -1;
        double normalWidth = 0.0;
        double normalHeight = 0.0;

        Storyboard fullScreenSb = null;
        Storyboard normalSb = null;

        List<UIElement> viewers;
        List<ApplicatoinService> services;
        Brush presentationBrush;
        Brush mainBrush;
        Brush presentationCover;
        //WeatherForecast weatherForecast;
        DropShadowEffect shadow = new DropShadowEffect();

        DispatcherTimer mainTimer;
        #endregion
        public Shell()
        {
            InitializeComponent();

            this.gdAnimationView.Width = viewActualWidth;
            this.gdAnimationView.Height = viewActualHeight;
            this.recBack.Width = viewActualWidth;
            this.recBack.Height = viewActualHeight;
            this.Loaded += this.Shell_Loaded;
            this.txtDate.Text = this.GetDateString();
            this.kioskMenu.RenderLayout = LayoutType.Even;
            this.kioskMenu.RenderOrientation = LayoutOrirentation.Up;
            this.kioskMenu.FireEvent += kioskMenu_FireEvent;

            Color presentationC = (Color)this.Resources["presentationbg"];
            this.presentationBrush = new SolidColorBrush(presentationC);
            this.presentationCover = (Brush)this.Resources["presentationcover"];
            this.mainBrush = (Brush)this.Resources["mainBrush"];

            NameValueCollection section = ConfigurationManager.GetSection("CustomerConfig/BaseInfo") as NameValueCollection;
            bool result = bool.TryParse(section["Debug"], out this.isDebug);
            if (result == false)
            {
                this.isDebug = false;
            }

            this.Unloaded += Shell_Unloaded;
            //this.weatherForecast = new WeatherForecast();
            //this.weatherForecast.WeatherChanged += weatherForecast_WeatherChanged;
            //imgWeather.SetImage(this.weatherForecast.GetWeatherImageAddress(this.weatherForecast.Start()));
            this.shadow.BlurRadius = 20;
            this.shadow.Color = Color.FromArgb(150, 172, 172, 172);
            this.InitModules();

            this.mainTimer = new DispatcherTimer();
            this.mainTimer.Tick += mainTimer_Tick;
            this.mainTimer.Interval = new TimeSpan(0, 0, 5);

        }
        int change = 1;
        void mainTimer_Tick(object sender, EventArgs e)
        {
            if (this.services.Count > 0 && this.isPrensening == false)
            {
                int oldIndex = this.CurrentMidIndex;

                int next = this.CurrentMidIndex + change;
                if (next > this.services.Count - 1 || next < 0)
                {
                    change = change * -1;
                }
                this.CurrentMidIndex = this.CurrentMidIndex + change;

                this.ReSetOpacityForOverflow(this.CurrentMidIndex, oldIndex);
                this.ReSetReflection(oldIndex);

            }
        }

        void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.weatherForecast.Stop();
        }

        void weatherForecast_WeatherChanged(object sender, WeatherForecastEventArgs e)
        {
            //imgWeather.SetImage(this.weatherForecast.GetWeatherImageAddress(e.Weather));
        }


        #region Public Properties
        #region CurrentMidIndexProperty
        public static readonly DependencyProperty CurrentMidIndexProperty = DependencyProperty.Register(
            "CurrentMidIndex", typeof(int), typeof(Shell),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(CurrentMidIndexPropertyChangedCallback)));

        private static void CurrentMidIndexPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Shell win = sender as Shell;
            if (win != null)
            {
                int index = int.Parse(arg.NewValue.ToString());
                int old = int.Parse(arg.OldValue.ToString());

                win.SetIndicator(index, true);
                win.SetIndicator(old, false);

                win.ReLayoutInteractiveVisual3D();
            }
        }

        public int CurrentMidIndex
        {
            get
            {
                return (int)this.GetValue(CurrentMidIndexProperty);
            }
            set
            {
                this.SetValue(CurrentMidIndexProperty, value);
            }
        }
        #endregion

        #region ModelAngleProperty
        public static readonly DependencyProperty ModelAngleProperty = DependencyProperty.Register(
            "ModelAngle", typeof(double), typeof(Shell),
            new FrameworkPropertyMetadata(70.0, new PropertyChangedCallback(ModelAnglePropertyChangedCallback)));


        private static void ModelAnglePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Shell win = sender as Shell;
            if (win != null)
            {
                win.ReLayoutInteractiveVisual3D();
            }
        }

        public double ModelAngle
        {
            get
            {
                return (double)this.GetValue(ModelAngleProperty);
            }
            set
            {
                this.SetValue(ModelAngleProperty, value);
            }
        }
        #endregion

        #region XDistanceBetweenModelsProperty
        public static readonly DependencyProperty XDistanceBetweenModelsProperty = DependencyProperty.Register(
            "XDistanceBetweenModels", typeof(double), typeof(Shell),
            new FrameworkPropertyMetadata(0.5, XDistanceBetweenModelsPropertyChangedCallback));

        private static void XDistanceBetweenModelsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Shell win = sender as Shell;
            if (win != null)
            {
                win.ReLayoutInteractiveVisual3D();
            }
        }

        public double XDistanceBetweenModels
        {
            get
            {
                return (double)this.GetValue(XDistanceBetweenModelsProperty);
            }
            set
            {
                this.SetValue(XDistanceBetweenModelsProperty, value);
            }
        }
        #endregion

        #region ZDistanceBetweenModelsProperty
        public static readonly DependencyProperty ZDistanceBetweenModelsProperty = DependencyProperty.Register(
            "ZDistanceBetweenModels", typeof(double), typeof(Shell),
            new FrameworkPropertyMetadata(0.5, ZDistanceBetweenModelsPropertyChangedCallback));

        private static void ZDistanceBetweenModelsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Shell win = sender as Shell;
            if (win != null)
            {
                win.ReLayoutInteractiveVisual3D();
            }
        }

        public double ZDistanceBetweenModels
        {
            get
            {
                return (double)this.GetValue(ZDistanceBetweenModelsProperty);
            }
            set
            {
                this.SetValue(ZDistanceBetweenModelsProperty, value);
            }
        }
        #endregion

        #region MidModelDistanceProperty
        public static readonly DependencyProperty MidModelDistanceProperty = DependencyProperty.Register(
            "MidModelDistance", typeof(double), typeof(Shell),
            new FrameworkPropertyMetadata(1.5, MidModelDistancePropertyChangedCallback));

        private static void MidModelDistancePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Shell win = sender as Shell;
            if (win != null)
            {
                win.ReLayoutInteractiveVisual3D();
            }
        }

        public double MidModelDistance
        {
            get
            {
                return (double)this.GetValue(MidModelDistanceProperty);
            }
            set
            {
                this.SetValue(MidModelDistanceProperty, value);
            }
        }
        #endregion

        #region OverFlowParentProperty
        public static readonly DependencyProperty OverFlowParentProperty = DependencyProperty.RegisterAttached(
            "OverFlowParent", typeof(Panel), typeof(Shell), new PropertyMetadata(null));

        public static void SetOverFlowParent(DependencyObject obj, Panel parent)
        {
            obj.SetValue(OverFlowParentProperty, parent);
        }

        public static Panel GetOverFlowParent(DependencyObject obj)
        {
            return (Panel)obj.GetValue(OverFlowParentProperty);
        }

        #endregion

        #region OverFlowUIIndexProperty
        public static readonly DependencyProperty OverFlowUIIndexProperty = DependencyProperty.RegisterAttached(
            "OverFlowUIIndex", typeof(int), typeof(Shell), new PropertyMetadata(-1));

        public static void SetOverFlowUIIndex(DependencyObject obj, int index)
        {
            obj.SetValue(OverFlowUIIndexProperty, index);
        }

        public static int GetOverFlowUIIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(OverFlowUIIndexProperty);
        }


        #region Others
        public int StartIndex { get; set; }
        #endregion
        #endregion
        #endregion

        private void InitModules()
        {
            if (this.services == null)
            {
                this.services = new List<ApplicatoinService>();
            }
            List<WebAppPlugin> plugins = new List<WebAppPlugin>();
            LoadModules.Load(this.services, plugins);

            NameValueCollection section = ConfigurationManager.GetSection("CustomerConfig/BrowserInfo") as NameValueCollection;
            foreach (string key in section.AllKeys)
            {
                string content = section[key];
                Kiosk.Browser.Service service = new Kiosk.Browser.Service(key, content);
                this.services.Add(service);

                foreach (WebAppPlugin plugin in plugins)
                {
                    if (plugin.GetHostName() == service.GetApplicationName())
                    {
                        service.AddPlugin(plugin);
                    }
                }
            }

            KioskLog.Instance().Info("Shell", "InitModules");
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            this.viewers = new List<UIElement>();
            foreach (ApplicatoinService service in services)
            {
                try
                {
                    viewers.Add((UIElement)service.GetViewer());
                    service.CloseApp += service_CloseApp;
                    service.OnNotificationEvent += service_OnNotificationEvent;
                    service.RequestPresentationEvent += service_RequestPresentationEvent;
                    KMenuItem item = new KMenuItem(service.ID, service.GetApplicationName());
                    this.kioskMenu.Items.Add(item);
                }
                catch (Exception ex)
                {
                    KioskLog.Instance().Info("Shell", ex.StackTrace);
                }
            }

            for (int i = 0; i < this.viewers.Count; i++)
            {
                Ellipse elp = new Ellipse();
                elp.Width = elp.Height = 10;
                elp.Fill = Brushes.Silver;
                elp.Margin = new Thickness(10, 0, 10, 0);
                this.stkIndicator.Children.Add(elp);
            }
            this.LoadModulesToViewport3D(viewers);

            this.ModelAngle = 64.313725490196077;
            this.XDistanceBetweenModels = 0.82189542483660138;
            this.ZDistanceBetweenModels = 2.0;
            this.MidModelDistance = 2.5;

            this.kioskMenu.ItemColor = new SolidColorBrush(Color.FromArgb(255, 87, 87, 129));
            this.kioskMenu.Render();

            CurrentMidIndex = StartIndex;
            KioskLog.Instance().Info("Shell", "Shell_Loaded");
        }

        private void service_RequestPresentationEvent(object sender, EventArgs e)
        {
            ApplicatoinService service = (ApplicatoinService)sender;
            int index = this.services.IndexOf(service);
            if (index >= 0)
            {
                this.wantToPresentationIndex = index;
                if (this.isPrensening == false)
                {
                    if (this.CurrentMidIndex == index)
                    {
                        this.Presentation(index);
                    }
                    else
                    {
                        this.ReSetOpacityForOverflow(index, this.CurrentMidIndex);
                        this.CurrentMidIndex = index;
                    }

                }
                else
                {
                    if (index != this.CurrentMidIndex)
                    {
                        this.GoToNormal();
                    }
                }
            }
        }

        private void service_OnNotificationEvent(object sender, NotificationArgs e)
        {
            Console.WriteLine("Get message form " + e.From);
            foreach (ApplicatoinService service in services)
            {
                if (sender != service)
                {
                    service.OnNotification(e.From, e.Key, e.Content);
                }
            }
        }

        private void service_CloseApp(object sender, EventArgs e)
        {
            this.GoToNormal();
        }

        private void kioskMenu_FireEvent(object sender, KMenuArgs e)
        {
            for (int index = 0; index < this.services.Count; index++)
            {
                ApplicatoinService service = this.services[index];
                if (service.ID == e.Key)
                {
                    this.Presentation(index, true);
                    break;
                }
            }
        }


        #region Show　overflow

        private void LoadModulesToViewport3D(List<UIElement> modules)
        {
            if (modules == null)
            {
                return;
            }

            for (int i = 0; i < modules.Count; i++)
            {
                InteractiveVisual3D iv3d = this.CreateInteractiveVisual3D(modules[i], i);

                this.viewport3D.Children.Add(iv3d);
            }

            this.ReLayoutInteractiveVisual3D();
        }

        private Visual CreateVisual(UIElement visual, int index)
        {
            Border bd = new Border();
            bd.Background = this.presentationBrush;
            bd.CornerRadius = new CornerRadius(20);

            Grid cover = new Grid();
            cover.Background = Brushes.Transparent;
            Rectangle rec = new Rectangle();
            rec.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            rec.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            rec.Fill = Brushes.Transparent;
            cover.Children.Add(visual);
            cover.Children.Add(rec);
            bd.Width = this.FullScreenPanel.ActualWidth;
            bd.Height = this.FullScreenPanel.ActualHeight;
            cover.Width = this.FullScreenPanel.ActualWidth;
            cover.Height = this.FullScreenPanel.ActualHeight;
            cover.Margin = new Thickness(20);
            Shell.SetOverFlowParent(visual, cover);
            Shell.SetOverFlowUIIndex(visual, index);
            Shell.SetOverFlowUIIndex(cover, index);

            if (index == this.CurrentMidIndex)
            {
                VisualBrush visualMark = new VisualBrush(bd);
                this.pathMark.Fill = visualMark;
            }
            else
            {
                cover.Opacity = opacityCover;
            }

            cover.AttachTouchOperation(this.HandleGesture);
            if (this.isDebug)
            {
                cover.MouseLeftButtonUp += cover_MouseLeftButtonUp;
            }

            bd.Child = cover;
            return bd;
        }

        void cover_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isAnimation) return;
            FrameworkElement ui = sender as FrameworkElement;
            Border bd = ui.Parent as Border;
            int oldIndex = this.CurrentMidIndex;
            int index = Shell.GetOverFlowUIIndex(ui);
            if (index >= 0)
            {
                if (this.CurrentMidIndex == index)
                {
                    this.Presentation(index);
                }
                else
                {
                    this.CurrentMidIndex = index;
                    VisualBrush visualMark = new VisualBrush(bd);
                    this.pathMark.Fill = visualMark;
                    this.txtCurrentName.Text = this.services[index].GetApplicationName();
                }
            }
        }

        void HandleGesture(GestureArg detector)
        {
            if (this.isAnimation) return;
            FrameworkElement ui = detector.Sender as FrameworkElement;
            Border bd = ui.Parent as Border;
            int oldIndex = this.CurrentMidIndex;
            int index = Shell.GetOverFlowUIIndex(ui);
            if (index >= 0)
            {
                if (detector.Result == GestureResult.Tap)
                {
                    if (this.CurrentMidIndex == index)
                    {
                        this.Presentation(index);
                    }
                    else
                    {
                        this.CurrentMidIndex = index;
                        VisualBrush visualMark = new VisualBrush(bd);
                        this.pathMark.Fill = visualMark;
                        this.txtCurrentName.Text = this.services[index].GetApplicationName();
                    }
                }
                else if (detector.Result == GestureResult.OneFinger || detector.Result == GestureResult.Drag)
                {
                    Vector v = (Vector)detector.Tag;
                    if (v.X > 0)
                    {
                        if (this.CurrentMidIndex > 0)
                        {
                            this.CurrentMidIndex--;
                        }

                    }
                    else
                    {
                        if (this.CurrentMidIndex < this.services.Count - 1)
                        {
                            this.CurrentMidIndex++;
                        }
                    }

                    ReSetReflection(oldIndex);
                }

                this.ReSetOpacityForOverflow(this.CurrentMidIndex, oldIndex);
            }
        }

        private Geometry3D CreateGeometry3D()
        {
            MeshGeometry3D geometry = new MeshGeometry3D();

            geometry.Positions = new Point3DCollection();
            geometry.Positions.Add(new Point3D(-viewWidth, viewHeight, 0));
            geometry.Positions.Add(new Point3D(-viewWidth, -viewHeight, 0));
            geometry.Positions.Add(new Point3D(viewWidth, -viewHeight, 0));
            geometry.Positions.Add(new Point3D(viewWidth, viewHeight, 0));

            geometry.TriangleIndices = new Int32Collection();
            geometry.TriangleIndices.Add(0);
            geometry.TriangleIndices.Add(1);
            geometry.TriangleIndices.Add(2);
            geometry.TriangleIndices.Add(0);
            geometry.TriangleIndices.Add(2);
            geometry.TriangleIndices.Add(3);

            geometry.TextureCoordinates = new PointCollection();
            geometry.TextureCoordinates.Add(new Point(0, 0));
            geometry.TextureCoordinates.Add(new Point(0, 1));
            geometry.TextureCoordinates.Add(new Point(1, 1));
            geometry.TextureCoordinates.Add(new Point(1, 0));

            return geometry;
        }

        private InteractiveVisual3D CreateInteractiveVisual3D(UIElement visual, int index)
        {
            InteractiveVisual3D iv3d = new InteractiveVisual3D();
            iv3d.Visual = this.CreateVisual(visual, index);
            iv3d.Geometry = this.CreateGeometry3D();
            iv3d.Transform = this.CreateEmptyTransform3DGroup();

            return iv3d;
        }

        private Transform3DGroup CreateEmptyTransform3DGroup()
        {
            Transform3DGroup group = new Transform3DGroup();
            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)));
            group.Children.Add(new TranslateTransform3D(new Vector3D()));
            group.Children.Add(new ScaleTransform3D());

            return group;
        }

        private void GetTransformOfInteractiveVisual3D(int index, int midIndex, out double angle, out double offsetX, out double offsetZ)
        {
            int disToMidIndex = index - midIndex;
            angle = 0;
            if (disToMidIndex < 0)
            {
                angle = this.ModelAngle;
            }
            else if (disToMidIndex > 0)
            {
                angle = (-this.ModelAngle);
            }


            offsetX = 0;
            if (Math.Abs(disToMidIndex) <= 1)
            {
                offsetX = disToMidIndex * this.MidModelDistance;
            }
            else if (disToMidIndex != 0)
            {
                offsetX = disToMidIndex * this.XDistanceBetweenModels + (disToMidIndex > 0 ? this.MidModelDistance : -this.MidModelDistance);
            }
            offsetZ = Math.Abs(disToMidIndex) * -this.ZDistanceBetweenModels;

        }

        private void ReLayoutInteractiveVisual3D()
        {
            int j = 0;
            for (int i = 0; i < this.viewport3D.Children.Count; i++)
            {
                InteractiveVisual3D iv3d = this.viewport3D.Children[i] as InteractiveVisual3D;
                if (iv3d != null)
                {
                    double angle = 0;
                    double offsetX = 0;
                    double offsetZ = 0;
                    this.GetTransformOfInteractiveVisual3D(j++, this.CurrentMidIndex, out angle, out offsetX, out offsetZ);


                    NameScope.SetNameScope(this, new NameScope());
                    this.RegisterName("iv3d", iv3d);
                    Duration time = new Duration(TimeSpan.FromSeconds(0.3));

                    DoubleAnimation angleAnimation = new DoubleAnimation(angle, time);
                    DoubleAnimation xAnimation = new DoubleAnimation(offsetX, time);
                    DoubleAnimation zAnimation = new DoubleAnimation(offsetZ, time);

                    Storyboard story = new Storyboard();
                    story.Children.Add(angleAnimation);
                    story.Children.Add(xAnimation);
                    story.Children.Add(zAnimation);

                    Storyboard.SetTargetName(angleAnimation, "iv3d");
                    Storyboard.SetTargetName(xAnimation, "iv3d");
                    Storyboard.SetTargetName(zAnimation, "iv3d");

                    Storyboard.SetTargetProperty(
                        angleAnimation,
                        new PropertyPath("(ModelVisual3D.Transform).(Transform3DGroup.Children)[0].(RotateTransform3D.Rotation).(AxisAngleRotation3D.Angle)"));

                    Storyboard.SetTargetProperty(
                        xAnimation,
                        new PropertyPath("(ModelVisual3D.Transform).(Transform3DGroup.Children)[1].(TranslateTransform3D.OffsetX)"));
                    Storyboard.SetTargetProperty(
                        zAnimation,
                        new PropertyPath("(ModelVisual3D.Transform).(Transform3DGroup.Children)[1].(TranslateTransform3D.OffsetZ)"));

                    story.Completed += ChangeApp_Completed;
                    story.Begin(this);

                }
            }
        }

        void ChangeApp_Completed(object sender, EventArgs e)
        {
            if (this.wantToPresentationIndex >= 0)
            {
                this.wantToPresentationIndex = -1;
                this.Presentation(this.CurrentMidIndex);
            }
        }

        private void SetIndicator(int index, bool isHigh)
        {
            Ellipse ep = (Ellipse)this.stkIndicator.Children[index];
            ep.Fill = isHigh ? Brushes.White : Brushes.Silver;
        }
        #endregion

        #region Presentation
        private void Presentation(int index, bool needStarted = false)
        {
            if (this.isPrensening == false && isAnimation == false)
            {
                KioskLog.Instance().Info("Shell", "Presentation");
                ReSetOpacityForOverflow(index, this.CurrentMidIndex);
                this.CurrentMidIndex = index;
                UIElement visual = (UIElement)this.services[index].GetViewer();
                if (visual != null)
                {
                    this.needStartApp = needStarted;
                    Panel panel = Shell.GetOverFlowParent(visual);
                    if (panel.Children.Contains(visual))
                    {
                        panel.Children.Remove(visual);
                    }
                    this.gdAnimationView.Children.Add(visual);

                    this.GoToFullScreen(needStarted);
                }
            }

        }

        private void OnPresentation(bool needStarted)
        {
            if (this.CurrentMidIndex >= 0 && this.CurrentMidIndex <= this.services.Count)
            {
                this.services[this.CurrentMidIndex].OnPresentation(needStarted);
            }
        }

        private void OnOverview()
        {
            if (this.CurrentMidIndex >= 0 && this.CurrentMidIndex <= this.services.Count)
            {
                this.services[this.CurrentMidIndex].OnBackground();
            }

            if (this.wantToPresentationIndex >= 0)
            {
                this.CurrentMidIndex = this.wantToPresentationIndex;
            }
        }


        private void ReSetOpacityForOverflow(int index, int oldIndex)
        {
            if (index != oldIndex)
            {
                UIElement showOldUI = (UIElement)this.services[oldIndex].GetViewer();
                Panel uiOldPanel = Shell.GetOverFlowParent(showOldUI);
                uiOldPanel.Opacity = opacityCover;

                UIElement showNewUI = (UIElement)this.services[index].GetViewer();
                Panel uiNewPanel = Shell.GetOverFlowParent(showNewUI);
                uiNewPanel.Opacity = 1.0;
            }
        }

        private void ReSetReflection(int oldIndex)
        {
            if (oldIndex != this.CurrentMidIndex)
            {

                UIElement showUI = (UIElement)this.services[this.CurrentMidIndex].GetViewer();
                Panel uiPanel = Shell.GetOverFlowParent(showUI);
                uiPanel.Opacity = 1.0;
                Border bUI = uiPanel.Parent as Border;
                VisualBrush visualMark = new VisualBrush(bUI);
                this.pathMark.Fill = visualMark;
                this.txtCurrentName.Text = this.services[this.CurrentMidIndex].GetApplicationName();
            }
        }

        #region Go fullscreen
        private Storyboard GetFullScreenStory()
        {
            double xRage = this.FullScreenPanel.ActualWidth / this.gdAnimationView.Width;
            double yRage = this.FullScreenPanel.ActualHeight / this.gdAnimationView.Height;

            this.normalWidth = this.gdAnimationView.ActualWidth;
            this.normalHeight = this.gdAnimationView.ActualHeight;

            DoubleAnimation anix = StoryboardFactory.CreateDoubleAnimation(this.gdAnimationView, animationTime, 1, xRage, StoryboardFactory.ScareX);
            DoubleAnimation aniy = StoryboardFactory.CreateDoubleAnimation(this.gdAnimationView, animationTime, 1, yRage, StoryboardFactory.ScareY);

            DoubleAnimation opacity = StoryboardFactory.CreateDoubleAnimation(this.recBack, animationTime, 1, 0.0, StoryboardFactory.Opacity);

            Storyboard sb = new Storyboard();
            sb.Children.Add(anix);
            sb.Children.Add(aniy);
            sb.Children.Add(opacity);
            return sb;
        }

        private void GoToFullScreen(bool needStarted)
        {
            if (this.isPrensening == false)
            {
                this.services[this.CurrentMidIndex].BeginPresentation(needStarted);
                this.isPrensening = true;
                this.isAnimation = true;
                if (this.fullScreenSb == null)
                {
                    this.fullScreenSb = this.GetFullScreenStory();
                    this.fullScreenSb.Completed += this.fullScreenSb_Completed;
                }

                this.gdAnimationView.CacheMode = new BitmapCache(0.25);
                this.recBack.Visibility = System.Windows.Visibility.Visible;
                if (this.normalSb != null)
                {
                    this.normalSb.Stop();
                }
                this.fullScreenSb.Begin();
            }
        }

        void fullScreenSb_Completed(object sender, EventArgs e)
        {
            this.GoToFullScreenStatus();

            this.OnPresentation(needStartApp);

            this.isAnimation = false;
        }

        private void GoToFullScreenStatus()
        {
            if (this.gdAnimationView.Children.Count > 0)
            {
                UIElement ui = this.gdAnimationView.Children[this.gdAnimationView.Children.Count - 1];
                this.gdAnimationView.Children.Remove(ui);
                this.FullScreenPanel.Children.Add(ui);
            }
            this.recBack.Visibility = System.Windows.Visibility.Collapsed;
            this.FullScreenPanel.IsHitTestVisible = true;
            this.FullScreenPanel.Background = this.mainBrush;
            this.gdAnimationView.CacheMode = null;
        }

        private void AddPresentationMenu()
        {
            KMenu menu = new KMenu();
            menu.RenderLayout = LayoutType.Even;
            menu.RenderOrientation = LayoutOrirentation.Up;
            menu.Width = menu.Height = 300;

            ApplicatoinService service = this.services[this.CurrentMidIndex];
        }
        #endregion

        #region GoToNormal
        private void GoToNormal()
        {
            if (this.isPrensening && this.isAnimation == false)
            {
                KioskLog.Instance().Info("Shell", "GoToNormal");
                this.services[this.CurrentMidIndex].BeginBackground();
                this.isPrensening = false;
                this.isAnimation = true;
                if (this.FullScreenPanel.Children.Count > 0)
                {
                    UIElement ui = this.FullScreenPanel.Children[0];
                    this.FullScreenPanel.Children.Remove(ui);
                    this.gdAnimationView.Children.Add(ui);
                    this.FullScreenPanel.IsHitTestVisible = false;
                    this.FullScreenPanel.ClearValue(Panel.BackgroundProperty);
                }

                if (this.fullScreenSb != null)
                {
                    this.fullScreenSb.Stop();
                }

                if (this.normalSb == null)
                {
                    this.normalSb = this.GetNormalScreenStory();
                    this.normalSb.Completed += this.normalSb_Completed;
                }
                this.gdAnimationView.CacheMode = new BitmapCache(0.2);
                this.recBack.Visibility = System.Windows.Visibility.Visible;
                this.normalSb.Begin();
            }
        }

        private void normalSb_Completed(object sender, EventArgs e)
        {
            GoToNormalStatus();

            this.OnOverview();

            this.isAnimation = false;
        }

        private void GoToNormalStatus()
        {
            UIElement ui = this.gdAnimationView.Children[this.gdAnimationView.Children.Count - 1];
            Panel panel = Shell.GetOverFlowParent(ui);
            this.gdAnimationView.Children.Remove(ui);
            this.recBack.Visibility = System.Windows.Visibility.Collapsed;
            panel.Children.Insert(0, ui);
            this.gdAnimationView.CacheMode = null;
        }

        private Storyboard GetNormalScreenStory()
        {
            DoubleAnimation anix = StoryboardFactory.CreateDoubleAnimation(this.gdAnimationView, animationTime, null, 1.0, StoryboardFactory.ScareX);
            DoubleAnimation aniy = StoryboardFactory.CreateDoubleAnimation(this.gdAnimationView, animationTime, null, 1.0, StoryboardFactory.ScareY);
            DoubleAnimation opacity = StoryboardFactory.CreateDoubleAnimation(this.recBack, animationTime, 0.0, 1.0, StoryboardFactory.Opacity);
            Storyboard sb = new Storyboard();

            sb.Children.Add(anix);
            sb.Children.Add(aniy);
            sb.Children.Add(opacity);
            return sb;
        }
        #endregion
        #endregion

        #region GetDateString
        private string GetDateString()
        {
            const string dateFormat = "{0}. {1}th, {2}";
            string month = string.Empty;
            switch (DateTime.Now.Month)
            {

                case 1:
                    month = "Jan";
                    break;
                case 2:
                    month = "Feb";
                    break;
                case 3:
                    month = "Mar";
                    break;
                case 4:
                    month = "Apr";
                    break;
                case 5:
                    month = "May";
                    break;
                case 6:
                    month = "Jun";
                    break;
                case 7:
                    month = "Jul";
                    break;
                case 8:
                    month = "Aug";
                    break;
                case 9:
                    month = "Sep";
                    break;
                case 10:
                    month = "Oct";
                    break;
                case 11:
                    month = "Nov";
                    break;
                default:
                    month = "Dec";
                    break;

            }
            return string.Format(dateFormat, month, DateTime.Now.Day, DateTime.Now.Year);
        }
        #endregion
    }
}
