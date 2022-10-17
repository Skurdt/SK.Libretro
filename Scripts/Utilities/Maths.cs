/* MIT License

 * Copyright (c) 2021-2022 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using System.Runtime.CompilerServices;

namespace SK.Libretro
{
    internal static class Maths
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int val, int min, int max) => Max(min, Min(max, val));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float val, float min, float max) => Max(min, Min(max, val));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int val, int min) => (val < min) ? val : min;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float val, float min) => (float.IsNaN(val) || val < min) ? val : min;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int val, int max) => (val > max) ? val : max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float val, float max) => (float.IsNaN(val) || val > max) ? val : max;
    }

    internal static class MathsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(this int val, int min, int max) => Maths.Clamp(val, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(this int val, int min) => Maths.Min(val, min);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(this int val, int max) => Maths.Max(val, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this float val, float min, float max) => Maths.Clamp(val, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(this float val, float min) => Maths.Min(val, min);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(this float val, float max) => Maths.Max(val, max);
    }
}
