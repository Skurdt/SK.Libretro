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

using SK.Libretro.Header;
using System;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class GraphicsHandler : IDisposable
    {
        public bool Enabled { get; set; }
        public retro_pixel_format PixelFormat { get; private set; }
        public OpenGLHelperWindow OpenGLHelperWindow { get; private set; }

        private readonly Wrapper _wrapper;
        private readonly retro_video_refresh_t _refreshCallback;

        private GraphicsFrameHandlerBase _frameHandler;
        private retro_hw_render_callback _hwRenderInterface;

        public GraphicsHandler(Wrapper wrapper) =>
            (_wrapper, _refreshCallback) = (wrapper, RefreshCallback);

        public void Init(GraphicsFrameHandlerBase frameHandler, bool enabled)
        {
            _frameHandler = frameHandler;
            Enabled       = enabled;
        }

        public void Dispose()
        {
            _frameHandler?.Dispose();
            OpenGLHelperWindow?.Dispose();
        }

        public void SetCoreCallback(retro_set_video_refresh_t setVideoRefresh) => setVideoRefresh(_refreshCallback);

        public void InitHardwareContext() =>_hwRenderInterface.context_reset.GetDelegate<retro_hw_context_reset_t>().Invoke();

        public bool GetOverscan(IntPtr data)
        {
            if (data.IsNotNull())
                data.Write(_wrapper.Settings.CropOverscan);
            return _wrapper.Settings.CropOverscan;
        }

        public bool GetCanDupe(IntPtr data)
        {
            if (data.IsNotNull())
                data.Write(true);
            return true;
        }

        public bool GetPreferredHwRender(IntPtr data)
        {
            if (data.IsNull())
                return false;
            
            data.Write((uint)retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE);
            return true;
        }

        public bool SetPixelFormat(IntPtr data)
        {
            if (data.IsNull())
                return false;

            int pixelFormat = data.ReadInt32();
            PixelFormat = pixelFormat switch
            {
                0 or 1 or 2 => (retro_pixel_format)pixelFormat,
                _ => retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN
            };

            return PixelFormat != retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN;
        }

        public bool SetHwRender(IntPtr data)
        {
            if (data.IsNull() || _wrapper.Core.HwAccelerated)
                return false;

            retro_hw_render_callback callback = data.ToStructure<retro_hw_render_callback>();
            if (callback.context_type is not retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL and not retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE)
                return false;

            OpenGLHelperWindow = new();
            if (!OpenGLHelperWindow.Init())
                return false;

            callback.get_current_framebuffer = OpenGLHelperWindow.GetCurrentFrameBuffer.GetFunctionPointer();
            callback.get_proc_address = OpenGLHelperWindow.GetProcAddress.GetFunctionPointer();

            _hwRenderInterface = callback;
            Marshal.StructureToPtr(_hwRenderInterface, data, true);

            _wrapper.Core.HwAccelerated = true;
            return true;
        }

        public bool SetGeometry(IntPtr data)
        {
            if (data.IsNull())
                return false;

            if (_wrapper.Game.SetGeometry(data.ToStructure<retro_game_geometry>()))
            {
                // TODO(Tom): Change width/height/aspect ratio
            }
            return true;
        }

        private unsafe void RefreshCallback(IntPtr data, uint width, uint height, nuint pitch)
        {
            if (Enabled)
                _frameHandler.ProcessFrame(data, width, height, pitch);
        }
    }
}
