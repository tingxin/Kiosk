using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drawingboard.controls.MendStrokes
{
    public class ShapeType
    {
        //support type
        public const string Square = "Square";
        public const string Rectangle = "Rectangle";
        public const string Circle = "Circle";
        public const string Triangle = "Triangle";
        public const string Straight = "Straight";
        public const string Arrow = "Arrow";


        public static bool IsSupportShape(string shapeKey)
        {
            if (shapeKey.Contains(Square) || shapeKey.Contains(Rectangle) || shapeKey.Contains(Circle) || shapeKey.Contains(Triangle))
            {
                return true;
            }
            return false;
        }

    }
}
