using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IgnoreSolutions.PsychSodoku.Extensions
{
    public static class Extensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while(n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string PadNumbers(string input)
        {
            return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }

        public static int Index2DTo1D(int x, int y, int width)
        {
            return (x + (y * width));
        }

        public static int[] Index1DTo2D(int input, int width)
        {
            return new int[] { input / width, input % width };
        }
    }
}