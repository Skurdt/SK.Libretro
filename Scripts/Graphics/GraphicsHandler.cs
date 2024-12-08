﻿/* MIT License

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
using System.Threading;

namespace SK.Libretro
{
    internal sealed class GraphicsHandler : IDisposable
    {
        public bool Enabled { get; set; }

        private static readonly retro_video_refresh_t _refreshCallback = RefreshCallback;

        private readonly Wrapper _wrapper;
        private readonly IGraphicsProcessor _processor;

        private retro_pixel_format _pixelFormat;
        private GraphicsFrameHandlerBase _frameHandler;

        private HardwareRenderHelperWindow _hardwareRenderHelperWindow;

        public GraphicsHandler(Wrapper wrapper, IGraphicsProcessor processor)
        {
            _wrapper   = wrapper;
            _processor = processor;
        }

        public void Init(bool enabled)
        {
            Enabled = enabled;

            if (_hardwareRenderHelperWindow is not null)
            {
                _frameHandler = _pixelFormat switch
                {
                    retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555 => new NullGraphicsFrameHandler(_processor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888 => new GraphicsFrameHandlerOpenGLXRGB8888VFlip(_wrapper, _processor, _hardwareRenderHelperWindow),
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
            _hardwareRenderHelperWindow?.Dispose();
        }

        public void PollEvents() => _hardwareRenderHelperWindow?.PollEvents();

        public void SetCoreCallback(retro_set_video_refresh_t setVideoRefresh) => setVideoRefresh(_refreshCallback);

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
            if (data.IsNull() || _hardwareRenderHelperWindow is not null)
                return false;

            retro_hw_render_callback hwRenderCallback = data.ToStructure<retro_hw_render_callback>();
            _hardwareRenderHelperWindow = hwRenderCallback.context_type switch
            {
                retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL
                or retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE => new OpenGLHelperWindowSDL(hwRenderCallback),

                retro_hw_context_type.RETRO_HW_CONTEXT_NONE
                or _ => default
            };

            if (_hardwareRenderHelperWindow is null || !_hardwareRenderHelperWindow.Init())
                return false;

            hwRenderCallback.get_current_framebuffer = _hardwareRenderHelperWindow.GetCurrentFrameBuffer.GetFunctionPointer();
            hwRenderCallback.get_proc_address        = _hardwareRenderHelperWindow.GetProcAddress.GetFunctionPointer();
            Marshal.StructureToPtr(hwRenderCallback, data, true);
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

        [MonoPInvokeCallback(typeof(retro_video_refresh_t))]
        private static void RefreshCallback(IntPtr data, uint width, uint height, nuint pitch)
        {
            if (!Wrapper.TryGetInstance(Thread.CurrentThread, out Wrapper wrapper))
                return;

            if (wrapper.GraphicsHandler.Enabled)
                wrapper.GraphicsHandler._frameHandler.ProcessFrame(data, width, height, pitch);
        }
    }
}
