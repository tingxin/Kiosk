using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows;
using KComponents;

namespace Drawingboard.controls.MendStrokes
{
    class EllipseStroke : Stroke
    {
        public EllipseStroke(StylusPointCollection pts)
            : base(pts)
        {
            this.StylusPoints = pts;
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (this.StylusPoints.Count > 0)
            {
                //if (drawingContext == null)
                //{
                //    throw new ArgumentNullException("drawingContext");
                //}
                //if (null == drawingAttributes)
                //{
                //    throw new ArgumentNullException("drawingAttributes");
                //}
                //double thickness = Math.Min(drawingAttributes.Width, drawingAttributes.Height);
                //SolidColorBrush brush2 = new SolidColorBrush(drawingAttributes.Color);
                //brush2.Freeze();

                //double radius = this.Getradius(center);
                //Pen newPen = new Pen(brush2, thickness);

                //drawingContext.DrawEllipse(null, newPen, center, radius, radius);
            }
        }

        public Point GetCenter(StylusPointCollection collection, out double aRadius, out double bRadius)
        {
            //y=a+bx;
            double xAverage = 0.0;
            double yAverage = 0.0;
            aRadius = 0.0;
            bRadius = 0.0;
            double b = 0.0;
            double a = 0.0;

            Point centerPoint = new Point();

            foreach (var point in collection)
            {
                xAverage += point.X;
                yAverage += point.Y;

            }

            xAverage = xAverage / collection.Count;
            yAverage = yAverage / collection.Count;

            double fenzi = 0.0;
            double fenmu = 0.0;
            foreach (var point in collection)
            {
                fenzi += point.X * point.Y;
                fenmu += point.X * point.X;
            }

            fenzi = fenzi - collection.Count * xAverage * yAverage;
            fenmu = fenmu - collection.Count * xAverage * xAverage;
            if (fenmu != 0)
            {
                b = fenzi / fenmu;
            }
            if (fenmu == 0 || fenzi == 0 || Math.Abs(b) > 6)
            {
                //vertical line
                double maxY = double.MinValue;
                double minY = double.MaxValue;
                double maxX = double.MinValue;
                double minX = double.MaxValue;

                foreach (var point in collection)
                {
                    if (maxY < point.Y)
                    {
                        maxY = point.Y;
                    }
                    if (minY > point.Y)
                    {
                        minY = point.Y;
                    }

                    if (maxX < point.X)
                    {
                        maxX = point.X;
                    }
                    if (minX > point.X)
                    {
                        minX = point.X;
                    }
                }
                double centerY = (maxY + minY) / 2;
                centerPoint.X = xAverage;
                centerPoint.Y = centerY;
                bRadius = (maxY - minY) / 2;
                aRadius = (maxX - minX) / 2;
                return centerPoint;
            }
            else
            {

                a = yAverage - b * xAverage;
                StylusPoint maxDisPoint = collection[0];
                StylusPoint minDisPoint = collection[0];
                double maxDis = double.MinValue;
                double minDis = double.MaxValue;
                foreach (var point in collection)
                {
                    //bx-y+a=0;
                    double dis = MathMethods.GetDistanceBetPointToLine(point.X, point.Y, b, -1, a);
                    if (dis > maxDis)
                    {
                        maxDis = dis;
                        maxDisPoint = point;
                    }
                    if (dis < minDis)
                    {
                        minDis = dis;
                        minDisPoint = point;
                    }
                }


            }
            return centerPoint;
        }

        public double Getradius(Point center)
        {
            double radius = 0.0;
            foreach (var item in this.StylusPoints)
            {
                radius += Math.Sqrt(Math.Pow((item.X - center.X), 2) + Math.Pow((item.Y - center.Y), 2));
            }
            radius = radius / this.StylusPoints.Count;
            return radius;
        }
    }
}


