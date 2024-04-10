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

using Unity.Mathematics;
using UnityEngine;

namespace SK.Libretro.Unity
{
    public static class CSExtensions
    {
        public static (short, short) ToShort(this Vector2 vec, int mul = 1) => (vec.x.ToShort(mul), vec.y.ToShort(mul));
        public static short ToShort(this float floatValue, int mul = 1) => (short)(math.clamp(math.round(floatValue), short.MinValue, short.MaxValue) * mul);

        public static void SetBit(this ref uint bits, in uint bit) => bits |= 1u << (int)bit;
        public static void SetBit(this ref uint bits, in uint bit, in bool enable)
        {
            if (enable)
                bits.SetBit(bit);
            else
                bits.UnsetBit(bit);
        }
        public static void SetBitIf(this ref uint bits, in uint bit, in bool cond)
        {
            if (cond)
                bits |= 1u << (int)bit;
        }
        public static void UnsetBit(this ref uint bits, in uint bit) => bits &= ~(1u << (int)bit);
        public static void ToggleBit(this ref uint bits, in uint bit) => bits ^= 1u << (int)bit;
        public static bool IsBitSet(this uint bits, in uint bit) => (bits & (int)bit) != 0;
        public static short IsBitSetAsShort(this uint bits, in uint bit) => (short)((bits >> (int)bit) & 1);
    }
}
