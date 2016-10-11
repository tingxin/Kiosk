using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Shapes;

namespace StrokesAnimationEngine
{
    public static class StrokeHelper
    {
        readonly static Guid positionKey = Guid.NewGuid();
        readonly static Guid timeStampKey = Guid.NewGuid();

        public static Point GetPosition(this Stroke owener)
        {
            return (Point)owener.DrawingAttributes.GetPropertyData(positionKey);
        }

        public static void SetPosition(this Stroke owener, Point pos)
        {
            owener.DrawingAttributes.AddPropertyData(positionKey, pos);
        }

        public static double GetTimeStamp(this Stroke owener)
        {
            return (double)owener.DrawingAttributes.GetPropertyData(timeStampKey);
        }

        public static void SetTimeStamp(this Stroke owener, double time)
        {
            owener.DrawingAttributes.AddPropertyData(timeStampKey, time);
        }
    }
}
