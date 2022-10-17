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

namespace SK.Libretro.Unity
{
    internal static class GraphicsUtilities
    {
        public static uint ARGB1555toBGRA32(ushort packed)
        {
            uint a = (uint)packed & 0x8000;
            uint r = (uint)packed & 0x7C00;
            uint g = (uint)packed & 0x03E0;
            uint b = (uint)packed & 0x1F;
            uint rgb = (r << 9) | (g << 6) | (b << 3);
            return (a * 0x1FE00) | rgb | ((rgb >> 5) & 0x070707);
        }

        public static uint RGB565toBGRA32(ushort packed)
        {
            uint r = ((uint)packed >> 11) & 0x1f;
            uint g = ((uint)packed >> 5) & 0x3f;
            uint b = ((uint)packed >> 0) & 0x1f;
            r = (r << 3) | (r >> 2);
            g = (g << 2) | (g >> 4);
            b = (b << 3) | (b >> 2);
            return (0xffu << 24) | (r << 16) | (g << 8) | (b << 0);
        }
    }
}
