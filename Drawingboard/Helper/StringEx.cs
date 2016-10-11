using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace SpringRoll.Components.Helpers
{
    public static class StringEx
    {
        /// <summary>
        /// make String to Color
        /// </summary>
        /// <param name="str">format : "#aarrggbb"</param>
        /// <returns></returns>
        public static Color ToColor(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Colors.Black;
            }

            Color clr = Color.FromArgb(str.Substring(1, 2).ToHex(), str.Substring(3, 2).ToHex(), str.Substring(5, 2).ToHex(), str.Substring(7, 2).ToHex());
            return clr;
        }

        private static byte ToHex(this string str)
        {
            return byte.Parse(str, System.Globalization.NumberStyles.HexNumber);
        }

        public static string ToJson(this Guid id)
        {
            return id.ToString("N");
        }
    }
}
