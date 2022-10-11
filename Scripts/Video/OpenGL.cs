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
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class OpenGL
    {
        // GLFW
        private const int GLFW_FOCUSED                  = 0x00020001;
        private const int GLFW_VISIBLE                  = 0x00020004;
        private const int GLFW_FOCUS_ON_SHOW            = 0x0002000C;
        private const int GLFW_RED_BITS                 = 0x00021001;
        private const int GLFW_GREEN_BITS               = 0x00021002;
        private const int GLFW_BLUE_BITS                = 0x00021003;
        private const int GLFW_ALPHA_BITS               = 0x00021004;
        private const int GLFW_DEPTH_BITS               = 0x00021005;
        private const int GLFW_STENCIL_BITS             = 0x00021006;
        private const int GLFW_CLIENT_API               = 0x00022001;
        private const int GLFW_CONTEXT_VERSION_MAJOR    = 0x00022002;
        private const int GLFW_CONTEXT_VERSION_MINOR    = 0x00022003;
        private const int GLFW_OPENGL_FORWARD_COMPAT    = 0x00022006;
        private const int GLFW_OPENGL_PROFILE           = 0x00022008;
        private const int GLFW_CONTEXT_CREATION_API     = 0x0002200B;
        private const int GLFW_FALSE                    = 0;
        private const int GLFW_OPENGL_API               = 0x00030001;
        private const int GLFW_OPENGL_ANY_PROFILE       = 0;
        private const int GLFW_NATIVE_CONTEXT_API       = 0x00036001;

        [DllImport("glfw3")] private static extern bool glfwInit();
        [DllImport("glfw3")] private static extern void glfwWindowHint(int hint, int value);
        [DllImport("glfw3")] private static extern IntPtr glfwCreateWindow(int width, int height, string title, IntPtr monitor, IntPtr share);
        [DllImport("glfw3")] private static extern void glfwTerminate();
        [DllImport("glfw3")] private static extern void glfwMakeContextCurrent(IntPtr window);
        [DllImport("glfw3")] private static extern void glfwDestroyWindow(IntPtr window);
        [DllImport("glfw3")] private static extern void glfwPollEvents();
        [DllImport("glfw3")] private static extern void glfwSwapBuffers(IntPtr window);
        [DllImport("glfw3")] private static extern IntPtr glfwGetProcAddress(string procname);

        // GL
        public const uint GL_BGRA          = 0x80E1;
        public const uint GL_UNSIGNED_BYTE = 0x1401;

        public unsafe delegate void glReadPixels_f(int x, int y, int width, int height, uint format, uint type, void* data);
        public glReadPixels_f glReadPixels;

        public retro_hw_get_current_framebuffer_t GetCurrentFrameBufferCallback;
        public retro_hw_get_proc_address_t GetProcAddressCallback;
        private IntPtr _windowHandle;

        public bool Init()
        {
            if (!glfwInit())
                return false;

            glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
            glfwWindowHint(GLFW_FOCUSED, GLFW_FALSE);
            glfwWindowHint(GLFW_FOCUS_ON_SHOW, GLFW_FALSE);
            glfwWindowHint(GLFW_RED_BITS , 8);
            glfwWindowHint(GLFW_GREEN_BITS , 8);
            glfwWindowHint(GLFW_BLUE_BITS , 8);
            glfwWindowHint(GLFW_ALPHA_BITS , 8);
            glfwWindowHint(GLFW_DEPTH_BITS , 24);
            glfwWindowHint(GLFW_STENCIL_BITS, 8);
            glfwWindowHint(GLFW_CLIENT_API, GLFW_OPENGL_API);
            glfwWindowHint(GLFW_CONTEXT_CREATION_API, GLFW_NATIVE_CONTEXT_API);
            glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 1);
            glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 0);
            glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GLFW_FALSE);
            glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_ANY_PROFILE);

            _windowHandle = glfwCreateWindow(800, 600, "LibretroOpenGLHelperWindow", IntPtr.Zero, IntPtr.Zero);
            if (_windowHandle.IsNull())
            {
                glfwTerminate();
                return false;
            }

            glfwMakeContextCurrent(_windowHandle);

            glReadPixels = Marshal.GetDelegateForFunctionPointer<glReadPixels_f>(glfwGetProcAddress("glReadPixels"));
            if (glReadPixels is null)
            {
                glfwTerminate();
                return false;
            }

            GetCurrentFrameBufferCallback = GetCurrentFrameBuffer;
            GetProcAddressCallback        = GetProcAddress;

            return true;
        }

        public void Deinit() =>
            glfwTerminate();

        public void PollEvents() =>
            glfwPollEvents();

        public void SwapBuffers() =>
            glfwSwapBuffers(_windowHandle);

        private UIntPtr GetCurrentFrameBuffer() =>
            (UIntPtr)0;

        private IntPtr GetProcAddress(string functionName) =>
            glfwGetProcAddress(functionName);
    }
}
