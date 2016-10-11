using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace KComponents
{

    public class DashStroke : Stroke
    {

        private static double[] dashs = new double[] { 3.0, 3.0 };

        private static double Thickness = 2.5;

        public DashStroke(StylusPointCollection spc)

            : base(spc)
        {

        }
        public void AddPoints(IEnumerable<StylusPoint> stylusPoints)
        {
            foreach (StylusPoint point in stylusPoints)
            {
                base.StylusPoints.Add(point);
            }
        }

        protected override void DrawCore(DrawingContext dc, DrawingAttributes da)
        {
            Brush brush = new SolidColorBrush(da.Color);

            StylusPointCollection stylusPoints = base.StylusPoints;

            if ((dashs == null) || (dashs.Length == 0))
            {
                base.DrawCore(dc, da);
            }

            else if (stylusPoints.Count > 0)
            {
                Pen pen = new Pen
                {
                    Brush = brush,

                    Thickness = Thickness,

                    DashStyle = new DashStyle(dashs, 0.0),

                    DashCap = PenLineCap.Round,

                    LineJoin = PenLineJoin.Round,

                    MiterLimit = 0.0
                };

                PathGeometry geometry = new PathGeometry();

                PathFigure figure = new PathFigure
                {
                    StartPoint = (Point)stylusPoints[0],

                    IsClosed = false
                };

                for (int i = 1; i < stylusPoints.Count; i++)
                {
                    figure.Segments.Add(new LineSegment((Point)stylusPoints[i], true));
                }

                geometry.Figures.Add(figure);

                dc.DrawGeometry(null, pen, geometry);

                dc.DrawGeometry(brush, null, new EllipseGeometry((Point)stylusPoints[0], Thickness / 2.0, Thickness / 2.0));

                dc.DrawGeometry(brush, null, new EllipseGeometry((Point)stylusPoints[stylusPoints.Count - 1], Thickness / 2.0, Thickness / 2.0));
            }
        }
    }
}
