using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace Drawingboard.controls.MendStrokes
{
    public class ShapeStroke : Stroke
    {
        public string ShapeKey { get; protected set; }
        public ShapeStroke(StylusPointCollection pts)
            : base(pts)
        {
            this.StylusPoints = pts;
        }

    }
}
