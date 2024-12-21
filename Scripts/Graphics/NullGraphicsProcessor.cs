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

using System;
using System.Threading.Tasks;

namespace SK.Libretro
{
    internal sealed class NullGraphicsProcessor : IGraphicsProcessor
    {
        public IntPtr NativeWindow { get; }

        public ValueTask<IntPtr> GetCurrentSoftwareFramebuffer(int width, int height) => new(IntPtr.Zero);
        public void ProcessFrameSoftwareFramebuffer(IntPtr data, int pitch, int height) { }
        public void ProcessFrame0RGB1555(IntPtr data, int width, int height, int pitch) { }
        public void ProcessFrameXRGB8888(IntPtr data, int width, int height, int pitch) { }
        public void ProcessFrameXRGB8888VFlip(IntPtr data, int width, int height, int pitch) { }
        public void ProcessFrameRGB565(IntPtr data, int width, int height, int pitch) { }
        public void FinalizeFrame() { }
        public void Dispose() { }
    }
}
