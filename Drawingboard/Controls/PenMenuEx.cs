using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Drawingboard.controls
{
    static class PenMenuEx
    {
        public static void MoveMenuCenter(this FrameworkElement ctrl, Point position)
        {
            if (ctrl != null)
            {
                TranslateTransform tran = ctrl.RenderTransform as TranslateTransform;
                if (tran != null)
                {
                    tran.X = position.X - ctrl.Width / 2;
                    tran.Y = position.Y - ctrl.Width / 2;
                }
            }
        }

        public static Point GetMenuCenterPosition(this FrameworkElement ctrl)
        {
            Point pos = new Point();
            if (ctrl != null)
            {
                TranslateTransform tran = ctrl.RenderTransform as TranslateTransform;
                if (tran != null)
                {
                    pos.X = tran.X + ctrl.Width / 2;
                    pos.Y = tran.Y + ctrl.Width / 2;
                }
            }

            return pos;
        }
    }
}
