using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Configuration;

namespace KComponents
{
    public static class SystemHelper
    {
        #region Screen
        public static bool IsTouchWall { get; private set; }
        public static Screen MainScreen { get; private set; }
        public static Screen SecondScreen { get; private set; }
        public static double DpiScaleRate { get; private set; }
        public static double DPI { get; private set; }
        
        static SystemHelper()
        {
            IsTouchWall = DetectTouchWall();
            MainScreen = DetectMainScreen();
            SecondScreen = DetectSecondScreen();

            DpiScaleRate = (double)Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;
            DPI = 96.0 * DpiScaleRate;
        }

        public static double PhysicalPixelToLogicUnit(int physicalPixel)
        {
            return (double)physicalPixel / DpiScaleRate;
        }

        private static Screen DetectMainScreen()
        {
            if (Screen.AllScreens.Count() >= 2)
            {
                foreach (Screen s in Screen.AllScreens)
                {
                    if (s.Bounds.Width >= 3840 && s.Bounds.Height >= 160)
                    {
                        return s;
                    }
                }

                return Screen.AllScreens[0];
            }
            else
            {
                return Screen.AllScreens[0];
            }
        }

        private static Screen DetectSecondScreen()
        {
            foreach (Screen s in Screen.AllScreens)
            {
                if (s != MainScreen)
                {
                    return s;
                }
            }

            return null;
        }

        private static bool DetectTouchWall()
        {
            if (Screen.AllScreens.Count() >= 2)
            {
                foreach (Screen s in Screen.AllScreens)
                {
                    if (s.Bounds.Width >= 3840 && s.Bounds.Height >= 2160)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
    }
}
