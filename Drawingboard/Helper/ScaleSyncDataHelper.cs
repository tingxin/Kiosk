using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Drawingboard.Helpers
{
    public static class ScaleSyncDataHelper
    {
        public static double ScaleXForExport(double x, double width)
        {
            return x / width;
        }

        public static double ScaleYForExport(double y, double height)
        {
            return y / height;
        }

        public static double ScaleXForImport(double x, double width)
        {
            return x * width;
        }

        public static double ScaleYForImport(double y, double height)
        {
            return y * height;
        }

        public static Point ScaleForExport(Point point, Size size)
        {
            return new Point(ScaleXForExport(point.X, size.Width), ScaleYForExport(point.Y, size.Height));
        }

        public static Point ScaleForImport(Point point, Size size)
        {
            return new Point(ScaleXForImport(point.X, size.Width), ScaleYForImport(point.Y, size.Height));
        }
    }
}
