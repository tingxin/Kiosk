using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace KComponents
{
    public class EraserRecognizer
    {
        static bool isInTouchWall = false;
        static EraserRecognizer()
        {
            isInTouchWall = true;
        }

        public static bool Recoginze(Size size)
        {
            double height = GestureDetector.GetMendedHeight(size.Width, size.Height);
            return Recoginze(size.Width, height);
        }

        public static bool Recoginze(double width, double height)
        {
            double ratio = width / height;
            double size = width * height;
            //double minSize = WhiteBoardParam.EraserMinWidth * WhiteBoardParam.EraserMinHeight;
            //double maxSize = WhiteBoardParam.EraserMaxWidth * WhiteBoardParam.EraserMaxHeight;
            if (isInTouchWall)
            {
                if (GestureDetector.IsErase(width, height))
                {
                    return true;
                }
            }
            else
            {
                if (size > 6000)
                {
                    return true;
                }

                if (size > 2000)
                {
                    if (ratio < 0.4 || ratio > 2.5)
                    {
                        return true;
                    }
                }
                else if (size > 1000)
                {

                    return ratio < 0.45 || ratio > 2.25;
                }
                else if (size > 800)
                {
                    return ratio < 0.5 || ratio > 2;
                }
            }
            return false;
        }


    }
}
