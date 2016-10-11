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
    class ArrowStroke : ShapeStroke
    {

        public ArrowStroke(StylusPointCollection pts)
            : base(pts)
        {
            this.ShapeKey = ShapeType.Arrow;
        }
    }
}
