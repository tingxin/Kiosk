using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawingboard.controls.MendStrokes;

namespace Drawingboard.Helper
{
    interface IShapeHost
    {
        void Add(ShapeStroke shape);
    }
}
