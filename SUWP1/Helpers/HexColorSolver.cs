using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace SUWP1.Helpers
{
    static class HexColorSolver
    {
        public static Color GetColorFromHex(string hexString)
        {
            hexString = hexString.Replace("#", string.Empty);
            if (hexString.Length < 6) hexString += "0";
            var r = byte.Parse(hexString.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hexString.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hexString.Substring(4, 2), NumberStyles.HexNumber);
            return Color.FromArgb(byte.MaxValue, r, g, b);
        }

    }
}
