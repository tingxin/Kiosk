using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;

namespace StrokesAnimationEngine
{
    internal static class AnimationEngine
    {
        static readonly Guid AnimationID = new Guid("B12CE0DA-2962-4e63-AEC9-3E6F03FBC7F9");
        static DispatcherTimer timer;
        static List<AnimationObj> Cache = new List<AnimationObj>();
        public const int Frequency = 15;

        static bool isPause = false;
        static int runing = 0;
        static int eventOb = 0;
        static bool needCallEvent = false;
        public static event EventHandler PrepareToAnimationEvent;
        static AnimationEngine()
        {
            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, Frequency);
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            if (isPause == false)
            {
                if (eventOb > 50)
                {
                    if (needCallEvent)
                    {
                        eventOb = 0;
                        needCallEvent = false;
                        if (PrepareToAnimationEvent != null)
                        {
                            PrepareToAnimationEvent(sender, e);
                        }
                    }
                }
                else
                {
                    if (needCallEvent)
                    {
                        eventOb++;
                    }
                }
                if (runing > 60)
                {
                    foreach (AnimationObj obj in Cache)
                    {
                        obj.Run();
                    }
                }
                else
                {
                    runing++;
                }
            }
            else
            {
                runing = 0;
                needCallEvent = false;
            }
        }

        public static string GetAnimationId(this Stroke stroke)
        {
            if (stroke.GetPropertyDataIds().Contains(AnimationID))
            {
                return stroke.GetPropertyData(AnimationID) as string;
            }
            return string.Empty;
        }

        public static void SetAnimationId(this Stroke stroke, string id)
        {
            stroke.AddPropertyData(AnimationID, id);
        }



        static void SetStrokesAnimationsId(this StrokeCollection strokes, string id)
        {
            foreach (var s in strokes)
            {
                s.SetAnimationId(id);
            }
        }

        public static AnimationObj AddDurationActionAnimation(StrokeCollection strokes, AnimationActionType type, Point centerOffset, int duration, Point from, Point to, bool isForever = false)
        {
            DurationActionAnimationObj obj = new DurationActionAnimationObj(strokes, type, centerOffset, duration, from, to);
            obj.IsForeEver = isForever;
            Cache.Add(obj);

            return obj;
        }

        public static AnimationObj AddSeriesActionsAnimation(StrokeCollection strokes, AnimationActionType type, Point centerOffset, Point from, List<AnimationTo> to, bool isForever = false)
        {
            SeriesActionsAnimationObj obj = new SeriesActionsAnimationObj(strokes, type, centerOffset, from, to);
            obj.IsForeEver = isForever;
            Cache.Add(obj);

            return obj;
        }

        public static AnimationObj AddPropertyAnimation(StrokeCollection strokes, int duration, double from, double to, Action<double> effectOnProperty, bool isForever = false)
        {
            PropertyAnimationObj obj = new PropertyAnimationObj(strokes, duration, from, to, effectOnProperty);
            obj.IsForeEver = isForever;
            Cache.Add(obj);
            return obj;
        }

        public static void AddCustomAnimation(AnimationObj obj, StrokeCollection strokes)
        {
            Cache.Add(obj);
            strokes.SetStrokesAnimationsId(obj.ID);
        }

        public static void Run()
        {
            timer.Start();
        }

        public static void Stop()
        {
            timer.Stop();
        }

        public static void Pause()
        {
            isPause = true;
        }


        public static void Resume()
        {
            isPause = false;
            needCallEvent = true;
        }


        public static void Remove(AnimationObj obj)
        {
            if (Cache.Contains(obj))
            {
                Cache.Remove(obj);
            }
        }

