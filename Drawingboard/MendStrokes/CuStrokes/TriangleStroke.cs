using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;

namespace Drawingboard.controls.MendStrokes
{
    class TriangleStroke : ShapeStroke
    {
        public TriangleStroke(StylusPointCollection pts)
            : base(pts)
        {
            this.ShapeKey = ShapeType.Triangle;
        }

    }
}
