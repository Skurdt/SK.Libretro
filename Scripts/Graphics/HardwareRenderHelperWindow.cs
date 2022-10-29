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

namespace SK.Libretro
{
    internal abstract class HardwareRenderHelperWindow : IDisposable
    {
        public readonly retro_hw_get_current_framebuffer_t GetCurrentFrameBuffer;
        public readonly retro_hw_get_proc_address_t GetProcAddress;

        protected IntPtr WindowHandle => _windowHandle;

        private readonly retro_hw_context_reset_t _contextReset;
        private readonly retro_hw_context_reset_t _contextDestroy;

        private IntPtr _windowHandle;
        private bool _disposedValue;

        public HardwareRenderHelperWindow(retro_hw_render_callback hwRenderCallback)
        {
            _contextReset         = hwRenderCallback.context_reset.GetDelegate<retro_hw_context_reset_t>();
            _contextDestroy       = hwRenderCallback.context_destroy.GetDelegate<retro_hw_context_reset_t>();
            GetCurrentFrameBuffer = GetCurrentFrameBufferCall;
            GetProcAddress        = GetProcAddressCall;
        }

        ~HardwareRenderHelperWindow() => Dispose(disposing: false);

        public bool Init()
        {
            if (!GLFW.Init())
                return false;

            SetCreationHints();

            _windowHandle = GLFW.CreateWindow(640, 512, "LibretroHardwareRenderHelperWindow", IntPtr.Zero, IntPtr.Zero);
            if (_windowHandle.IsNull())
                return false;

            OnPostInit();
            return true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void InitContext() => _contextReset.Invoke();

        public abstract void SwapBuffers();

        protected abstract void SetCreationHints();

        protected abstract void OnPostInit();

        protected abstract UIntPtr GetCurrentFrameBufferCall();

        private IntPtr GetProcAddressCall(string functionName) => GLFW.GetProcAddress(functionName);

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing)
                _contextDestroy?.Invoke();

            if (_windowHandle.IsNotNull())
            {
                GLFW.DestroyWindow(_windowHandle);
                PointerUtilities.SetToNull(ref _windowHandle);
            }

            GLFW.Terminate();

            _disposedValue = true;
        }
    }
}
