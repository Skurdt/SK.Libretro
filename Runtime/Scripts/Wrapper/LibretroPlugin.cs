/* MIT License

 * Copyright (c) 2020 Skurdt
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
    internal sealed class LibretroPlugin
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct InteropInterface
        {
            public IntPtr context_reset;
            public IntPtr context_destroy;

            public IntPtr retro_run;
        }

        [DllImport("LibretroUnityPlugin", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetupInteropInterface(ref InteropInterface interopInterface);

        [DllImport("LibretroUnityPlugin", CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr GetCurrentFramebuffer();

        [DllImport("LibretroUnityPlugin", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetHwProcAddress([MarshalAs(UnmanagedType.LPStr)] string sym);

        [DllImport("LibretroUnityPlugin", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetRenderEventFunc();
    }
}
