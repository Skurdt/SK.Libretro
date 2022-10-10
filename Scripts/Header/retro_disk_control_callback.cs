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

using System.Runtime.InteropServices;

namespace SK.Libretro
{
    //typedef bool (RETRO_CALLCONV *retro_set_eject_state_t)(bool ejected);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool retro_set_eject_state_t(bool ejected);
    //typedef bool (RETRO_CALLCONV *retro_get_eject_state_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool retro_get_eject_state_t();
    //typedef unsigned (RETRO_CALLCONV *retro_get_image_index_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate uint retro_get_image_index_t();
    //typedef bool (RETRO_CALLCONV *retro_set_image_index_t)(unsigned index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool retro_set_image_index_t(uint index);
    //typedef unsigned (RETRO_CALLCONV *retro_get_num_images_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate uint retro_get_num_images_t();
    //typedef bool (RETRO_CALLCONV *retro_replace_image_index_t)(unsigned index, const struct retro_game_info *info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool retro_replace_image_index_t(uint index, ref retro_game_info info);
    //typedef bool (RETRO_CALLCONV *retro_add_image_index_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool retro_add_image_index_t();

    internal struct retro_disk_control_callback
    {
        public retro_set_eject_state_t set_eject_state;
        public retro_get_eject_state_t get_eject_state;
        public retro_get_image_index_t get_image_index;
        public retro_set_image_index_t set_image_index;
        public retro_get_num_images_t get_num_images;
        public retro_replace_image_index_t replace_image_index;
        public retro_add_image_index_t add_image_index;
    }
}
