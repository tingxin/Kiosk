using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using KComponents;

namespace Drawingboard.controls.MendStrokes
{
    class ShapeCornerFinder
    {
        static double minLineLength = 100;
        static double minTriangleBorder = 200;
        static double minRectangleBorder = 250;

        public static StylusPointCollection GetTriangleCorner(StylusPointCollection collection)
        {
            try
            {
                #region get max values
                double maxX = double.MinValue, minX = double.MaxValue, maxY = double.MinValue, minY = double.MaxValue;
                foreach (StylusPoint point in collection)
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
                double borderLength = MathMethods.GetDistanceBetTowPoints(maxX, maxY, minX, minY);
                if (borderLength < minTriangleBorder)
                {
                    return null;
                }

                List<double> data = new List<double>();
                data.Add(maxX);
                data.Add(minX);
                data.Add(maxY);
                data.Add(minY);
                #endregion

                #region Get bounds
                StylusPointCollection bounds = new StylusPointCollection();

                foreach (StylusPoint point in collection)
                {
                    bool isBound = data.Any(item => (item == point.X) || (item == point.Y));
                    if (isBound)
                    {
                        bounds.Add(point);
                    }
                }

                if (bounds.Count == 3)
                {
                    return bounds;
                }
                #endregion

                StylusPointCollection pointsPre = new StylusPointCollection();
                StylusPoint corner1 = bounds.FirstOrDefault(item => item.X == maxX);//1
                StylusPoint corner2 = bounds.FirstOrDefault(item => item.Y == maxY);//2
                StylusPoint corner3 = bounds.FirstOrDefault(item => item.X == minX);//3
                StylusPoint corner4 = bounds.FirstOrDefault(item => item.Y == minY);//4
                corner1.PressureFactor = 0.5f;
                corner2.PressureFactor = 0.5f;
                corner3.PressureFactor = 0.5f;
                corner4.PressureFactor = 0.5f;
                if (corner1 != null)
                {
                    pointsPre.Add(corner1);
                }
                if (corner2 != null)
                {
                    pointsPre.Add(corner2);
                }
                if (corner3 != null)
                {
                    pointsPre.Add(corner3);
                }
                if (corner4 != null)
                {
                    pointsPre.Add(corner4);
                }
                if (pointsPre.Count > 3)
                {

                    //1,2,3;
                    double area1 = GetArea(corner1.X, corner1.Y, corner2.X, corner2.Y, corner3.X,
                       corner3.Y);
                    //1,2,4
                    double area2 = GetArea(corner1.X, corner1.Y, corner2.X, corner2.Y, corner4.X,
                       corner4.Y);

                    //1,3,4
                    double area3 = GetArea(corner1.X, corner1.Y, corner3.X, corner3.Y, corner4.X,
                       corner4.Y);

                    //2,3,4
                    double area4 = GetArea(corner2.X, corner2.Y, corner3.X, corner3.Y, corner4.X,
                       corner4.Y);
                    StylusPointCollection corners = new StylusPointCollection();
                    double maxArea = area1;
                    if (maxArea < area2)
                    {
                        maxArea = area2;
                    }
                    if (maxArea < area3)
                    {
                        maxArea = area3;
                    }
                    if (maxArea < area4)
                    {
                        maxArea = area4;
                    }
                    if (maxArea == area1)
                    {
                        corners.Add(corner1);
                        corners.Add(corner2);
                        corners.Add(corner3);
                    }
                    else if (maxArea == area2)
                    {
                        corners.Add(corner1);
                        corners.Add(corner2);
                        corners.Add(corner4);
                    }
                    else if (maxArea == area3)
                    {
                        corners.Add(corner1);
                        corners.Add(corner3);
                        corners.Add(corner4);
                    }
                    else//==area4
                    {
                        corners.Add(corner2);
                        corners.Add(corner3);
                        corners.Add(corner4);
                    }
                    StylusPoint closedPoint = new StylusPoint(corners[0].X, corners[0].Y, 0.5f);
                    corners.Add(closedPoint);
                    return corners;
                }

                return pointsPre;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("exception on find triangle is {0}", ex.StackTrace));
            }