        public static AnimationObj Get(string animationId)
        {
            int index = -1;
            for (int i = 0; i < Cache.Count; i++)
            {

                if (Cache[i].ID == animationId)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                return Cache[index];
            }
            return null;
        }
    }

    #region AnimationObjs
    public abstract class AnimationObj
    {
        private int index = 0;

        private Point center;

        public string ID { private set; get; }
        public bool IsForeEver { set; get; }

        public Point CenterOffset { get; private set; }
        public StrokeCollection Strokes { private set; get; }
        public AnimationActionType AnimationType { private set; get; }

        public abstract int GetCount();

        public bool IsStop { private set; get; }
        public bool IsPause { private set; get; }

        public event EventHandler AnimationCompleted;

        protected abstract bool GetActionKeyValue(int index, ref Point keyChange);


        public AnimationObj(StrokeCollection strokes, AnimationActionType type, Point centerOffset)
        {
            ID = Guid.NewGuid().ToString("N");
            this.Strokes = strokes;
            this.AnimationType = type;
            this.IsPause = true;
            this.CenterOffset = centerOffset;
        }

        public void Begin()
        {
            this.Recovery();
            this.IsPause = false;
        }

        public void Pause()
        {
            this.IsPause = true;
        }

        /// <summary>
        /// You should call Begin method instead of this method, not call this method anytime
        /// </summary>
        internal void Run()
        {
            if (this.IsStop == false && this.IsPause == false)
            {
                this.index++;
                if (index >= this.GetCount())
                {
                    if (this.IsForeEver)
                    {
                        index = 0;
                        this.Recovery();
                    }
                    else
                    {
                        this.IsStop = true;
                        if (this.AnimationCompleted != null)
                        {
                            this.AnimationCompleted(this, null);
                        }
                    }
                }
                if (this.IsStop == false)
                {
                    this.RunAction(index);
                }
            }
        }

        protected virtual void Recovery()
        {
            this.index = 0;
            this.IsStop = false;
        }

        private void RunAction(int index)
        {
            Rect bound = this.Strokes.GetBounds();
            this.center = new Point(bound.Left + bound.Width * this.CenterOffset.X, bound.Top + bound.Height * this.CenterOffset.Y);
            Point posChange = new Point();
            bool needDoAction = this.GetActionKeyValue(index, ref posChange);
            if (needDoAction)
            {
                Matrix matrix = new Matrix();
                switch (this.AnimationType)
                {
                    case AnimationActionType.Translate:
                        matrix.Translate(posChange.X, posChange.Y);
                        break;
                    case AnimationActionType.Rotate:
                        matrix.RotateAt(posChange.X, this.center.X, this.center.Y);
                        break;
                    case AnimationActionType.Scale:
                        matrix.ScaleAt(1 + posChange.X, 1 + posChange.Y, this.center.X, this.center.Y);
                        break;
                    case AnimationActionType.Skew:
                        matrix.Skew(posChange.X, posChange.Y);
                        break;
                    default:
                        break;

                }
                this.Strokes.Transform(matrix, false);
            }
        }
    }

    public class DurationActionAnimationObj : AnimationObj
    {
        double xChange = 0;
        double yChange = 0;
        int totalCount = 0;
        Point posChange;

        public DurationActionAnimationObj(StrokeCollection strokes, AnimationActionType type, Point centerOffset, int duration, Point from, Point to)
            : base(strokes, type, centerOffset)
        {
            this.From = from;
            this.To = to;
            this.Duration = duration;
            this.totalCount = this.Duration * 1000 / AnimationEngine.Frequency;
            xChange = (To.X - From.X) / this.GetCount();
            yChange = (To.Y - From.Y) / this.GetCount();
            posChange = new Point(xChange, yChange);
        }


        public Point From { private set; get; }
        public Point To { private set; get; }
        public int Duration { private set; get; }

        public override int GetCount()
        {
            return this.totalCount;
        }

        protected override bool GetActionKeyValue(int index, ref Point change)
        {
            change.X = xChange;
            change.Y = yChange;
            return true;
        }
    }

    public class SeriesActionsAnimationObj : AnimationObj
    {
        double xchange = 0;
        double ychange = 0;

        double totalDuration = 0;
        int totalCount = 0;
        int mileStoneIndex = 0;

        List<int> milestone;

        public SeriesActionsAnimationObj(StrokeCollection strokes, AnimationActionType type, Point centerOffset, Point from, List<AnimationTo> to)
            : base(strokes, type, centerOffset)
        {
            this.From = from;
            this.To = to;

            if (this.To.Count > 0)
            {
                this.totalDuration = to.Sum(item => item.Duration);
                this.totalCount = (int)(this.totalDuration * 1000 / AnimationEngine.Frequency);
                this.milestone = new List<int>();

                this.milestone.Add((int)(to[0].Duration * 1000 / AnimationEngine.Frequency));
                for (int i = 1; i < to.Count; i++)
                {
                    int result = (int)(to[i].Duration * 1000 / AnimationEngine.Frequency + this.milestone[i - 1]);
                    this.milestone.Add(result);
                }

                xchange = (this.To[mileStoneIndex].To.X - this.From.X) / this.milestone[mileStoneIndex];
                ychange = (this.To[mileStoneIndex].To.Y - this.From.Y) / this.milestone[mileStoneIndex];
            }
            else
            {
                this.totalCount = 0;
            }
        }

        public Point From { private set; get; }
        public List<AnimationTo> To { private set; get; }

        public override int GetCount()
        {
            return this.totalCount;
        }

        protected override bool GetActionKeyValue(int index, ref Point change)
        {

            if (index > this.milestone[mileStoneIndex] && mileStoneIndex + 1 < this.milestone.Count)
            {
                int allCount = (this.milestone[mileStoneIndex + 1] - this.milestone[mileStoneIndex]);
                xchange = (this.To[mileStoneIndex + 1].To.X - this.To[mileStoneIndex].To.X) / allCount;
                ychange = (this.To[mileStoneIndex + 1].To.Y - this.To[mileStoneIndex].To.Y) / allCount;
                Console.WriteLine(string.Format("X chane is {0} and Y change is {1}", xchange, ychange));
                mileStoneIndex++;
            }
            if (Math.Abs(xchange) > 0 || Math.Abs(ychange) > 0)
            {
                change.X = xchange;
                change.Y = ychange;
                return true;
            }
            return false;
        }

        protected override void Recovery()
        {
            base.Recovery();

            mileStoneIndex = 0;
            xchange = (this.To[mileStoneIndex].To.X - this.From.X) / this.milestone[mileStoneIndex];
            ychange = (this.To[mileStoneIndex].To.Y - this.From.Y) / this.milestone[mileStoneIndex];
        }
    }

    public class PropertyAnimationObj : AnimationObj
    {
        double change = 0;
        public PropertyAnimationObj(StrokeCollection strokes, int duration, double from, double to, Action<double> effectOnProperty)
            : base(strokes, AnimationActionType.Property, new Point(0.5, 0.5))
        {
            this.EffectOnProperty = effectOnProperty;
            this.From = from;
            this.To = to;
            this.Duration = duration;
            this.change = (To - From) / this.GetCount();
        }
        public Action<double> EffectOnProperty { private set; get; }
        public double From { private set; get; }
        public double To { private set; get; }
        public int Duration { private set; get; }

        public override int GetCount()
        {
            int count = this.Duration * 1000 / AnimationEngine.Frequency;
            return count;
        }
        protected override bool GetActionKeyValue(int index, ref Point keyChange)
        {
            double kValue = this.From + index * change;
            this.EffectOnProperty(kValue);
            return false;
        }
    }

    public class AnimationTo
    {
        public AnimationTo(double duration, Point to)
        {
            this.Duration = duration;
            this.To = to;
        }
        public double Duration { private set; get; }
        public Point To { private set; get; }
    }

    public enum AnimationActionType
    {
        Translate,
        Scale,
        Rotate,
        Skew,
        Property
    }
    #endregion
}
