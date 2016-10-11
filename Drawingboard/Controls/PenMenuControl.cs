using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
    ///     xmlns:MyNamespace="clr-namespace:Drawingboard.controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Drawingboard.controls;assembly=Drawingboard.controls"
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
    ///     <MyNamespace:PenMenuControl/>
    ///
    /// </summary>
    [TemplatePart(Name = "gdRoot", Type = typeof(Grid))]
    [TemplatePart(Name = "gdMain", Type = typeof(Grid))]
    [TemplatePart(Name = "bdRoot", Type = typeof(Border))]
    [TemplatePart(Name = "pathOverView", Type = typeof(Path))]
    [TemplatePart(Name = "pathClose", Type = typeof(Path))]
    [TemplatePart(Name = "cvsClose", Type = typeof(Canvas))]
    public class PenMenuControl : Control
    {
        #region Private
        object synObj = new object();

        private DateTime timeCalculate = DateTime.Now;
        private Brush highlightBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
        private Brush highlightPenBrush = Brushes.White;
        private Brush normalBrush = new SolidColorBrush(Color.FromArgb(255, 74, 80, 119));
        private Brush animatoinOrCose = new SolidColorBrush(Color.FromArgb(255, 22, 0, 92));
        private Path focusPenPath = null;
        private Path focusToolPath = null;
        private Path lastFocusToolPenPath = null;
        private Path lastTouchObj = null;
        private Storyboard showAnimation;
        private Storyboard closeAnimation;
        private Panel currentPanel = null;
        private Action closeCompleted;
        private List<Path> toolCache = new List<Path>();

        #region Parts
        private Grid gdRoot;
        private Grid gdMain;
        private Border bdRoot;
        private Path pathClose;
        private Canvas cvsClose;
        #endregion
        #endregion

        static PenMenuControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PenMenuControl), new FrameworkPropertyMetadata(typeof(PenMenuControl)));
        }

        public PenMenuControl()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                this.Width = 500;
                this.Height = 500;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.gdRoot = this.GetTemplateChild("gdRoot") as Grid;
            this.gdMain = this.GetTemplateChild("gdMain") as Grid;
            this.bdRoot = this.GetTemplateChild("bdRoot") as Border;
            this.pathClose = this.GetTemplateChild("pathClose") as Path;
            this.cvsClose = this.GetTemplateChild("cvsClose") as Canvas;
            this.IsManipulationEnabled = true;

            #region Init
            this.showAnimation = this.bdRoot.Resources["ShowAnimation"] as Storyboard;
            this.closeAnimation = this.bdRoot.Resources["CloseAnimation"] as Storyboard;
            this.closeAnimation.Completed += new EventHandler(closeAnimation_Completed);

            InitPathCommand();
            #endregion
        }

        #region Public Properties
        /// <summary>
        /// Menu Command changed
        /// </summary>
        public event PenMenuCommandFire MenuCommandFireEvent;

        #region PenMenuCommandType

        public static void SetCommandType(DependencyObject obj, PenMenuCommandType value)
        {
            obj.SetValue(CommandTypeProperty, value);
        }

        public static PenMenuCommandType GetCommandType(DependencyObject obj)
        {
            return (PenMenuCommandType)obj.GetValue(CommandTypeProperty);
        }

        public static readonly DependencyProperty CommandTypeProperty = DependencyProperty.RegisterAttached("CommandType", typeof(PenMenuCommandType), typeof(PenMenuControl), new PropertyMetadata(PenMenuCommandType.Tools));

        #endregion

        #region MenuCommand

        public static void SetMenuCommand(DependencyObject obj, PenMenuCommand value)
        {
            obj.SetValue(MenuCommandProperty, value);
        }

        public static PenMenuCommand GetMenuCommand(DependencyObject obj)
        {
            return (PenMenuCommand)obj.GetValue(MenuCommandProperty);
        }


        public static readonly DependencyProperty MenuCommandProperty = DependencyProperty.RegisterAttached("MenuCommand", typeof(PenMenuCommand), typeof(PenMenuControl), new PropertyMetadata(PenMenuCommand.FreeHandPen));

        #endregion
        #endregion

        #region Public Method
        /// <summary>
        /// Open the PenMenu
        /// </summary>
        public void Open()
        {
            if (this.showAnimation != null)
            {
                if (this.Parent == null && this.currentPanel != null)
                {
                    this.currentPanel.Children.Add(this);
                }
                this.showAnimation.Begin();

            }
        }

        /// <summary>
        /// Close the PenMenu
        /// </summary>
        public void Close(Action closeCompleted)
        {
            if (this.closeAnimation != null)
            {
                if (this.showAnimation != null)
                {
                    this.showAnimation.Stop();
                }
                this.closeCompleted = closeCompleted;
                this.closeAnimation.Begin();
            }
        }
        #endregion

        #region Private Method
        void InitPathCommand()
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                this.pathClose.TouchDown += this.CloseCommand_Fire;
                #region init touch area
                foreach (var item in this.gdRoot.Children)
                {
                    Path targetItem = item as Path;
                    if (targetItem != null)
                    {
                        if (targetItem.Name.Length > 0)
                        {
                            string[] namestr = targetItem.Name.Split('_');
                            if (namestr.Length == 3)
                            {
                                string commandTypeStr = namestr[1];
                                int commandIndex = int.Parse(namestr[2]);
                                PenMenuCommandType type;
                                //1-4:Heightlight,5-9:Marker,10-13:tool
                                if (commandIndex >= 1 && commandIndex <= 4)
                                {
                                    type = PenMenuCommandType.Highlight;
                                }
                                else if (commandIndex >= 5 && commandIndex <= 9)
                                {
                                    type = PenMenuCommandType.Marker;
                                }
                                else
                                {
                                    type = PenMenuCommandType.Tools;
                                }

                                PenMenuCommand command = (PenMenuCommand)commandIndex;
                                SetCommandType(targetItem, type);
                                SetMenuCommand(targetItem, command);
                                targetItem.TouchDown += targetItem_TouchDown;

                                if (command == PenMenuCommand.FreeHandPen)
                                {
                                    this.focusToolPath = targetItem;
                                    this.lastFocusToolPenPath = targetItem;
                                    this.toolCache.Add(this.focusToolPath);
                                }
                                else if (command == PenMenuCommand.MarkerWhite)
                                {
                                    this.focusPenPath = targetItem;
                                }
                            }
                        }
                    }
                }
                #endregion
            }

        }


        void RaisePenMenuCommandFireEvent(PenMenuCommandType type, PenMenuCommand command)
        {
            if (this.MenuCommandFireEvent != null)
            {
                this.MenuCommandFireEvent(this, new PenMenuItemChangedArgs(type, command));
            }
        }


        void targetItem_TouchDown(object sender, TouchEventArgs e)
        {
            lock (synObj)
            {
                lastTouchObj = null;
                this.timeCalculate = DateTime.Now;
                Path targetItem = sender as Path;

                PenMenuCommand command = PenMenuControl.GetMenuCommand(targetItem);
                PenMenuCommandType type = PenMenuControl.GetCommandType(targetItem);

                if (type == PenMenuCommandType.Tools)
                {
                    this.toolCache.Add(targetItem);
                    if (this.focusToolPath != null)
                    {
                        this.toolCache.Remove(focusToolPath);
                        //if last focus tool belong to pen,use lastFocusPenToolPath to record this one
                        PenMenuCommand lastCommand = PenMenuControl.GetMenuCommand(this.focusToolPath);
                        if (lastCommand == PenMenuCommand.FreeHandPen || lastCommand == PenMenuCommand.SmoothlyPen)
                        {
                            this.lastFocusToolPenPath = this.focusToolPath;
                        }
                    }
                    //not drawing operation
                    if (command != PenMenuCommand.FreeHandPen && command != PenMenuCommand.SmoothlyPen)
                    {
                        //drwaing pen turn to normal state
                        if (this.focusPenPath != null)
                        {
                            this.focusPenPath.Fill = this.normalBrush;
                            this.SetRelativePathStatus(this.focusPenPath, false);
                        }
                    }
                    else
                    {
                        //if select pen,get the current draing pen type(highlight or marker pen )
                        if (this.focusPenPath != null)
                        {
                            this.focusPenPath.Fill = this.highlightBrush;
                            this.SetRelativePathStatus(this.focusPenPath, true);
                        }
                    }
                    this.focusToolPath = targetItem;
                }
                else
                {
                    focusPenPath.Fill = this.normalBrush;
                    this.SetRelativePathStatus(this.focusPenPath, false);

                    //if click the drawing pen,use current button command
                    targetItem.Fill = this.highlightBrush;
                    this.SetRelativePathStatus(targetItem, true);

                    PenMenuCommand focusCommand = PenMenuControl.GetMenuCommand(this.focusToolPath);
                    if (focusCommand == PenMenuCommand.Eraser || focusCommand == PenMenuCommand.LassoSelected)
                    {
                        if (this.lastFocusToolPenPath != null)
                        {
                            if (this.toolCache.Contains(lastFocusToolPenPath) == false)
                            {
                                this.toolCache.Clear();
                                this.toolCache.Add(lastFocusToolPenPath);
                                this.focusToolPath = lastFocusToolPenPath;

                            }

                        }
                    }
                    focusPenPath = targetItem;
                }
                this.RaisePenMenuCommandFireEvent(type, command);

                if (toolCache.Count != 1)
                {
                    throw new Exception("Hight light should be one");
                }

                foreach (var item in this.gdRoot.Children)
                {
                    Path path = item as Path;
                    if (path != null)
                    {
                        PenMenuCommandType currentElementType = PenMenuControl.GetCommandType(path);
                        PenMenuCommand menuCommand = PenMenuControl.GetMenuCommand(path);
                        if (currentElementType == PenMenuCommandType.Tools)
                        {
                            if (path.Name.StartsWith("path_"))
                            {
                                if (path == toolCache[0])
                                {
                                    path.Fill = this.highlightBrush;
                                    this.SetRelativePathStatus(path, true);
                                    Trace.WriteLine(string.Format("{0} is hightlight", path.Name));
                                }
                                else
                                {
                                    path.Fill = menuCommand == PenMenuCommand.Animation ? this.animatoinOrCose : this.normalBrush;
                                    this.SetRelativePathStatus(path, false);
                                    Trace.WriteLine(string.Format("{0} is normal", path.Name));
                                }
                            }
                        }
                    }
                }
            }
        }

        void CloseCommand_Fire(object sender, TouchEventArgs e)
        {
            lastTouchObj = this.pathClose;
            this.SetBrushTransition(this.pathClose, this.cvsClose, true);
            PenMenuCommandType type = PenMenuControl.GetCommandType(this.focusToolPath);

            this.RaisePenMenuCommandFireEvent(type, PenMenuCommand.Close);
        }


        void closeAnimation_Completed(object sender, EventArgs e)
        {
            if (lastTouchObj != null)
            {
                if (lastTouchObj == this.pathClose)
                {
                    this.SetBrushTransition(this.pathClose, this.cvsClose, false);
                }
            }
            if (this.closeCompleted != null)
            {
                this.closeCompleted();
            }
        }

        private void SetTargetPathGroupStatus(PenMenuCommand command, bool isHilght)
        {
            int index = (int)command;

            foreach (FrameworkElement item in this.gdMain.Children)
            {
                if (item.Name.Equals("path_" + index.ToString()))
                {
                    Panel panel = item as Panel;
                    foreach (UIElement ui in panel.Children)
                    {
                        Path path = ui as Path;
                        BrushColor(isHilght, index, path);

                        Panel wordsPanel = ui as Panel;
                        if (wordsPanel != null)
                        {
                            foreach (Path word in wordsPanel.Children)
                            {
                                if (isHilght == false)
                                {
                                    word.Fill = this.highlightPenBrush;
                                }
                                else
                                {
                                    word.Fill = this.normalBrush;
                                }
                            }
                        }
                    }
                    continue;
                }
                if (item.Name.Equals("path_" + index.ToString() + "_back"))
                {
                    item.SetVisible(!isHilght);
                }
            }

        }

        private void BrushColor(bool isHilght, int index, Path path)
        {
            if (path != null)
            {
                if (index >= (int)PenMenuCommand.LassoSelected)
                {
                    if (isHilght == false)
                    {

                        path.Stroke = this.highlightPenBrush;
                    }
                    else
                    {
                        path.Stroke = this.normalBrush;
                    }
                }
                else
                {
                    if (isHilght == false)
                    {
                        path.Fill = this.highlightPenBrush;
                    }
                    else
                    {
                        path.Fill = this.normalBrush;
                    }
                }

            }
        }

        private void SetRelativePathStatus(Path path, bool isHilght)
        {
            PenMenuCommand focusPenPathcommand = PenMenuControl.GetMenuCommand(path);

            this.SetTargetPathGroupStatus(focusPenPathcommand, isHilght);
        }

        private void SetBrushTransition(Path mainPath, Panel relativePathContainer, bool isHigh)
        {
            mainPath.Fill = isHigh ? this.highlightPenBrush : this.animatoinOrCose;
            foreach (Path path in relativePathContainer.Children)
            {
                path.Fill = isHigh ? this.animatoinOrCose : this.highlightPenBrush;
            }
        }
        #endregion
    }
}
