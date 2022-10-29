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
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class GraphicsFrameHandlerOpenGLXRGB8888VFlip : GraphicsFrameHandlerBase
    {
        private readonly HardwareRenderHelperWindow _hardwareRenderHelperWindow;

        public GraphicsFrameHandlerOpenGLXRGB8888VFlip(IGraphicsProcessor processor, HardwareRenderHelperWindow hardwareRenderHelperWindow)
        : base(processor)
        {
            _hardwareRenderHelperWindow = hardwareRenderHelperWindow;
            _hardwareRenderHelperWindow?.InitContext();
        }

        public override void ProcessFrame(IntPtr _, uint width, uint height, nuint pitch)
        {
            int bufferSize   = (int)(width * height * 4);
            byte[] bufferSrc = new byte[bufferSize];
            GCHandle handle  = GCHandle.Alloc(bufferSrc, GCHandleType.Pinned);
            IntPtr data      = Marshal.UnsafeAddrOfPinnedArrayElement(bufferSrc, 0);
            GL.ReadPixels(0, 0, (int)width, (int)height, GL.BGRA, GL.UNSIGNED_BYTE, data);
            _processor.ProcessFrameXRGB8888VFlip(data, (int)width, (int)height, (int)width * 4);
            _hardwareRenderHelperWindow.SwapBuffers();
            handle.Free();
        }
    }
}
