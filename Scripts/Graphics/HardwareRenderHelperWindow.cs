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

        private IntPtr _windowHandle;
        private bool _disposedValue;

        public HardwareRenderHelperWindow()
        {
            GetCurrentFrameBuffer = GetCurrentFrameBufferCall;
            GetProcAddress        = GetProcAddressCall;
        }

        ~HardwareRenderHelperWindow() => DisposeImpl();

        public bool Init()
        {
            if (!GLFW.Init())
                return false;

            SetCreationHints();

            _windowHandle = GLFW.CreateWindow(1920, 1080, "LibretroHardwareRenderHelperWindow", IntPtr.Zero, IntPtr.Zero);
            if (_windowHandle.IsNull())
            {
                GLFW.Terminate();
                return false;
            }

            OnPostInit();
            return true;
        }

        public void Dispose()
        {
            DisposeImpl();
            GC.SuppressFinalize(this);
        }

        protected abstract void SetCreationHints();

        protected abstract void OnPostInit();

        protected abstract UIntPtr GetCurrentFrameBufferCall();

        private IntPtr GetProcAddressCall(string functionName) => GLFW.GetProcAddress(functionName);

        private void DisposeImpl()
        {
            if (_disposedValue)
                return;

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
