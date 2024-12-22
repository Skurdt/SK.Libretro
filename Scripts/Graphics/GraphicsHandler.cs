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

        private static readonly retro_video_refresh_t _refreshCallback = RefreshCallback;

        private readonly IGraphicsProcessor _processor;

        private retro_pixel_format _pixelFormat;
        private GraphicsFrameHandlerBase _frameHandler;

        private HardwareRenderProxy _hardwareRenderProxy;

        public GraphicsHandler(IGraphicsProcessor processor) => _processor = processor;

        public void Init(bool enabled)
        {
            Enabled = enabled;

            if (_hardwareRenderProxy is not null)
            {
                _frameHandler = _pixelFormat switch
                {
                    retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555 => new NullGraphicsFrameHandler(_processor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888 => new GraphicsFrameHandlerOpenGLXRGB8888VFlip(_processor, _hardwareRenderProxy),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_RGB565   => new NullGraphicsFrameHandler(_processor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN
                    or _ => new NullGraphicsFrameHandler(_processor)
                };
                return;
            }

            _frameHandler = _pixelFormat switch
            {
                retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555 => new GraphicsFrameHandlerSoftware0RGB1555(_processor),
                retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888 => new GraphicsFrameHandlerSoftwareXRGB8888(_processor),
                retro_pixel_format.RETRO_PIXEL_FORMAT_RGB565   => new GraphicsFrameHandlerSoftwareRGB565(_processor),
                retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN
                or _ => new NullGraphicsFrameHandler(_processor)
            };
        }

        public void Dispose()
        {
            _processor.Dispose();
            _hardwareRenderProxy?.Dispose();
        }

        public void PollEvents() => _hardwareRenderProxy?.PollEvents();

        public void SetCoreCallback(retro_set_video_refresh_t setVideoRefresh) => setVideoRefresh(_refreshCallback);

        public bool GetOverscan(IntPtr data)
        {
            if (data.IsNotNull())
                data.Write(Wrapper.Instance.Settings.CropOverscan);
            return Wrapper.Instance.Settings.CropOverscan;
        }

        public bool GetCanDupe(IntPtr data)
        {
            if (data.IsNotNull())
                data.Write(true);
            return true;
        }

        public bool GetCurrentSoftwareFramebuffer(IntPtr data)
        {
#if ENABLE_THIS
            if (data.IsNull())
                return false;

            retro_framebuffer framebuffer = data.ToStructure<retro_framebuffer>();

            uint width     = framebuffer.width;
            uint height    = framebuffer.height;
            IntPtr dataPtr = _processor.GetCurrentSoftwareFramebuffer((int)width, (int)height);
            if (dataPtr.IsNull())
                return false;

            framebuffer.data         = dataPtr;
            framebuffer.pitch        = width * 2;
            framebuffer.format       = retro_pixel_format.RETRO_PIXEL_FORMAT_RGB565;
            framebuffer.access_flags = (uint)RETRO_MEMORY_ACCESS.WRITE;
            framebuffer.memory_flags = RETRO.MEMORY_TYPE_CACHED;

            Marshal.StructureToPtr(framebuffer, data, true);
            _frameHandler = new GraphicsFrameHandlerSoftwareFramebuffer(_processor);
            return true;
#else
            return false;
#endif
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
            _pixelFormat = pixelFormat switch
            {
                0 or 1 or 2 => (retro_pixel_format)pixelFormat,
                _ => retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN
            };

            return _pixelFormat is not retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN;
        }

        public bool SetHwRender(IntPtr data)
        {
            if (Wrapper.Instance.Settings.Platform == Platform.Android)
                return false;

            if (data.IsNull() || _hardwareRenderProxy is not null)
                return false;

            retro_hw_render_callback hwRenderCallback = data.ToStructure<retro_hw_render_callback>();
            _hardwareRenderProxy = hwRenderCallback.context_type switch
            {
                retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL
                or retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE => new OpenGLRenderProxySDL(hwRenderCallback),

                retro_hw_context_type.RETRO_HW_CONTEXT_NONE
                or _ => default
            };

            if (_hardwareRenderProxy is null || !_hardwareRenderProxy.Init())
                return false;

            hwRenderCallback.get_current_framebuffer = _hardwareRenderProxy.GetCurrentFrameBuffer.GetFunctionPointer();
            hwRenderCallback.get_proc_address        = _hardwareRenderProxy.GetProcAddress.GetFunctionPointer();
            Marshal.StructureToPtr(hwRenderCallback, data, false);
            return true;
        }

        public bool SetGeometry(IntPtr data)
        {
            if (data.IsNull())
                return false;

            if (Wrapper.Instance.Game.SetGeometry(data.ToStructure<retro_game_geometry>()))
            {
                // TODO(Tom): Change width/height/aspect ratio
            }
            return true;
        }

        [MonoPInvokeCallback(typeof(retro_video_refresh_t))]
        private static void RefreshCallback(IntPtr data, uint width, uint height, nuint pitch)
        {
            if (!Wrapper.Instance.GraphicsHandler.Enabled)
                return;

            Wrapper.Instance.GraphicsHandler._frameHandler.ProcessFrame(data, width, height, pitch);
        }
    }
}
