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
    // typedef bool (RETRO_CALLCONV *retro_camera_start_t) (void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_camera_start_t();
    // typedef void (RETRO_CALLCONV *retro_camera_stop_t) (void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_camera_stop_t();
    // typedef void (RETRO_CALLCONV *retro_camera_lifetime_status_t) (void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_camera_lifetime_status_t();
    // typedef void (RETRO_CALLCONV *retro_camera_frame_raw_framebuffer_t) (const uint32_t* buffer, unsigned width, unsigned height, size_t pitch);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_camera_frame_raw_framebuffer_t(UIntPtr buffer, uint width, uint height, nuint pitch);
    // typedef void (RETRO_CALLCONV *retro_camera_frame_opengl_texture_t) (unsigned texture_id, unsigned texture_target, const float* affine);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_camera_frame_opengl_texture_t(uint texture_id, uint texture_target, ref float affine);

    internal struct retro_camera_callback
    {
        public ulong caps;
        public uint width;
        public uint height;
        public IntPtr start;                 // retro_camera_start_t
        public IntPtr stop;                  // retro_camera_stop_t
        public IntPtr frame_raw_framebuffer; // retro_camera_frame_raw_framebuffer_t
        public IntPtr frame_opengl_texture;  // retro_camera_frame_opengl_texture_t
        public IntPtr initialized;           // retro_camera_lifetime_status_t
        public IntPtr deinitialized;         // retro_camera_lifetime_status_t
    }
}
