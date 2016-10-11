using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Diagnostics;

namespace Drawingboard.controls
{
    public delegate void PenMenuCommandFire(object sender, PenMenuItemChangedArgs ex);

    public delegate void LassoSelectedCompeletd(object sender);

    public class PenMenuItemChangedArgs : EventArgs
    {

        public PenMenuCommandType CommandType { private set; get; }
        public PenMenuCommand Command { private set; get; }
        public PenArgument PenDetail { private set; get; }


        public PenMenuItemChangedArgs(PenMenuCommandType type, PenMenuCommand command)
        {
            this.CommandType = type;
            this.Command = command;

            if (CommandType != PenMenuCommandType.Tools)
            {
                string colorString = PenColorHelper.GetColorStr(command);
                Debug.Assert(!string.IsNullOrEmpty(colorString), "笔触颜色字典不包含对应的颜色，请添加");


                if (CommandType == PenMenuCommandType.Highlight)
                {
                    PenDetail = new PenArgument(7.5, 0.5, (Color)ColorConverter.ConvertFromString(colorString));
                }
                else
                {
                    PenDetail = new PenArgument(3.5, 1.0, (Color)ColorConverter.ConvertFromString(colorString));
                }
            }
            else
            {
                PenDetail = null;
            }

        }
    }

    public class LassoSelectedCompeletdArgs : EventArgs
    {

    }

    public enum PenMenuCommand
    {
        FreeHandPen = 13,
        SmoothlyPen = 12,
        LassoSelected = 11,
        Eraser = 10,

        MarkerWhite = 9,
        MarkerRed = 8,
        MarkerPurple = 7,
        MarkerYellow = 6,
        MarkerCyan = 5,

        HighlightRed = 4,
        HighlightPurple = 3,
        HightCyan = 2,
        HightYellow = 1,
        Animation = 14,
        Close = 16,
        Panning = 15,
    }

    public enum PenMenuCommandType
    {
        UnKnow = 0,
        Marker = 1,
        Highlight = 2,
        Tools = 3,
    }

    public class PenArgument
    {
        public double PenThickness { private set; get; }
        public double PenOpacity { private set; get; }
        public Color PenColor { private set; get; }

        public PenArgument(double thickness, double opacity, Color color)
        {
            this.PenThickness = thickness;
            this.PenOpacity = opacity;
            this.PenColor = color;
        }
    }

    public static class PenColorHelper
    {
        static Dictionary<PenMenuCommand, string> colorValue;

        static PenColorHelper()
        {
            colorValue = new Dictionary<PenMenuCommand, string>();
            colorValue.Add(PenMenuCommand.HighlightPurple, "#e85cff");
            colorValue.Add(PenMenuCommand.HighlightRed, "#ff350b");
            colorValue.Add(PenMenuCommand.HightCyan, "#17b1f6");
            colorValue.Add(PenMenuCommand.HightYellow, "#fffb0b");

            colorValue.Add(PenMenuCommand.MarkerWhite, "#b3b3b3");
            colorValue.Add(PenMenuCommand.MarkerRed, "#ff0000");
            colorValue.Add(PenMenuCommand.MarkerPurple, "#b265cd");
            colorValue.Add(PenMenuCommand.MarkerCyan, "#17b1f6");
            colorValue.Add(PenMenuCommand.MarkerYellow, "#fbff0b");
        }

        public static string GetColorStr(PenMenuCommand command)
        {

            if (colorValue.ContainsKey(command))
            {
                return colorValue[command];
            }
            else
            {
                return string.Empty;
            }
        }

        public static PenMenuCommand GetCommandByStr(string colorStr)
        {
            if (colorValue != null)
            {
                bool isExist = colorValue.Any(item => item.Value == colorStr);
                if (isExist)
                {
                    var result = colorValue.FirstOrDefault(item => item.Value == colorStr);
                    return result.Key;
                }

            }
            return PenMenuCommand.FreeHandPen;
        }
    }
}
