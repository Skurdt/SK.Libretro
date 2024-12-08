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

using System;
using System.Runtime.InteropServices;

namespace SK.Libretro.Header
{
    // typedef void (RETRO_CALLCONV *retro_hw_context_reset_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_hw_context_reset_t();
    // typedef uintptr_t (RETRO_CALLCONV *retro_hw_get_current_framebuffer_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr retro_hw_get_current_framebuffer_t();
    // typedef retro_proc_address_t (RETRO_CALLCONV *retro_hw_get_proc_address_t)(const char* sym);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr retro_hw_get_proc_address_t([MarshalAs(UnmanagedType.LPStr)] string sym);

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class retro_hw_render_callback
    {
        public retro_hw_context_type context_type;
        public IntPtr context_reset;           // retro_hw_context_reset_t
        public IntPtr get_current_framebuffer; // retro_hw_get_current_framebuffer_t
        public IntPtr get_proc_address;        // retro_hw_get_proc_address_t
        [MarshalAs(UnmanagedType.I1)] public bool depth;
        [MarshalAs(UnmanagedType.I1)] public bool stencil;
        [MarshalAs(UnmanagedType.I1)] public bool bottom_left_origin;
        public uint version_major;
        public uint version_minor;
        [MarshalAs(UnmanagedType.I1)] public bool cache_context;
        public IntPtr context_destroy;         // retro_hw_context_reset_t
        [MarshalAs(UnmanagedType.I1)] public bool debug_context;
    }
}
