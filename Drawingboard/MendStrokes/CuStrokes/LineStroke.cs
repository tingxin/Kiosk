using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Ink;
using System.Windows.Media;

namespace Drawingboard.controls.MendStrokes
{
    class StraightStroke : ShapeStroke
    {
        public StraightStroke(StylusPointCollection pts)
            : base(pts)
        {
            this.ShapeKey = ShapeType.Straight;
        }

    }
}
