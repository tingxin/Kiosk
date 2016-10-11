using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IKiosk;
using StrokesAnimationEngine;

namespace Drawingboard
{
    public class Service : ApplicatoinService
    {
        Wall wall;

        public Service()
            : base()
        {
            AnimationEngine.Run();

        }

        public override object GetViewer()
        {
            if (this.wall == null)
            {
                wall = new Wall();
                wall.kMenu.FireEvent += kMenu_FireEvent;
                wall.CloseEvent += wall_CloseEvent;
                wall.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                wall.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            }
            AnimationEngine.Pause();
            return wall;
        }

        void wall_CloseEvent(object sender, EventArgs e)
        {
            this.RaiseCloseAppEvent(this, null);
        }

        void kMenu_FireEvent(object sender, KComponents.KMenuArgs e)
        {
            if (e.Key == Config.KEYCLOSE)
            {
                this.RaiseCloseAppEvent(this, null);
            }

        }

        public override string GetApplicationName()
        {
            return "Doodling";
        }

        public override void OnBackground()
        {
            base.OnBackground();
            AnimationEngine.Pause();
        }

        public override void OnPresentation(bool needStarted)
        {
            base.OnPresentation(needStarted);
            AnimationEngine.Resume();
            if (this.wall != null)
            {
                wall.kMenu.Visibility = System.Windows.Visibility.Visible;
            }
            if (needStarted)
            {
                this.wall.OpenNewDrawing();
            }
        }

        public override void BeginPresentation(bool needStarted)
        {
            base.BeginPresentation(needStarted);
            if (this.wall != null)
            {

            }
        }

        public override void BeginBackground()
        {
            base.BeginBackground();

            if (this.wall != null)
            {
                wall.kMenu.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
