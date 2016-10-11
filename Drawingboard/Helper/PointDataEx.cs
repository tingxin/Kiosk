using Drawingboard.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Drawingboard.Helpers
{
    public static class PointDataEx
    {
        public static StylusPoint ToStylusPoint(this PointData data)
        {
            return new StylusPoint(data.X, data.Y);
        }

        public static PointData ToPointData(this StylusPoint point)
        {
            return new PointData(point.X, point.Y);
        }

        public static PointData Clone(this PointData src)
        {
            if (src == null)
            {
                return null;
            }

            PointData obj = new PointData();
            obj.X = src.X;
            obj.Y = src.Y;
            return obj;
        }
    }
}
