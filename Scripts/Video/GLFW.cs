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
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal static class GLFW
    {
        public const int FOCUSED = 0x00020001;
        public const int VISIBLE = 0x00020004;
        public const int FOCUS_ON_SHOW = 0x0002000C;
        public const int RED_BITS = 0x00021001;
        public const int GREEN_BITS = 0x00021002;
        public const int BLUE_BITS = 0x00021003;
        public const int ALPHA_BITS = 0x00021004;
        public const int DEPTH_BITS = 0x00021005;
        public const int STENCIL_BITS = 0x00021006;
        public const int CLIENT_API = 0x00022001;
        public const int CONTEXT_VERSION_MAJOR = 0x00022002;
        public const int CONTEXT_VERSION_MINOR = 0x00022003;
        public const int OPENGL_FORWARD_COMPAT = 0x00022006;
        public const int OPENGL_PROFILE = 0x00022008;
        public const int CONTEXT_CREATION_API = 0x0002200B;
        public const int FALSE = 0;
        public const int OPENGL_API = 0x00030001;
        public const int OPENGL_ANY_PROFILE = 0;
        public const int NATIVE_CONTEXT_API = 0x00036001;

        [DllImport("glfw3", EntryPoint = "glfwInit")]
        public static extern bool Init();
        
        [DllImport("glfw3", EntryPoint = "glfwWindowHint")]
        public static extern void WindowHint(int hint, int value);
        
        [DllImport("glfw3", EntryPoint = "glfwCreateWindow")]
        public static extern IntPtr CreateWindow(int width, int height, string title, IntPtr monitor, IntPtr share);
        
        [DllImport("glfw3", EntryPoint = "glfwTerminate")]
        public static extern void Terminate();
        
        [DllImport("glfw3", EntryPoint = "glfwMakeContextCurrent")]
        public static extern void MakeContextCurrent(IntPtr window);
        
        [DllImport("glfw3", EntryPoint = "glfwDestroyWindow")]
        public static extern void DestroyWindow(IntPtr window);
        
        [DllImport("glfw3", EntryPoint = "glfwPollEvents")]
        public static extern void PollEvents();
        
        [DllImport("glfw3", EntryPoint = "glfwSwapBuffers")]
        public static extern void SwapBuffers(IntPtr window);

        [DllImport("glfw3", EntryPoint = "glfwGetProcAddress")]
        public static extern IntPtr GetProcAddress(string procname);
    }
}
