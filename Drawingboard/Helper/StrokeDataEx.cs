using Drawingboard.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;

namespace Drawingboard.Helpers
{
    public static class StrokeDataEx
    {
        public static StylusPointCollection ToStylusPoints(this List<PointData> list, Size size)
        {
            ScaleStylusPointsForImportData(list, size);
            StylusPointCollection coll = new StylusPointCollection();
            foreach (var i in list)
            {
                coll.Add(i.ToStylusPoint());
            }

            return coll;
        }

        public static List<PointData> ToPoints(this StylusPointCollection coll, Size size)
        {
            List<PointData> list = new List<PointData>();

            foreach (var i in coll)
            {
                list.Add(new PointData(i.X, i.Y));
            }

            ScaleStylusPointsForExportData(list, size);
            return list;
        }

        private static void ScaleStylusPointsForExportData(this List<PointData> list, Size size, int digits = 7)
        {
            foreach (var i in list)
            {
                i.X = Math.Round(ScaleSyncDataHelper.ScaleXForExport(i.X, size.Width), digits);
                i.Y = Math.Round(ScaleSyncDataHelper.ScaleYForExport(i.Y, size.Height), digits);
            }
        }

        private static void ScaleStylusPointsForImportData(this List<PointData> list, Size size)
        {
            foreach (var i in list)
            {
                i.X = ScaleSyncDataHelper.ScaleXForImport(i.X, size.Width);
                i.Y = ScaleSyncDataHelper.ScaleYForImport(i.Y, size.Height);
            }
        }
    }
}
