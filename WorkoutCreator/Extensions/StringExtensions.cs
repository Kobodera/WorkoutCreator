using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkoutCreator.Extensions
{
    public static class StringExtensions
    {
        public static int ToInt(this string str)
        {
            int result = 0;
            if (int.TryParse(str, out result))
                return result;

            return 0;
        }

        public static double ToDouble(this string str)
        {
            return ToDouble(str, 0);
        }

        public static double ToDouble(this string str, double defaultValue)
        {
            double result = 1.1;

            string temp = result.ToString().Substring(1, 1);

            if (double.TryParse(str.Replace(".", temp).Replace(",", temp), out result))
                return result;

            return defaultValue;
        }

        public static float ToFloat(this string str)
        {
            return ToFloat(str, 0);
        }

        public static float ToFloat(this string str, float defaultValue)
        {
            float result = 1.1F;

            string temp = result.ToString().Substring(1, 1);

            if (float.TryParse(str.Replace(".", temp).Replace(",", temp), out result))
                return result;

            return defaultValue;
        }
    }
}