            return null;
        }

        /// <summary>
        /// Get line corner
        /// </summary>
        /// <param name="collection">sample points</param>
        /// <param name="offset">0-100</param>
        /// <returns>line stylus points</returns>
        public static StylusPointCollection GetLineCorner(StylusPointCollection collection, double ratio = 1.5)
        {
            //y=a+bx;
            try
            {
                Debug.Assert(ratio > 1 && ratio < 25, "ratio should be in 1-25");
                Point startPoint = collection[0].ToPoint();
                Point endPoint = collection[collection.Count - 1].ToPoint();
                double lineLength = MathMethods.GetDistanceBetTowPoints(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                if (lineLength < minLineLength)
                {
                    return null;
                }

                double xAverage = 0.0;
                double yAverage = 0.0;
                double endX = double.MinValue;
                double startX = double.MaxValue;


                double b = 0.0;
                double a = 0.0;
                foreach (var point in collection)
                {
                    xAverage += point.X;
                    yAverage += point.Y;

                    if (endX < point.X)
                    {
                        endX = point.X;
                    }
                    if (startX > point.X)
                    {
                        startX = point.X;
                    }

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
                double offset = ratio * lineLength / 100;
                if (fenmu == 0 || fenzi == 0 || Math.Abs(b) > 6)
                {
                    //vertical line

                    double startYtest = double.MaxValue;
                    double endYtest = double.MinValue;
                    int unExpectPoints = 0;
                    foreach (var point in collection)
                    {
                        if (endYtest < point.Y)
                        {
                            endYtest = point.Y;
                        }
                        if (startYtest > point.Y)
                        {
                            startYtest = point.Y;
                        }

                        if (Math.Abs(xAverage - point.X) > offset * 2)
                        {
                            unExpectPoints++;
                        }
                    }
                    if (unExpectPoints > collection.Count / 4)
                    {
                        return null;
                    }
                    StylusPointCollection result = new StylusPointCollection();
                    result.Add(new StylusPoint(xAverage, startYtest, 0.5f));
                    result.Add(new StylusPoint(xAverage, endYtest, 0.5f));
                    return result;
                }
                else
                {

                    a = yAverage - b * xAverage;
                    int unExpectPoints = 0;
                    foreach (var point in collection)
                    {
                        double offsetY = a + b * point.X;
                        if (Math.Abs(offsetY - point.Y) > offset)
                        {
                            unExpectPoints++;
                        }
                    }
                    if (unExpectPoints > collection.Count / 5)
                    {

                        return null;
                    }
                    double startY = a + b * startX;
                    double endY = a + b * endX;
                    StylusPointCollection result = new StylusPointCollection();
                    result.Add(new StylusPoint(startX, startY, 0.5f));
                    result.Add(new StylusPoint(endX, endY, 0.5f));
                    return result;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.StackTrace);
#if DEBUG
                throw ex;
#else
                return null;
#endif
            }


        }

        public static StylusPointCollection GetArrowCorner(StylusPointCollection collection, double ratio = 5)
        {

            try
            {
                StylusPointCollection corners = GetTriangleCorner(collection);
                if (corners != null && corners.Count == 4)
                {
                    StylusPoint first = collection[0];
                    StylusPoint end = collection[collection.Count - 1];
                    StylusPoint firstCorner = corners[0];
                    StylusPoint endCorner = corners[1];

                    double minFistDis = double.MaxValue;
                    double minEndDis = double.MaxValue;
                    foreach (StylusPoint point in corners)
                    {
                        double dis = MathMethods.GetPowDistanceBetTowPoints(point.X, point.Y, first.X, first.Y);
                        double endDis = MathMethods.GetPowDistanceBetTowPoints(point.X, point.Y, end.X, end.Y);

                        if (minFistDis > dis)
                        {
                            minFistDis = dis;
                            firstCorner = point;
                        }

                        if (minEndDis > endDis)
                        {
                            minEndDis = endDis;
                            endCorner = point;
                        }

                    }

                    StylusPoint centerCorner = corners.FirstOrDefault(item => item != firstCorner && item != endCorner);
                    if (centerCorner != null && centerCorner.X > 0 && centerCorner.Y > 0)
                    {
                        double cf = MathMethods.GetDistanceBetTowPoints(centerCorner.X, centerCorner.Y, firstCorner.X, firstCorner.Y);
                        double ce = MathMethods.GetDistanceBetTowPoints(centerCorner.X, centerCorner.Y, endCorner.X, endCorner.Y);

                        double cf_ce = Math.Abs(cf - ce);
                        if (cf_ce > 150)
                        {
                            return null;
                        }
                        double fe = MathMethods.GetDistanceBetTowPoints(firstCorner.X, firstCorner.Y, endCorner.X, endCorner.Y);
                        //　cosA = (c^2 + b^2 - a^2) / (2·b·c) 
                        double cosA = (cf * cf + ce * ce - fe * fe) / (2 * cf * ce);
                        if (cosA < 0.04 || cosA > 0.93)
                        {
                            return null;
                        }

                        #region Check bound is two line
                        //Get line one function
                        double line1K = (centerCorner.Y - firstCorner.Y) / (centerCorner.X - firstCorner.X);
                        double line1A = line1K;
                        double line1B = -1;
                        double line1C = centerCorner.Y - line1K * centerCorner.X;

                        //Get line two function
                        double line2K = (centerCorner.Y - endCorner.Y) / (centerCorner.X - endCorner.X);
                        double line2A = line2K;
                        double line2B = -1;
                        double line2C = centerCorner.Y - line2K * centerCorner.X;

                        int line1Count = 0;
                        int line2Count = 0;

                        double lineDis1 = MathMethods.GetDistanceBetTowPoints(centerCorner.X, centerCorner.Y, firstCorner.X, firstCorner.Y);
                        double lineDis2 = MathMethods.GetDistanceBetTowPoints(centerCorner.X, centerCorner.Y, endCorner.X, endCorner.Y);
                        double line1offset = lineDis1 * ratio / 100;
                        double line2offset = lineDis2 * ratio / 100;
                        for (int i = 0; i < collection.Count; i++)
                        {
                            double dis1 = MathMethods.GetDistanceBetPointToLine(collection[i].X, collection[i].Y, line1A, line1B, line1C);


                            if (dis1 < line1offset)
                            {
                                line1Count++;
                            }
                            else
                            {
                                double dis2 = MathMethods.GetDistanceBetPointToLine(collection[i].X, collection[i].Y, line2A, line2B, line2C);
                                if (dis2 < line2offset)
                                {
                                    line2Count++;
                                }
                            }
                        }

                        if (collection.Count - (line1Count + line2Count) > collection.Count / 20)
                        {
                            return null;
                        }
                        #endregion

                        StylusPoint maxDisPoint = firstCorner;
                        StylusPoint minDisPoint = endCorner;
                        double maxDisCenter = cf;
                        double minDisCenter = ce;
                        if (cf < ce)
                        {
                            maxDisPoint = endCorner;
                            minDisPoint = firstCorner;
                            maxDisCenter = ce;
                            minDisCenter = cf;
                        }

                        //line:y=a+bx;
                        double b = (maxDisPoint.Y - centerCorner.Y) / (maxDisPoint.X - centerCorner.X);//(y1-y2)/(x1-x2)
                        double a = 0 - b * centerCorner.X + centerCorner.Y;//-b*x1+y2

                        double startTest = Math.Min(maxDisPoint.X, centerCorner.X);
                        double endTest = Math.Max(maxDisPoint.X, centerCorner.X);
                        Point targetPoint = new Point(maxDisPoint.X, maxDisPoint.Y);

                        for (double i = startTest; i < endTest; i += 1)
                        {
                            double y = a + b * i;
                            double dis = MathMethods.GetDistanceBetTowPoints(i, y, centerCorner.X, centerCorner.Y);
                            if (Math.Abs(minDisCenter - dis) <= 5)
                            {
                                targetPoint.X = i;
                                targetPoint.Y = y;
                                break;
                            }
                        }

                        if (targetPoint.X <= 0 || centerCorner.X <= 0)
                        {
#if DEBUG
                            throw new Exception("targetPoint.X should noe be zero");
#endif
                        }
                        StylusPointCollection result = new StylusPointCollection();

                        result.Add(new StylusPoint(targetPoint.X, targetPoint.Y, 0.5f));
                        result.Add(centerCorner);
                        result.Add(minDisPoint);

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.StackTrace);
#if DEBUG
                throw ex;
#endif

            }
            return null;
        }

        public static StylusPointCollection GetRectangleCorner(StylusPointCollection collection)
        {
            double maxX = double.MinValue, minX = double.MaxValue, maxY = double.MinValue, minY = double.MaxValue;
            foreach (StylusPoint point in collection)
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

            double borderLength = MathMethods.GetDistanceBetTowPoints(maxX, maxY, minX, minY);
            if (borderLength < minRectangleBorder)
            {
                return null;
            }


            double centerX = (maxX + minX) / 2;
            double centerY = (maxY + minY) / 2;
            double adjustWidht = (maxX - minX) / 3;
            double adjustHeight = (maxY - minY) / 3;

            List<StylusPoint> topPoints = new List<StylusPoint>();
            List<StylusPoint> bottomPoints = new List<StylusPoint>();
            List<StylusPoint> leftPoints = new List<StylusPoint>();
            List<StylusPoint> rightPoints = new List<StylusPoint>();

            foreach (var item in collection)
            {
                if (Math.Abs(item.X - centerX) > adjustWidht)
                {
                    if (item.X > centerX)
                    {
                        rightPoints.Add(item);
                    }
                    else
                    {
                        leftPoints.Add(item);
                    }

                }
                if (Math.Abs(item.Y - centerY) > adjustHeight)
                {
                    if (item.Y > centerY)
                    {
                        bottomPoints.Add(item);
                    }
                    else
                    {
                        topPoints.Add(item);
                    }
                }
            }

            double topAverage = 0.0;

            foreach (var point in topPoints)
            {
                topAverage += point.Y;

            }
            topAverage = topAverage / topPoints.Count;

            double bottomAverage = 0.0;
            foreach (var point in bottomPoints)
            {
                bottomAverage += point.Y;

            }
            bottomAverage = bottomAverage / bottomPoints.Count;

            double leftAverage = 0.0;
            foreach (var point in leftPoints)
            {
                leftAverage += point.X;

            }
            leftAverage = leftAverage / leftPoints.Count;

            double rightAvarage = 0.0;
            foreach (var point in rightPoints)
            {
                rightAvarage += point.X;

            }
            rightAvarage = rightAvarage / rightPoints.Count;
            StylusPointCollection corners = new StylusPointCollection();
            StylusPoint one = new StylusPoint(leftAverage, topAverage);
            StylusPoint two = new StylusPoint(rightAvarage, topAverage);
            StylusPoint three = new StylusPoint(rightAvarage, bottomAverage);
            StylusPoint four = new StylusPoint(leftAverage, bottomAverage);
            corners.Add(one);
            corners.Add(two);

            corners.Add(three);
            corners.Add(four);
            StylusPoint closedPoint = new StylusPoint(corners[0].X, corners[0].Y, 0.5f);
            corners.Add(closedPoint);
            return corners;

        }

        static double GetArea(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double pa = 0.5 * Math.Abs(x1 * y2 - y1 * x2 + x2 * y3 - y2 * x3 + x3 * y1 - y3 * x1);
            return pa;
        }

    }
}
