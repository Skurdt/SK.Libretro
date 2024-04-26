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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace SK.Libretro
{
    internal sealed class GraphicsHandler : IDisposable
    {
        private delegate void CallbackDelegate(IntPtr data, uint width, uint height, nuint pitch, int instanceId);

        public bool Enabled { get; set; }

        private readonly Wrapper _wrapper;
        private readonly IGraphicsProcessor _processor;
        private readonly retro_video_refresh_t _refreshCallback;

        private retro_pixel_format _pixelFormat;
        private GraphicsFrameHandlerBase _frameHandler;

        private HardwareRenderHelperWindow _hardwareRenderHelperWindow;

        private Thread _thread;

        public GraphicsHandler(Wrapper wrapper, IGraphicsProcessor processor)
        {
            _wrapper         = wrapper;
            _processor       = processor;
            _refreshCallback = RefreshCallback;
            _thread          = Thread.CurrentThread;
            lock (instancePerHandle)
            {
                instancePerHandle.Add(_thread, this);
            }
        }

        ~GraphicsHandler()
        {
            lock (instancePerHandle)
            {
                instancePerHandle.Remove(_thread);
            }
        }

        // IL2CPP does not support marshaling delegates that point to instance methods to native code.
        // Using static method and per instance table.
        private static Dictionary<Thread, GraphicsHandler> instancePerHandle = new Dictionary<Thread, GraphicsHandler>();
        
        private static GraphicsHandler GetInstance(Thread thread)
        {
            GraphicsHandler instance;
            bool ok;
            lock (instancePerHandle)
            {
                ok = instancePerHandle.TryGetValue(thread, out instance);
            }
            return ok ? instance : null;
        }

        public void Init(bool enabled)
        {
            Enabled = enabled;

            if (_wrapper.Core.HwAccelerated)
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

        public void SetCoreCallback(retro_set_video_refresh_t setVideoRefresh) => setVideoRefresh(NativeGraphicsCallback);

        public class MonoPInvokeCallbackAttribute : System.Attribute
        {
            private Type type;
            public MonoPInvokeCallbackAttribute(Type t) { type = t; }
        }

        [MonoPInvokeCallbackAttribute(typeof(CallbackDelegate))]
        private static void NativeGraphicsCallback(IntPtr data, uint width, uint height, nuint pitch)
        {
            GraphicsHandler instance = GetInstance(Thread.CurrentThread);
            if (instance == null)
                return;
            instance._refreshCallback(data, width, height, pitch);
        }

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

        public bool GetCurrentSoftwareFramebuffer() => false;

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
            if (data.IsNull() || _wrapper.Core.HwAccelerated)
                return false;

            retro_hw_render_callback hwRenderCallback = data.ToStructure<retro_hw_render_callback>();
            _hardwareRenderHelperWindow = hwRenderCallback.context_type switch
            {
                retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL
                or retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE => new OpenGLHelperWindow(hwRenderCallback),

                retro_hw_context_type.RETRO_HW_CONTEXT_NONE
                or _ => default
            };

            if (_hardwareRenderHelperWindow is null || !_hardwareRenderHelperWindow.Init())
                return false;

            hwRenderCallback.get_current_framebuffer = _hardwareRenderHelperWindow.GetCurrentFrameBuffer.GetFunctionPointer();
            hwRenderCallback.get_proc_address        = _hardwareRenderHelperWindow.GetProcAddress.GetFunctionPointer();
            Marshal.StructureToPtr(hwRenderCallback, data, true);
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

        private void RefreshCallback(IntPtr data, uint width, uint height, nuint pitch)
        {
            if (Enabled)
                _frameHandler.ProcessFrame(data, width, height, pitch);
        }
    }
}
