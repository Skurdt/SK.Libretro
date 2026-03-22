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

namespace SK.Libretro
{
    internal sealed class GraphicsFrameHandlerOpenGLXRGB8888 : GraphicsFrameHandlerBase
    {
        private readonly HardwareRenderProxy _hardwareRenderProxy;

        private byte[] _bufferSrc = Array.Empty<byte>();

        public GraphicsFrameHandlerOpenGLXRGB8888(IGraphicsProcessor processor, HardwareRenderProxy hardwareRenderProxy)
        : base(processor)
            => _hardwareRenderProxy = hardwareRenderProxy;

        public override void ProcessFrame(IntPtr _, uint width, uint height, nuint pitch)
        {
            if (_hardwareRenderProxy is null)
                return;

            var bufferSize = (int)(width * height * 4);
            if (_bufferSrc.Length != bufferSize)
                _bufferSrc = new byte[bufferSize];

            if (!_hardwareRenderProxy.ReadbackFrame(width, height, ref _bufferSrc))
                return;

            unsafe
            {
                fixed (byte* bufferPtr = _bufferSrc)
                    _processor.ProcessFrameXRGB8888((IntPtr)bufferPtr, (int)width, (int)height, (int)width * 4);
            }
        }
    }
}
