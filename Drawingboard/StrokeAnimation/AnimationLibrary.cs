using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StrokesAnimationEngine
{
    internal class AnimationLibrary
    {
        Random rand;
        readonly int defaultRepeatTimes = 4;

        public double ScreenWidth { set; get; }
        public double ScreenHeight { set; get; }
        private AnimationLibrary()
        {
            rand = new Random(DateTime.Now.TimeOfDay.Milliseconds);
            this.ScreenWidth = 1920;
            this.ScreenHeight = 1080;
        }

        static AnimationLibrary current;

        class AnimationThread
        {
            public AnimationThread()
            {
                Paramaters = new List<object>();
            }
            public List<object> Paramaters { private set; get; }
            public Action<List<AnimationTo>> CompletedEvent { set; get; }
        }

        public static AnimationLibrary Current
        {
            get
            {
                if (current == null)
                {
                    current = new AnimationLibrary();
                }
                return current;
            }
        }

        #region Private Method
        private AnimationTo GetRandomPosition()
        {
            double xPos = this.rand.NextDouble() * this.ScreenWidth;
            double yPos = this.rand.NextDouble() * this.ScreenHeight;
            Point to = new Point(xPos, yPos);
            AnimationTo at = new AnimationTo(0.2, to);
            return at;
        }

        private Point GetCenter(Rect bound)
        {
            Point center = new Point(bound.Left + bound.Width / 2, bound.Top + bound.Height / 2);
            return center;
        }
        #endregion

        #region FlashAnimation
        public void GetFlashAnimationSync(Rect bound, Action<List<AnimationTo>> completedEvent)
        {
            AnimationThread obj = new AnimationThread();
            obj.Paramaters.Add(bound);
            obj.CompletedEvent = completedEvent;

            Thread thread = new Thread(new ParameterizedThreadStart(this.GetFlashAnimationBackground));
            thread.IsBackground = true;
            thread.Name = "FlashAnimation";
            thread.Start(obj);
        }

        void GetFlashAnimationBackground(object obj)
        {
            AnimationThread paramater = (AnimationThread)obj;
            Rect bound = (Rect)(paramater.Paramaters[0]);

            int times = this.rand.Next(5, 200);

            List<AnimationTo> result = new List<AnimationTo>();
            for (int i = 0; i < times; i++)
            {
                AnimationTo at = this.GetRandomPosition();
                Rect posRec = new Rect(at.To.X, at.To.Y, bound.Width, bound.Height);
                result.Add(at);
                AnimationTo stay = new AnimationTo(this.rand.NextDouble() * 3, at.To);
                result.Add(stay);
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                paramater.CompletedEvent(result);
            }));
        }


        #endregion

        #region JumpAnimation
        public void GetJumpAnimationSync(Rect bound, int jumpTimes, Action<List<AnimationTo>> completedEvent)
        {
            AnimationThread obj = new AnimationThread();
            obj.Paramaters.Add(bound);
            obj.Paramaters.Add(jumpTimes);
            obj.CompletedEvent = completedEvent;

            Thread thread = new Thread(new ParameterizedThreadStart(this.GetJumpAnimationBackground));
            thread.IsBackground = true;
            thread.Name = "JumpAnimation";
            thread.Start(obj);
        }

        void GetJumpAnimationBackground(object obj)
        {
            AnimationThread paramater = (AnimationThread)obj;
            Rect bound = (Rect)(paramater.Paramaters[0]);
            int jumpTimes = (int)(paramater.Paramaters[1]);

            List<AnimationTo> jumps = this.GetJump(bound, jumpTimes);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                paramater.CompletedEvent(jumps);
            }));
        }

        List<AnimationTo> GetJump(Rect bound, int jumpTmes = 0)
        {
            List<AnimationTo> result = new List<AnimationTo>();
            Point from = this.GetCenter(bound);
            int times = jumpTmes;
            if (times == 0)
            {
                this.rand.Next(1, 10);
            }

            double yChange = 0 - (this.rand.NextDouble() + 1) * bound.Height;
            double yBackChane = 0 - yChange;


            for (int i = 0; i < times; i++)
            {
                double xChange = (this.rand.NextDouble() - 0.5) * bound.Height;
                double xBackChane = 0 - xChange;

                Point pChane = new Point(from.X + xChange, from.Y + yChange);

                AnimationTo atChange = new AnimationTo(0.3, pChane);
                AnimationTo atBack = new AnimationTo(0.3, from);
                AnimationTo stay = new AnimationTo(1, from);
                result.Add(atChange);
                result.Add(atBack);
                result.Add(stay);
            }

            return result;
        }
        #endregion

        #region ShockAnimation
        public void GetShockAnimationSync(Rect bound, AOrientation orientation, int jumpTimes, Action<List<AnimationTo>> completedEvent)
        {
            AnimationThread obj = new AnimationThread();
            obj.Paramaters.Add(bound);
            obj.Paramaters.Add(jumpTimes);
            obj.Paramaters.Add(orientation);
            obj.CompletedEvent = completedEvent;

            Thread thread = new Thread(new ParameterizedThreadStart(this.GetShockAnimationBackground));
            thread.IsBackground = true;
            thread.Name = "ShockAnimation";
            thread.Start(obj);
        }

        void GetShockAnimationBackground(object obj)
        {
            AnimationThread paramater = (AnimationThread)obj;
            Rect bound = (Rect)(paramater.Paramaters[0]);
            int jumpTimes = (int)(paramater.Paramaters[1]);
            AOrientation orientation = (AOrientation)(paramater.Paramaters[2]);
            List<AnimationTo> jumps = this.GetShock(bound, orientation, jumpTimes);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                paramater.CompletedEvent(jumps);
            }));
        }

        List<AnimationTo> GetShock(Rect bound, AOrientation orientation, int ShockTimes = 0)
        {
            List<AnimationTo> result = new List<AnimationTo>();
            Point from = this.GetCenter(bound);
            int times = ShockTimes;
            if (times == 0)
            {
                this.rand.Next(1, 10);
            }


            double change = 0;
            if (orientation == AOrientation.Horizontal)
            {
                change = (this.rand.NextDouble() + 0.2) * bound.Width;
            }
            else
            {
                change = (this.rand.NextDouble() + 0.2) * bound.Height;
            }

            double backChange = 0 - change;


            for (int i = 0; i < times; i++)
            {
                double ratio = change * (times - i) / times;
                Point pChane;
                Point pBack;
                if (orientation == AOrientation.Horizontal)
                {
                    pChane = new Point(from.X + ratio, from.Y);
                    pBack = new Point(from.X - ratio, from.Y);
                }
                else
                {
                    pChane = new Point(from.X, from.Y + ratio);
                    pBack = new Point(from.X, from.Y - ratio);
                }

                AnimationTo at1 = new AnimationTo(0.5, pChane);
                AnimationTo at2 = new AnimationTo(0.5, from);
                AnimationTo at3 = new AnimationTo(0.5, pBack);
                AnimationTo at4 = new AnimationTo(0.5, from);
                result.Add(at1);
                result.Add(at2);
                result.Add(at3);
                result.Add(at4);
            }

            return result;
        }
        #endregion

        #region ShakeAnimation
        public void GetShakeAnimationSync(Rect bound, AOrientation orientation, int shakeTimes, Action<List<AnimationTo>> completedEvent)
        {
            AnimationThread obj = new AnimationThread();
            obj.Paramaters.Add(bound);
            obj.Paramaters.Add(shakeTimes);
            obj.Paramaters.Add(orientation);
            obj.CompletedEvent = completedEvent;

            Thread thread = new Thread(new ParameterizedThreadStart(this.GetShakeAnimationBackground));
            thread.IsBackground = true;
            thread.Name = "ShakeAnimation";
            thread.Start(obj);
        }

        void GetShakeAnimationBackground(object obj)
        {
            AnimationThread paramater = (AnimationThread)obj;
            Rect bound = (Rect)(paramater.Paramaters[0]);
            int shakeTimes = (int)(paramater.Paramaters[1]);
            AOrientation orientation = (AOrientation)(paramater.Paramaters[2]);
            List<AnimationTo> jumps = this.GetShake(bound, orientation, shakeTimes);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                paramater.CompletedEvent(jumps);
            }));
        }

        List<AnimationTo> GetShake(Rect bound, AOrientation orientation, int ShockTimes = 0)
        {
            List<AnimationTo> result = new List<AnimationTo>();
            Point from = this.GetCenter(bound);
            int times = ShockTimes;
            if (times == 0)
            {
                this.rand.Next(1, 10);
            }


            double change = 0;
            if (orientation == AOrientation.Horizontal)
            {
                change = (this.rand.NextDouble() + 0.1) * bound.Width / 4;
            }
            else
            {
                change = (this.rand.NextDouble() + 0.1) * bound.Height / 4;
            }

            double backChange = 0 - change;


            for (int i = 0; i < times; i++)
            {
                Point pChane;
                Point pBack;
                if (orientation == AOrientation.Horizontal)
                {
                    pChane = new Point(from.X + change, from.Y);
                    pBack = new Point(from.X - change, from.Y);
                }
                else
                {
                    pChane = new Point(from.X, from.Y + change);
                    pBack = new Point(from.X, from.Y - change);
                }

                AnimationTo at1 = new AnimationTo(0.5, pChane);
                AnimationTo at2 = new AnimationTo(0.5, from);
                AnimationTo at3 = new AnimationTo(0.5, pBack);
                AnimationTo at4 = new AnimationTo(0.5, from);
                result.Add(at1);
                result.Add(at2);
                result.Add(at3);
                result.Add(at4);
            }

            return result;
        }
        #endregion

        #region SwingAnimation
        public void GetSwingAnimationSync(Rect bound, int swingTimes, double range, Action<List<AnimationTo>> completedEvent)
        {
            AnimationThread obj = new AnimationThread();
            obj.Paramaters.Add(bound);
            obj.Paramaters.Add(swingTimes);
            obj.Paramaters.Add(range);
            obj.CompletedEvent = completedEvent;

            Thread thread = new Thread(new ParameterizedThreadStart(this.GetSwingAnimationBackground));
            thread.IsBackground = true;
            thread.Name = "SwingAnimation";
            thread.Start(obj);
        }

        void GetSwingAnimationBackground(object obj)
        {
            AnimationThread paramater = (AnimationThread)obj;
            Rect bound = (Rect)(paramater.Paramaters[0]);
            int swingTimes = (int)(paramater.Paramaters[1]);
            double range = (double)(paramater.Paramaters[2]);
            List<AnimationTo> jumps = this.GetSwing(bound, range, swingTimes);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                paramater.CompletedEvent(jumps);
            }));
        }

        List<AnimationTo> GetSwing(Rect bound, double range, int swingTimes = 0)
        {
            List<AnimationTo> result = new List<AnimationTo>();
            int times = swingTimes;
            if (times == 0)
            {
                this.rand.Next(1, defaultRepeatTimes);
            }

            for (int i = 0; i < times; i++)
            {
                Point ori = new Point(0, 0);
                Point pChane = new Point(range, range);
                Point pBack = new Point(0 - range, 0 - range);

                AnimationTo at1 = new AnimationTo(0.5, pChane);
                AnimationTo at2 = new AnimationTo(0.5, ori);
                AnimationTo at3 = new AnimationTo(0.5, pBack);
                AnimationTo at4 = new AnimationTo(0.5, ori);
                result.Add(at1);
                result.Add(at2);
                result.Add(at3);
                result.Add(at4);
            }

            return result;
        }
        #endregion

        #region BlinkAnimation
        public void GetBlinkAnimationSync(Rect bound, AOrientation orientation, int blinkTimes, double duration, Action<List<AnimationTo>> completedEvent)
        {
            AnimationThread obj = new AnimationThread();
            obj.Paramaters.Add(bound);
            obj.Paramaters.Add(orientation);
            obj.Paramaters.Add(blinkTimes);
            obj.Paramaters.Add(duration);
            obj.CompletedEvent = completedEvent;

            Thread thread = new Thread(new ParameterizedThreadStart(this.GetBlinkAnimationBackground));
            thread.IsBackground = true;
            thread.Name = "BlinkAnimation";
            thread.Start(obj);
        }

        void GetBlinkAnimationBackground(object obj)
        {
            AnimationThread paramater = (AnimationThread)obj;
            Rect bound = (Rect)(paramater.Paramaters[0]);
            AOrientation orientation = (AOrientation)(paramater.Paramaters[1]);
            int blinkTimes = (int)(paramater.Paramaters[2]);
            double duration = (double)(paramater.Paramaters[3]);
            List<AnimationTo> jumps = this.GetBlink(bound, orientation, duration, blinkTimes);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                paramater.CompletedEvent(jumps);
            }));
        }

        List<AnimationTo> GetBlink(Rect bound, AOrientation orientation, double duration, int blinkTimes = 0)
        {
            List<AnimationTo> result = new List<AnimationTo>();
            int times = blinkTimes;
            if (times == 0)
            {
                this.rand.Next(1, defaultRepeatTimes);
            }

            double xChange = orientation == AOrientation.Horizontal ? 0.1 : 1;
            double yChange = orientation == AOrientation.Vertical ? 0.1 : 1;

            for (int i = 0; i < times; i++)
            {
                Point ori = new Point(1, 1);
                Point pChane = new Point(xChange, yChange);
                int offset = rand.Next(10, 30);
                AnimationTo at1 = new AnimationTo(duration, pChane);
                AnimationTo at2 = new AnimationTo(duration, ori);
                AnimationTo at3 = new AnimationTo(offset * duration, ori);
                result.Add(at1);
                result.Add(at2);
                result.Add(at3);
            }

            return result;
        }
        #endregion

        #region GoAnimation
        public void GetGoAnimationSync(Rect bound, Rect screenBound, ATowardto toward, bool stopInBound, Action<List<AnimationTo>> completedEvent)
        {
            AnimationThread obj = new AnimationThread();
            obj.Paramaters.Add(bound);
            obj.Paramaters.Add(screenBound);
            obj.Paramaters.Add(toward);
            obj.Paramaters.Add(stopInBound);
            obj.CompletedEvent = completedEvent;

            Thread thread = new Thread(new ParameterizedThreadStart(this.GetGoAnimationBackground));
            thread.IsBackground = true;
            thread.Name = "GoAnimation";
            thread.Start(obj);
        }

        void GetGoAnimationBackground(object obj)
        {
            AnimationThread paramater = (AnimationThread)obj;
            Rect bound = (Rect)(paramater.Paramaters[0]);
            Rect screenBound = (Rect)(paramater.Paramaters[1]);
            ATowardto toward = (ATowardto)(paramater.Paramaters[2]);
            bool stopInBound = (bool)(paramater.Paramaters[3]);
            List<AnimationTo> jumps = this.GetGo(bound, screenBound, toward, stopInBound);
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                paramater.CompletedEvent(jumps);
            }));
        }

        List<AnimationTo> GetGo(Rect bound, Rect screenBound, ATowardto toward, bool stopInBound)
        {
            List<AnimationTo> result = new List<AnimationTo>();
            Point from = this.GetCenter(bound);
            double duration = rand.NextDouble() * 5 + 5;

            Point target1 = new Point();
            switch (toward)
            {
                case ATowardto.ToTop:
                    target1.Y = stopInBound ? bound.Height / 2 : 0 - bound.Height / 2;
                    target1.X = from.X;
                    break;
                case ATowardto.ToBottom:
                    target1.Y = stopInBound ? screenBound.Height - bound.Height / 2 : 0 - bound.Height / 2;
                    target1.X = from.X;
                    break;
                case ATowardto.ToLeft:
                    target1.X = stopInBound ? bound.Width / 2 : 0 - bound.Width / 2;
                    target1.Y = from.Y;
                    break;
                case ATowardto.ToRight:
                    target1.X = stopInBound ? screenBound.Width - bound.Width / 2 : screenBound.Width + bound.Width / 2;
                    target1.Y = from.Y;
                    break;
                default:
                    break;
            }

            AnimationTo at1 = new AnimationTo(duration, target1);
            result.Add(at1);


            if (stopInBound == false)
            {
                double change1 = 0;
                double change2 = 0;

                Point target2 = new Point();

                switch (toward)
                {
                    case ATowardto.ToTop:
                        target2.Y = screenBound.Height + bound.Height / 2;
                        target2.X = from.X;
                        change1 = target1.Y - from.Y;
                        change2 = target2.Y - target1.Y;
                        break;
                    case ATowardto.ToBottom:
                        target2.Y = 0 - bound.Height / 2;
                        target2.X = from.X;
                        change1 = target1.Y - from.Y;
                        change2 = target2.Y - target1.Y;
                        break;
                    case ATowardto.ToLeft:
                        target2.X = screenBound.Width + bound.Width / 2;
                        target2.Y = from.Y;
                        change1 = target1.X - from.X;
                        change2 = target2.X - target1.X;
                        break;
                    case ATowardto.ToRight:
                        target2.X = 0 - bound.Width / 2;
                        target2.Y = from.Y;
                        change1 = target1.X - from.X;
                        change2 = target2.X - target1.X;
                        break;
                    default:
                        break;

                }

                double dur2 = duration * change2 / change1;
                AnimationTo at2 = new AnimationTo(dur2, target2);
                result.Add(at2);
                result.Add(at1);
            }

            return result;
        }
        #endregion
    }

    public enum AOrientation
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum ATowardto
    {
        ToLeft,
        ToTop,
        ToRight,
        ToBottom
    }
}
