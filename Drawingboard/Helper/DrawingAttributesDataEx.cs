using Drawingboard.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace Drawingboard.Helpers
{
    public static class DrawingAttributesDataEx
    {
        public static DrawingAttributes ToDrawingAttributes(this DrawingAttributesData data)
        {
            DrawingAttributes att = new DrawingAttributes();
            att.Color = SpringRoll.Components.Helpers.StringEx.ToColor(data.Color);
            att.Width = data.Width;
            att.Height = data.Height;
            att.FitToCurve = true;
            att.IsHighlighter = data.IsHighlighter;

            return att;
        }

        public static DrawingAttributesData ToData(this DrawingAttributes att)
        {
            DrawingAttributesData data = new DrawingAttributesData();
            data.Color = att.Color.ToString();
            data.Width = att.Width;
            data.Height = att.Height;
            data.IsHighlighter = att.IsHighlighter;

            return data;
        }

        public static DrawingAttributesData Clone(this DrawingAttributesData src)
        {
            if (src == null)
            {
                return null;
            }

            DrawingAttributesData obj = new DrawingAttributesData();
            obj.Color = src.Color;
            obj.Width = src.Width;
            obj.Height = src.Height;
            obj.IsHighlighter = src.IsHighlighter;

            return obj;
        }
    }
}
