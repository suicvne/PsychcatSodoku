using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku.Extensions
{
    public static class Extensions
    {
        private static System.Random rng = new System.Random();

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

        public static void FadeOutCanvasGroup(this CanvasGroup cg)
        {

        }

        public static void FadeInCanvasGroup()
        {

        }

        private static IEnumerator Fade(CanvasGroup cg, bool inOut)
        {
            for(float f = 0;
                f <= 1.0f;
                f += 1.0f * UnityEngine.Time.deltaTime)
            {

                yield return null;
            }

            yield return null;
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