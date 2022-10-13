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

using System;

namespace SK.Libretro
{
    internal sealed class OpenGLHelperWindow : HardwareRenderHelperWindow
    {
        public void SwapBuffers() => GLFW.SwapBuffers(WindowHandle);

        protected override void SetCreationHints()
        {
            GLFW.WindowHint(GLFW.VISIBLE, GLFW.FALSE);
            GLFW.WindowHint(GLFW.FOCUSED, GLFW.FALSE);
            GLFW.WindowHint(GLFW.FOCUS_ON_SHOW, GLFW.FALSE);
            //GLFW.WindowHint(GLFW.RED_BITS, 8);
            //GLFW.WindowHint(GLFW.GREEN_BITS, 8);
            //GLFW.WindowHint(GLFW.BLUE_BITS, 8);
            //GLFW.WindowHint(GLFW.ALPHA_BITS, 8);
            //GLFW.WindowHint(GLFW.DEPTH_BITS, 24);
            //GLFW.WindowHint(GLFW.STENCIL_BITS, 8);
            GLFW.WindowHint(GLFW.CLIENT_API, GLFW.OPENGL_API);
            //GLFW.WindowHint(GLFW.CONTEXT_CREATION_API, GLFW.NATIVE_CONTEXT_API);
            //GLFW.WindowHint(GLFW.CONTEXT_VERSION_MAJOR, 1);
            //GLFW.WindowHint(GLFW.CONTEXT_VERSION_MINOR, 0);
            //GLFW.WindowHint(GLFW.OPENGL_FORWARD_COMPAT, GLFW.FALSE);
            //GLFW.WindowHint(GLFW.OPENGL_PROFILE, GLFW.OPENGL_ANY_PROFILE);
        }

        protected override void OnPostInit() => GLFW.MakeContextCurrent(WindowHandle);

        protected override UIntPtr GetCurrentFrameBufferCall() => (UIntPtr)0;
    }
}
