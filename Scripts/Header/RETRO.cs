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
    internal static class RETRO
    {
        public const uint API_VERSION = 1;

        public const int DEVICE_TYPE_SHIFT = 8;
        public const int DEVICE_MASK       = (1 << DEVICE_TYPE_SHIFT) - 1;
        public static int DEVICE_SUBCLASS(int @base, int id) => ((id + 1) << DEVICE_TYPE_SHIFT) | @base;

        public const int HW_FRAME_BUFFER_VALID = -1;

        public const int NUM_CORE_OPTION_VALUES_MAX = 128;

        public const int MEMORY_TYPE_CACHED = 1 << 0;
    }

    // typedef bool (CALLCONV *retro_environment_t)(unsigned cmd, void *data);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_environment_t(RETRO_ENVIRONMENT cmd, IntPtr data);

    // typedef void (CALLCONV *retro_video_refresh_t)(const void *data, unsigned width, unsigned height, size_t pitch);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_video_refresh_t(IntPtr data, uint width, uint height, nuint pitch);

    // typedef void (CALLCONV *retro_audio_sample_t)(int16_t left, int16_t right);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_audio_sample_t(short left, short right);

    // typedef size_t (CALLCONV *retro_audio_sample_batch_t)(const int16_t *data, size_t frames);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nuint retro_audio_sample_batch_t(IntPtr data, nuint frames);

    // typedef void (CALLCONV *retro_input_poll_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_input_poll_t();

    // typedef int16_t (CALLCONV* retro_input_state_t)(unsigned port, unsigned device, unsigned index, unsigned id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate short retro_input_state_t(uint port, RETRO_DEVICE device, uint index, uint id);

    // API void retro_set_environment(retro_environment_t);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_set_environment_t(retro_environment_t cb);

    // API void retro_set_video_refresh(retro_video_refresh_t);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_set_video_refresh_t(retro_video_refresh_t cb);

    // API void retro_set_audio_sample(retro_audio_sample_t);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_set_audio_sample_t(retro_audio_sample_t cb);

    // API void retro_set_audio_sample_batch(retro_audio_sample_batch_t);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_set_audio_sample_batch_t(retro_audio_sample_batch_t cb);

    // API void retro_set_input_poll(retro_input_poll_t);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_set_input_poll_t(retro_input_poll_t cb);

    // API void retro_set_input_state(retro_input_state_t);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_set_input_state_t(retro_input_state_t cb);

    // API void retro_init(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_init_t();

    // API void retro_deinit(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_deinit_t();

    // API unsigned retro_api_version(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate uint retro_api_version_t();

    // API void retro_get_system_info(struct retro_system_info *info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_get_system_info_t(out retro_system_info info);

    // API void retro_get_system_av_info(struct retro_system_av_info *info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_get_system_av_info_t(out retro_system_av_info info);

    // API void retro_set_controller_port_device(unsigned port, unsigned device);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_set_controller_port_device_t(uint port, uint device);

    // API void retro_reset(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_reset_t();

    // API void retro_run(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_run_t();

    // API size_t retro_serialize_size(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long retro_serialize_size_t();

    // API bool retro_serialize(void* data, size_t size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_serialize_t(IntPtr data, long size);

    // API bool retro_unserialize(const void* data, size_t size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_unserialize_t(IntPtr data, long size);

    // API void retro_cheat_reset(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_cheat_reset_t();

    // API void retro_cheat_set(unsigned index, bool enabled, const char* code);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_cheat_set_t(uint index, [MarshalAs(UnmanagedType.I1)] bool enabled, IntPtr code);

    // API bool retro_load_game(const struct retro_game_info *game);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_load_game_t(ref retro_game_info game);

    // API bool retro_load_game_special(unsigned game_type, const struct retro_game_info *info, size_t num_info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_load_game_special_t(uint game_type, ref retro_game_info info, nuint num_info);

    // API void retro_unload_game(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_unload_game_t();

    // API unsigned retro_get_region(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate uint retro_get_region_t();

    // API void* retro_get_memory_data(unsigned id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr retro_get_memory_data_t(RETRO_MEMORY id);

    // API size_t retro_get_memory_size(unsigned id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate nuint retro_get_memory_size_t(RETRO_MEMORY id);
}
