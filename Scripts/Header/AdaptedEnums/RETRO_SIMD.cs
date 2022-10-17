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

namespace SK.Libretro.Header
{
    internal enum RETRO_SIMD
    {
        SSE    = 1 << 0,
        SSE2   = 1 << 1,
        VMX    = 1 << 2,
        VMX128 = 1 << 3,
        AVX    = 1 << 4,
        NEON   = 1 << 5,
        SSE3   = 1 << 6,
        SSSE3  = 1 << 7,
        MMX    = 1 << 8,
        MMXEXT = 1 << 9,
        SSE4   = 1 << 10,
        SSE42  = 1 << 11,
        AVX2   = 1 << 12,
        VFPU   = 1 << 13,
        PS     = 1 << 14,
        AES    = 1 << 15,
        VFPV3  = 1 << 16,
        VFPV4  = 1 << 17,
        POPCNT = 1 << 18,
        MOVBE  = 1 << 19,
        CMOV   = 1 << 20,
        ASIMD  = 1 << 21
    }
}
