using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace Drawingboard.controls.MendStrokes
{
    class CircleStroke : ShapeStroke
    {
        bool isFixPoints = false;
        public Point Center { get; private set; }
        public double Radius { get; private set; }
        public CircleStroke(StylusPointCollection pts)
            : base(pts)
        {
            this.ShapeKey = ShapeType.Circle;
            this.Center = this.GetCenter();
            this.Radius = this.GetRadius(Center);
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {

            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }
            if (null == drawingAttributes)
            {
                throw new ArgumentNullException("drawingAttributes");
            }

            this.Center = this.GetCenter();
            this.Radius = this.GetRadius(Center);
            if (this.Radius > 0)
            {
                double thickness = Math.Min(drawingAttributes.Width, drawingAttributes.Height);
                SolidColorBrush brush2 = new SolidColorBrush(drawingAttributes.Color);
                brush2.Freeze();
                Pen newPen = new Pen(brush2, thickness);

                this.FixStylusPoint(this.Center, this.Radius);
                drawingContext.DrawEllipse(null, newPen, this.Center, this.Radius, this.Radius);
            }

        }

        public Point GetCenter()
        {
            if (this.StylusPoints.Count > 2)
            {
                double minX = double.MaxValue;
                double minY = double.MaxValue;
                double maxX = double.MinValue;
                double maxY = double.MinValue;
                foreach (var point in this.StylusPoints)
                {

                    if (maxX < point.X)
                    {
                        maxX = point.X;
                    }
                    if (minX > point.X)
                    {
                        minX = point.X;
                    }
                    if (maxY < point.Y)
                    {
                        maxY = point.Y;
                    }
                    if (minY > point.Y)
                    {
                        minY = point.Y;
                    }

                }

                return new Point((maxX + minX) / 2, (maxY + minY) / 2);
            }
            else if (this.StylusPoints.Count == 2)
            {
                return new Point(this.StylusPoints[0].X, this.StylusPoints[0].Y);
            }
            else
            {
                return new Point(0, 0);
            }
        }

        public double GetRadius(Point center)
        {
            if (this.StylusPoints.Count > 2)
            {
                double radius = 0.0;
                foreach (var item in this.StylusPoints)
                {
                    radius += Math.Sqrt(Math.Pow((item.X - center.X), 2) + Math.Pow((item.Y - center.Y), 2));
                }
                radius = radius / this.StylusPoints.Count;
                return radius;
            }
            else if (this.StylusPoints.Count == 2)
            {
                return this.StylusPoints[1].X;
            }
            else
            {
                return 0;
            }
        }

        void FixStylusPoint(Point center, double radius)
        {
            if (this.isFixPoints == false)
            {
                this.isFixPoints = true;
                int count = this.StylusPoints.Count < 10 ? 10 : this.StylusPoints.Count;
                double startX = center.X - radius;
                StylusPointCollection halfTop = new StylusPointCollection();

                int halfcount = count / 2 + 1;
                double offset = (radius * 2) / halfcount;
                halfTop.Add(new StylusPoint(startX, center.Y, 0.5f));
                for (int i = 0; i < halfcount; i++)
                {
                    double x = startX + i * offset;
                    double y = center.Y;

                    double nextYvalue = Math.Sqrt(radius * radius - Math.Pow((x - center.X), 2));
                    if (double.IsNaN(nextYvalue) == false)
                    {
                        y += nextYvalue;
                    }

                    halfTop.Add(new StylusPoint(x, y, 0.5f));
                }
                halfTop.Add(new StylusPoint(center.X + radius, center.Y, 0.5f));
                for (int i = halfcount - 1; i > 0; i--)
                {
                    StylusPoint point = halfTop[i];
                    double y = 2 * center.Y - point.Y;
                    halfTop.Add(new StylusPoint(point.X, y, 0.5f));
                }


                this.StylusPoints = halfTop;
            }
        }
    }
}
