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
using static SK.Libretro.Header.RETRO;

namespace SK.Libretro
{
    internal sealed class Graphics
    {
        public readonly retro_video_refresh_t Callback;

        public bool Enabled { get; set; }
        public bool UseCoreRotation { get; set; }
        public retro_pixel_format PixelFormat { get; set; }

        private GraphicsFrameHandlerBase _frameHandler;

        public unsafe Graphics(bool useCoreRotation)
        {
            Callback        = CallbackCall;
            UseCoreRotation = useCoreRotation;
        }

        public void Init(GraphicsFrameHandlerBase frameHandler) =>
            _frameHandler = frameHandler;

        public void DeInit() =>
            _frameHandler?.DeInit();

        public unsafe void CallbackCall(IntPtr data, uint width, uint height, nuint pitch)
        {
            if (data == null || !Enabled)
                return;
            _frameHandler.ProcessFrame(data, width, height, pitch);
        }
    }
}
