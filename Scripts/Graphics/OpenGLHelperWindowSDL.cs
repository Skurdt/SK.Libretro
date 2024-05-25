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
    internal sealed class OpenGLHelperWindowSDL : HardwareRenderHelperWindow
    {
        private IntPtr _glContext;

        public OpenGLHelperWindowSDL(retro_hw_render_callback hwRenderCallback)
        : base(hwRenderCallback)
        {
        }

        public override bool Init()
        {
            if (SDL.InitSubSystem(SDL.INIT_VIDEO) != 0)
                return false;

            _windowHandle = SDL.CreateWindow("LibretroHardwareRenderHelperWindow", 1280, 1024, SDL.WINDOW_HIDDEN | SDL.WINDOW_OPENGL);
            if (_windowHandle.IsNull())
                return false;

            _glContext = SDL.GL_CreateContext(_windowHandle);
            return _glContext.IsNotNull() && SDL.GL_MakeCurrent(_windowHandle, _glContext) == 0;
        }

        public override void PollEvents()
        {
        }

        public override void SwapBuffers() => SDL.GL_SwapWindow(_windowHandle);

        protected override void DeInit()
        {
            if (_glContext.IsNotNull())
            {
                _ = SDL.GL_DeleteContext(_glContext);
                PointerUtilities.SetToNull(ref _glContext);
            }

            if (_windowHandle.IsNotNull())
            {
                SDL.DestroyWindow(_windowHandle);
                PointerUtilities.SetToNull(ref _windowHandle);
            }

            SDL.QuitSubSystem(SDL.INIT_VIDEO);
        }

        protected override IntPtr GetCurrentFrameBufferCall() => (IntPtr)0;

        protected override IntPtr GetProcAddressCall(string functionName) => SDL.GL_GetProcAddress(functionName);
    }
}
