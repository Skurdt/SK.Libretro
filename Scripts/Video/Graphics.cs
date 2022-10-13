/* MIT License

 * Copyright (c) 2022 Skurdt
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

using SK.Libretro.Header;
using System;

namespace SK.Libretro
{
    internal sealed class Graphics : IDisposable
    {
        public readonly retro_video_refresh_t Callback;

        public bool Enabled { get; set; }
        public retro_pixel_format PixelFormat { get; private set; }

        private GraphicsFrameHandlerBase _frameHandler;

        public Graphics() => Callback = CallbackCall;

        public void Init(GraphicsFrameHandlerBase frameHandler, bool enabled)
        {
            _frameHandler = frameHandler;
            Enabled = enabled;
        }

        public void Dispose() => _frameHandler.Dispose();

        public void SetPixelFormat(int pixelFormat) =>
            PixelFormat = pixelFormat switch
            {
                0 or 1 or 2 => (retro_pixel_format)pixelFormat,
                _ => retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN
            };

        private unsafe void CallbackCall(IntPtr data, uint width, uint height, nuint pitch)
        {
            if (data.IsNotNull() && Enabled)
                _frameHandler.ProcessFrame(data, width, height, pitch);
        }
    }
}
