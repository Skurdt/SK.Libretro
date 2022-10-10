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
    internal static class Header
    {
        public const uint RETRO_API_VERSION = 1;

        public const int RETRO_DEVICE_TYPE_SHIFT = 8;
        public const int RETRO_DEVICE_MASK       = (1 << RETRO_DEVICE_TYPE_SHIFT) - 1;
        public static int RETRO_DEVICE_SUBCLASS(int @base, int id) => ((id + 1) << RETRO_DEVICE_TYPE_SHIFT) | @base;

        public const uint RETRO_DEVICE_NONE     = 0;
        public const uint RETRO_DEVICE_JOYPAD   = 1;
        public const uint RETRO_DEVICE_MOUSE    = 2;
        public const uint RETRO_DEVICE_KEYBOARD = 3;
        public const uint RETRO_DEVICE_LIGHTGUN = 4;
        public const uint RETRO_DEVICE_ANALOG   = 5;
        public const uint RETRO_DEVICE_POINTER  = 6;

        public const uint RETRO_DEVICE_ID_JOYPAD_B      = 0;
        public const uint RETRO_DEVICE_ID_JOYPAD_Y      = 1;
        public const uint RETRO_DEVICE_ID_JOYPAD_SELECT = 2;
        public const uint RETRO_DEVICE_ID_JOYPAD_START  = 3;
        public const uint RETRO_DEVICE_ID_JOYPAD_UP     = 4;
        public const uint RETRO_DEVICE_ID_JOYPAD_DOWN   = 5;
        public const uint RETRO_DEVICE_ID_JOYPAD_LEFT   = 6;
        public const uint RETRO_DEVICE_ID_JOYPAD_RIGHT  = 7;
        public const uint RETRO_DEVICE_ID_JOYPAD_A      = 8;
        public const uint RETRO_DEVICE_ID_JOYPAD_X      = 9;
        public const uint RETRO_DEVICE_ID_JOYPAD_L      = 10;
        public const uint RETRO_DEVICE_ID_JOYPAD_R      = 11;
        public const uint RETRO_DEVICE_ID_JOYPAD_L2     = 12;
        public const uint RETRO_DEVICE_ID_JOYPAD_R2     = 13;
        public const uint RETRO_DEVICE_ID_JOYPAD_L3     = 14;
        public const uint RETRO_DEVICE_ID_JOYPAD_R3     = 15;

        public const uint RETRO_DEVICE_ID_JOYPAD_MASK = 256;

        public const uint RETRO_DEVICE_INDEX_ANALOG_LEFT   = 0;
        public const uint RETRO_DEVICE_INDEX_ANALOG_RIGHT  = 1;
        public const uint RETRO_DEVICE_INDEX_ANALOG_BUTTON = 2;
        public const uint RETRO_DEVICE_ID_ANALOG_X         = 0;
        public const uint RETRO_DEVICE_ID_ANALOG_Y         = 1;

        public const uint RETRO_DEVICE_ID_MOUSE_X               = 0;
        public const uint RETRO_DEVICE_ID_MOUSE_Y               = 1;
        public const uint RETRO_DEVICE_ID_MOUSE_LEFT            = 2;
        public const uint RETRO_DEVICE_ID_MOUSE_RIGHT           = 3;
        public const uint RETRO_DEVICE_ID_MOUSE_WHEELUP         = 4;
        public const uint RETRO_DEVICE_ID_MOUSE_WHEELDOWN       = 5;
        public const uint RETRO_DEVICE_ID_MOUSE_MIDDLE          = 6;
        public const uint RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELUP   = 7;
        public const uint RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELDOWN = 8;
        public const uint RETRO_DEVICE_ID_MOUSE_BUTTON_4        = 9;
        public const uint RETRO_DEVICE_ID_MOUSE_BUTTON_5        = 10;

        public const uint RETRO_DEVICE_ID_LIGHTGUN_SCREEN_X     = 13;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_SCREEN_Y     = 14;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN = 15;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_TRIGGER      = 2;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_RELOAD       = 16;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_AUX_A        = 3;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_AUX_B        = 4;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_START        = 6;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_SELECT       = 7;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_AUX_C        = 8;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_DPAD_UP      = 9;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_DPAD_DOWN    = 10;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_DPAD_LEFT    = 11;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_DPAD_RIGHT   = 12;

        public const uint RETRO_DEVICE_ID_LIGHTGUN_X            = 0;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_Y            = 1;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_CURSOR       = 3;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_TURBO        = 4;
        public const uint RETRO_DEVICE_ID_LIGHTGUN_PAUSE        = 5;

        public const uint RETRO_DEVICE_ID_POINTER_X       = 0;
        public const uint RETRO_DEVICE_ID_POINTER_Y       = 1;
        public const uint RETRO_DEVICE_ID_POINTER_PRESSED = 2;
        public const uint RETRO_DEVICE_ID_POINTER_COUNT   = 3;

        public const int RETRO_REGION_NTSC = 0;
        public const int RETRO_REGION_PAL  = 1;

        public const int RETRO_MEMORY_MASK       = 0xff;
        public const int RETRO_MEMORY_SAVE_RAM   = 0;
        public const int RETRO_MEMORY_RTC        = 1;
        public const int RETRO_MEMORY_SYSTEM_RAM = 2;
        public const int RETRO_MEMORY_VIDEO_RAM  = 3;

        public const int RETRO_VFS_FILE_ACCESS_READ            = 1 << 0;
        public const int RETRO_VFS_FILE_ACCESS_WRITE           = 1 << 1;
        public const int RETRO_VFS_FILE_ACCESS_READ_WRITE      = RETRO_VFS_FILE_ACCESS_READ | RETRO_VFS_FILE_ACCESS_WRITE;
        public const int RETRO_VFS_FILE_ACCESS_UPDATE_EXISTING = 1 << 2;

        public const int RETRO_VFS_FILE_ACCESS_HINT_NONE            = 0;
        public const int RETRO_VFS_FILE_ACCESS_HINT_FREQUENT_ACCESS = 1 << 0;

        public const int RETRO_VFS_SEEK_POSITION_START   = 0;
        public const int RETRO_VFS_SEEK_POSITION_CURRENT = 1;
        public const int RETRO_VFS_SEEK_POSITION_END     = 2;

        public const int RETRO_VFS_STAT_IS_VALID             = 1 << 0;
        public const int RETRO_VFS_STAT_IS_DIRECTORY         = 1 << 1;
        public const int RETRO_VFS_STAT_IS_CHARACTER_SPECIAL = 1 << 2;

        public const int RETRO_SERIALIZATION_QUIRK_INCOMPLETE          = 1 << 0;
        public const int RETRO_SERIALIZATION_QUIRK_MUST_INITIALIZE     = 1 << 1;
        public const int RETRO_SERIALIZATION_QUIRK_CORE_VARIABLE_SIZE  = 1 << 2;
        public const int RETRO_SERIALIZATION_QUIRK_FRONT_VARIABLE_SIZE = 1 << 3;
        public const int RETRO_SERIALIZATION_QUIRK_SINGLE_SESSION      = 1 << 4;
        public const int RETRO_SERIALIZATION_QUIRK_ENDIAN_DEPENDENT    = 1 << 5;
        public const int RETRO_SERIALIZATION_QUIRK_PLATFORM_DEPENDENT  = 1 << 6;

        public const int RETRO_MEMDESC_CONST      = 1 << 0;
        public const int RETRO_MEMDESC_BIGENDIAN  = 1 << 1;
        public const int RETRO_MEMDESC_SYSTEM_RAM = 1 << 2;
        public const int RETRO_MEMDESC_SAVE_RAM   = 1 << 3;
        public const int RETRO_MEMDESC_VIDEO_RAM  = 1 << 4;
        public const int RETRO_MEMDESC_ALIGN_2    = 1 << 16;
        public const int RETRO_MEMDESC_ALIGN_4    = 2 << 16;
        public const int RETRO_MEMDESC_ALIGN_8    = 3 << 16;
        public const int RETRO_MEMDESC_MINSIZE_2  = 1 << 24;
        public const int RETRO_MEMDESC_MINSIZE_4  = 2 << 24;
        public const int RETRO_MEMDESC_MINSIZE_8  = 3 << 24;

        public const int RETRO_SIMD_SSE    = 1 << 0;
        public const int RETRO_SIMD_SSE2   = 1 << 1;
        public const int RETRO_SIMD_VMX    = 1 << 2;
        public const int RETRO_SIMD_VMX128 = 1 << 3;
        public const int RETRO_SIMD_AVX    = 1 << 4;
        public const int RETRO_SIMD_NEON   = 1 << 5;
        public const int RETRO_SIMD_SSE3   = 1 << 6;
        public const int RETRO_SIMD_SSSE3  = 1 << 7;
        public const int RETRO_SIMD_MMX    = 1 << 8;
        public const int RETRO_SIMD_MMXEXT = 1 << 9;
        public const int RETRO_SIMD_SSE4   = 1 << 10;
        public const int RETRO_SIMD_SSE42  = 1 << 11;
        public const int RETRO_SIMD_AVX2   = 1 << 12;
        public const int RETRO_SIMD_VFPU   = 1 << 13;
        public const int RETRO_SIMD_PS     = 1 << 14;
        public const int RETRO_SIMD_AES    = 1 << 15;
        public const int RETRO_SIMD_VFPV3  = 1 << 16;
        public const int RETRO_SIMD_VFPV4  = 1 << 17;
        public const int RETRO_SIMD_POPCNT = 1 << 18;
        public const int RETRO_SIMD_MOVBE  = 1 << 19;
        public const int RETRO_SIMD_CMOV   = 1 << 20;
        public const int RETRO_SIMD_ASIMD  = 1 << 21;

        public const int RETRO_SENSOR_ACCELEROMETER_X = 0;
        public const int RETRO_SENSOR_ACCELEROMETER_Y = 1;
        public const int RETRO_SENSOR_ACCELEROMETER_Z = 2;
        public const int RETRO_SENSOR_GYROSCOPE_X     = 3;
        public const int RETRO_SENSOR_GYROSCOPE_Y     = 4;
        public const int RETRO_SENSOR_GYROSCOPE_Z     = 5;
        public const int RETRO_SENSOR_ILLUMINANCE     = 6;

        public const int RETRO_HW_FRAME_BUFFER_VALID = -1;

        public const int RETRO_NUM_CORE_OPTION_VALUES_MAX = 128;

        public const int RETRO_MEMORY_ACCESS_WRITE = 1 << 0;
        public const int RETRO_MEMORY_ACCESS_READ  = 1 << 1;
        public const int RETRO_MEMORY_TYPE_CACHED  = 1 << 0;

        public const int RETRO_THROTTLE_NONE = 0;
        public const int RETRO_THROTTLE_FRAME_STEPPING = 1;
        public const int RETRO_THROTTLE_FAST_FORWARD = 2;
        public const int RETRO_THROTTLE_SLOW_MOTION = 3;
        public const int RETRO_THROTTLE_REWINDING = 4;
        public const int RETRO_THROTTLE_VSYNC = 5;
        public const int RETRO_THROTTLE_UNBLOCKED = 6;

        // typedef bool (RETRO_CALLCONV *retro_environment_t)(unsigned cmd, void *data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool retro_environment_t(retro_environment cmd, IntPtr data);

        // typedef void (RETRO_CALLCONV *retro_video_refresh_t)(const void *data, unsigned width, unsigned height, size_t pitch);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void retro_video_refresh_t(void* data, uint width, uint height, ulong pitch);

        // typedef void (RETRO_CALLCONV *retro_audio_sample_t)(int16_t left, int16_t right);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_audio_sample_t(short left, short right);

        // typedef size_t (RETRO_CALLCONV *retro_audio_sample_batch_t)(const int16_t *data, size_t frames);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate ulong retro_audio_sample_batch_t(short* data, ulong frames);

        // typedef void (RETRO_CALLCONV *retro_input_poll_t)(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_input_poll_t();

        // typedef int16_t (RETRO_CALLCONV* retro_input_state_t)(unsigned port, unsigned device, unsigned index, unsigned id);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate short retro_input_state_t(uint port, uint device, uint index, uint id);

        // RETRO_API void retro_set_environment(retro_environment_t);
        public delegate void retro_set_environment_t(retro_environment_t cb);

        // RETRO_API void retro_set_video_refresh(retro_video_refresh_t);
        public delegate void retro_set_video_refresh_t(retro_video_refresh_t cb);

        // RETRO_API void retro_set_audio_sample(retro_audio_sample_t);
        public delegate void retro_set_audio_sample_t(retro_audio_sample_t cb);

        // RETRO_API void retro_set_audio_sample_batch(retro_audio_sample_batch_t);
        public delegate void retro_set_audio_sample_batch_t(retro_audio_sample_batch_t cb);

        // RETRO_API void retro_set_input_poll(retro_input_poll_t);
        public delegate void retro_set_input_poll_t(retro_input_poll_t cb);

        // RETRO_API void retro_set_input_state(retro_input_state_t);
        public delegate void retro_set_input_state_t(retro_input_state_t cb);

        // RETRO_API void retro_init(void);
        public delegate void retro_init_t();

        // RETRO_API void retro_deinit(void);
        public delegate void retro_deinit_t();

        // RETRO_API unsigned retro_api_version(void);
        public delegate uint retro_api_version_t();

        // RETRO_API void retro_get_system_info(struct retro_system_info *info);
        public delegate void retro_get_system_info_t(out retro_system_info info);

        // RETRO_API void retro_get_system_av_info(struct retro_system_av_info *info);
        public delegate void retro_get_system_av_info_t(out retro_system_av_info info);

        // RETRO_API void retro_set_controller_port_device(unsigned port, unsigned device);
        public delegate void retro_set_controller_port_device_t(uint port, uint device);

        // RETRO_API void retro_reset(void);
        public delegate void retro_reset_t();

        // RETRO_API void retro_run(void);
        public delegate void retro_run_t();

        // RETRO_API size_t retro_serialize_size(void);
        public delegate ulong retro_serialize_size_t();

        // RETRO_API bool retro_serialize(void* data, size_t size);
        public delegate bool retro_serialize_t(IntPtr data, ulong size);

        // RETRO_API bool retro_unserialize(const void* data, size_t size);
        public delegate bool retro_unserialize_t(IntPtr data, ulong size);

        // RETRO_API void retro_cheat_reset(void);
        public delegate void retro_cheat_reset_t();

        // RETRO_API void retro_cheat_set(unsigned index, bool enabled, const char* code);
        public delegate void retro_cheat_set_t(uint index, bool enabled, IntPtr code);

        // RETRO_API bool retro_load_game(const struct retro_game_info *game);
        public delegate bool retro_load_game_t(ref retro_game_info game);

        // RETRO_API bool retro_load_game_special(unsigned game_type, const struct retro_game_info *info, size_t num_info);
        public delegate bool retro_load_game_special_t(uint game_type, ref retro_game_info info, ulong num_info);

        // RETRO_API void retro_unload_game(void);
        public delegate void retro_unload_game_t();

        // RETRO_API unsigned retro_get_region(void);
        public delegate uint retro_get_region_t();

        // RETRO_API void* retro_get_memory_data(unsigned id);
        public delegate IntPtr retro_get_memory_data_t(uint id);

        // RETRO_API size_t retro_get_memory_size(unsigned id);
        public delegate ulong retro_get_memory_size_t(uint id);
    }
}
