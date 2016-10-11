using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Ink;
using System.Windows.Media;

namespace KComponents
{
    public class DecorateStroke : Stroke
    {
        public DecorateStroke(StylusPointCollection stylusPoints)
            : base(stylusPoints)
        {

        }

        public DecorateStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes)
            : base(stylusPoints, drawingAttributes)
        {

        }

        protected override void DrawCore(System.Windows.Media.DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            DrawingAttributes fat = drawingAttributes.Clone();
            fat.Color = Colors.Black;// Color.FromArgb(255, 81, 255, 255);
            fat.Width += 1;
            fat.Height += 1;
            base.DrawCore(drawingContext, fat);

            base.DrawCore(drawingContext, drawingAttributes);
        }

        public override Stroke Clone()
        {
            DecorateStroke obj = new DecorateStroke(this.StylusPoints.Clone(), this.DrawingAttributes.Clone());
            foreach (var i in GetPropertyDataIds())
            {
                obj.AddPropertyData(i, GetPropertyData(i));
            }
            return obj;
        }
    }
}
