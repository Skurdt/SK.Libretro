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
    // typedef bool (RETRO_CALLCONV *retro_set_initial_image_t)(unsigned index, const char *path);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_set_initial_image_t(uint index, [MarshalAs(UnmanagedType.LPStr)] string path);
    
    // typedef bool (RETRO_CALLCONV *retro_get_image_path_t)(unsigned index, char *path, size_t len);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_get_image_path_t(uint index, [MarshalAs(UnmanagedType.LPStr)] ref string path, nuint len);
    
    // typedef bool (RETRO_CALLCONV *retro_get_image_label_t)(unsigned index, char *label, size_t len);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_get_image_label_t(uint index, [MarshalAs(UnmanagedType.LPStr)] ref string label, nuint len);

    internal struct retro_disk_control_ext_callback
    {
        public IntPtr set_eject_state;     // retro_set_eject_state_t
        public IntPtr get_eject_state;     // retro_get_eject_state_t
        public IntPtr get_image_index;     // retro_get_image_index_t
        public IntPtr set_image_index;     // retro_set_image_index_t
        public IntPtr get_num_images;      // retro_get_num_images_t
        public IntPtr replace_image_index; // retro_replace_image_index_t
        public IntPtr add_image_index;     // retro_add_image_index_t
        public IntPtr set_initial_image;   // retro_set_initial_image_t
        public IntPtr get_image_path;      // retro_get_image_path_t
        public IntPtr get_image_label;     // retro_get_image_label_t
    }
}
