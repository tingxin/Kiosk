using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;

namespace Drawingboard.controls.MendStrokes
{
    class GetAdjustShapeStroke
    {
        public static ShapeStroke Get(StylusPointCollection input, string strType, DrawingAttributes attributes)
        {
            ShapeStroke result = null;

            if (strType == ShapeType.Circle && input.Count > 5)
            {
                result = new CircleStroke(input);

            }
            else if (strType == ShapeType.Rectangle || strType == ShapeType.Square)
            {
                StylusPointCollection points = ShapeCornerFinder.GetRectangleCorner(input);
                if (points != null && points.Count == 5)
                {
                    result = new RectangleStroke(points);
                }
            }
            else if (strType.Contains(ShapeType.Triangle))
            {
                StylusPointCollection points = ShapeCornerFinder.GetTriangleCorner(input);
                if (points != null && points.Count == 4)
                {
                    result = new TriangleStroke(points);
                }
            }
            else
            {
                //if line
                StylusPointCollection points = ShapeCornerFinder.GetLineCorner(input);
                if (points != null && points.Count == 2)
                {
                    result = new StraightStroke(points);

                }

            }
            if (result != null)
            {
                result.DrawingAttributes = attributes;
            }

            return result;
        }
    }
}
