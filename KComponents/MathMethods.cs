using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace KComponents
{
    public static class MathMethods
    {
        public static double GetDistanceBetTowPoints(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(GetPowDistanceBetTowPoints(x1, y1, x2, y2));
        }

        public static double GetPowDistanceBetTowPoints(double x1, double y1, double x2, double y2)
        {
            return Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2); ;
        }

        public static double GetDistanceBetPointToLine(double x1, double y1, double A, double B, double C)
        {
            double fenzi = Math.Abs(A * x1 + B * y1 + C);
            double fenmu = Math.Sqrt(A * A + B * B);
            return fenzi / fenmu;
        }

        public static double GetDistanceBetPointToLine(Point lineStartPoint, Point lineEndPoint, Point targetPoint)
        {
            double gradient = (lineStartPoint.X - lineEndPoint.X) / (lineStartPoint.Y - lineEndPoint.Y);
            double constParamater = 0 - lineEndPoint.X + gradient * lineEndPoint.X;
            return GetDistanceBetPointToLine(targetPoint.X, targetPoint.Y, 1, 0 - gradient, constParamater);
        }

        public static double GetArea(this IEnumerable<Point> points)
        {
            double area = 0;
            if (points.Count() > 3)
            {
                for (int i = 0; i < points.Count(); i++)
                {
                    Point current = points.ElementAt(i);
                    Point next = current;
                    if (i + 1 >= points.Count())
                    {
                        next = points.ElementAt(0);
                    }
                    else
                    {
                        next = points.ElementAt(i + 1);
                    }
                    double currentArea = current.X * next.Y - current.Y * next.X;
                    area += currentArea;
                }
            }
            return Math.Abs(area / 2);
        }

        public static double GetMaxDistance(this IEnumerable<Point> points)
        {
            double result = 0;
            int count = points.Count();
            if (count >= 2)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        var v = points.ElementAt(i) - points.ElementAt(j);
                        double distance = Math.Abs(v.Length);
                        if (distance > result)
                        {
                            result = distance;
                        }
                    }
                }
            }
            return result;
        }

        public static double GetMaxNearBySortDistance(this List<Point> points)
        {
            points.Sort();
            double result = 0;
            int count = points.Count();
            if (count >= 2)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    var v = points.ElementAt(i) - points.ElementAt(i + 1);
                    double distance = Math.Abs(v.Length);
                    if (distance > result)
                    {
                        result = distance;
                    }

                }
            }
            return result;
        }

        public static double GetMaxNearByDistance(this IEnumerable<Point> points)
        {
            double result = 0;
            int count = points.Count();
            if (count >= 2)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    var v = points.ElementAt(i) - points.ElementAt(i + 1);
                    double distance = Math.Abs(v.Length);
                    if (distance > result)
                    {
                        result = distance;
                    }

                }
                var nv = points.ElementAt(0) - points.ElementAt(points.Count() - 1);
                double ndistance = Math.Abs(nv.Length);
                if (ndistance > result)
                {
                    result = ndistance;
                }
            }
            return result;
        }


        public static double GetMaxDistanceTwoPoints(this IEnumerable<Point> points, Point start, Point end)
        {
            double result = 0;
            int count = points.Count();
            if (count >= 2)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        var v = points.ElementAt(i) - points.ElementAt(j);
                        double distance = Math.Abs(v.Length);
                        if (distance > result)
                        {
                            start = points.ElementAt(i);
                            end = points.ElementAt(j);
                            result = distance;
                        }
                    }
                }
            }
            return result;
        }

        public static void GetMaxNearByTwoPoints(this IEnumerable<Point> points, Point start, Point end)
        {
            int count = points.Count();
            if (count >= 2)
            {
                double maxDis = 0;

                for (int i = 0; i < count - 1; i++)
                {
                    var v = points.ElementAt(i) - points.ElementAt(i + 1);
                    double distance = Math.Abs(v.Length);
                    if (distance > maxDis)
                    {
                        maxDis = distance;
                        start = points.ElementAt(i);
                        end = points.ElementAt(i + 1);
                    }

                }
                var nv = points.ElementAt(0) - points.ElementAt(count - 1);
                double ndistance = Math.Abs(nv.Length);
                if (ndistance > maxDis)
                {
                    maxDis = ndistance;
                    start = points.ElementAt(0);
                    end = points.ElementAt(count - 1);
                }
            }
        }

        public static double GetExceptionPoint(this IEnumerable<Point> input, out Point exceptionPoint)
        {
            int count = input.Count();
            Dictionary<int, double> keys = new Dictionary<int, double>();
            for (int i = 0; i < count; i++)
            {
                double avg = 0;
                for (int j = 0; j < count; j++)
                {
                    double distance = MathMethods.GetDistanceBetTowPoints(input.ElementAt(i).X, input.ElementAt(i).Y, input.ElementAt(j).X, input.ElementAt(j).X);
                    avg += distance;
                }
                avg = avg / count;
                keys.Add(i, avg);
            }
            double max = 0;
            int target = -1;
            foreach (var item in keys)
            {
                if (item.Value > max)
                {
                    max = item.Value;
                    target = item.Key;
                }
            }
            exceptionPoint = input.ElementAt(target);
            return keys[target];
        }

    }
}
