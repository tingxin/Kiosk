using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drawingboard.controls
{
    internal static class DrawingBoardParam
    {
        public static readonly double MarkerPenThiness = 4;
        public static readonly double HighlighterPenThiness = 30;
        public static readonly double EraserWidth = 60;
        public static readonly double EraserHeight = 60;
        public static readonly double EraserMinWidth = 60;
        public static readonly double EraserMinHeight = 100;
        public static readonly double EraserMaxWidth = 300;
        public static readonly double EraserMaxHeight = 500;
        //
        internal const double MinScaleRate = 0.25;
        internal const double MaxScaleRate = 2.0;
        internal const double MinExtraScaleRate = 0.20;
        internal const double MaxExtraScaleRate = 2.5;
        internal const double MinValueToScalePercent100 = 0.91;
        internal const double MaxValueToScalePercent100 = 1.10;
        //
        internal const double MapScaleRate = 10;
    }
}
