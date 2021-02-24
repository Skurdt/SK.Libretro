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

using int8_t               = System.SByte;
using int16_t              = System.Int16;
using int32_t              = System.Int32;
using int64_t              = System.Int64;
using uint8_t              = System.Byte;
using uint16_t             = System.UInt16;
using uint32_t             = System.UInt32;
using uint64_t             = System.UInt64;
using size_t               = System.UInt64;
using uintptr_t            = System.UIntPtr;
using retro_perf_tick_t    = System.UInt64;
using retro_time_t         = System.Int64;
using retro_usec_t         = System.Int64;
using retro_proc_address_t = System.IntPtr;

namespace SK.Libretro
{
    internal static unsafe class LibretroHeader
    {
#pragma warning disable IDE1006 // Naming Styles
        public const uint RETRO_API_VERSION = 1;

        public const int RETRO_DEVICE_TYPE_SHIFT = 8;
        public const int RETRO_DEVICE_MASK       = (1 << RETRO_DEVICE_TYPE_SHIFT) - 1;
        public static int RETRO_DEVICE_SUBCLASS(int base_, int id) => ((id + 1) << RETRO_DEVICE_TYPE_SHIFT) | base_;

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

        public enum retro_language : int
        {
            RETRO_LANGUAGE_ENGLISH             = 0,
            RETRO_LANGUAGE_JAPANESE            = 1,
            RETRO_LANGUAGE_FRENCH              = 2,
            RETRO_LANGUAGE_SPANISH             = 3,
            RETRO_LANGUAGE_GERMAN              = 4,
            RETRO_LANGUAGE_ITALIAN             = 5,
            RETRO_LANGUAGE_DUTCH               = 6,
            RETRO_LANGUAGE_PORTUGUESE_BRAZIL   = 7,
            RETRO_LANGUAGE_PORTUGUESE_PORTUGAL = 8,
            RETRO_LANGUAGE_RUSSIAN             = 9,
            RETRO_LANGUAGE_KOREAN              = 10,
            RETRO_LANGUAGE_CHINESE_TRADITIONAL = 11,
            RETRO_LANGUAGE_CHINESE_SIMPLIFIED  = 12,
            RETRO_LANGUAGE_ESPERANTO           = 13,
            RETRO_LANGUAGE_POLISH              = 14,
            RETRO_LANGUAGE_VIETNAMESE          = 15,
            RETRO_LANGUAGE_ARABIC              = 16,
            RETRO_LANGUAGE_GREEK               = 17,
            RETRO_LANGUAGE_TURKISH             = 18,
            RETRO_LANGUAGE_SLOVAK              = 19,
            RETRO_LANGUAGE_PERSIAN             = 20,
            RETRO_LANGUAGE_HEBREW              = 21,
            RETRO_LANGUAGE_ASTURIAN            = 22,
            RETRO_LANGUAGE_LAST
        }

        public const int RETRO_MEMORY_MASK       = 0xff;
        public const int RETRO_MEMORY_SAVE_RAM   = 0;
        public const int RETRO_MEMORY_RTC        = 1;
        public const int RETRO_MEMORY_SYSTEM_RAM = 2;
        public const int RETRO_MEMORY_VIDEO_RAM  = 3;

        public enum retro_key : uint
        {
            RETROK_UNKNOWN      = 0,
            RETROK_FIRST        = 0,
            RETROK_BACKSPACE    = 8,
            RETROK_TAB          = 9,
            RETROK_CLEAR        = 12,
            RETROK_RETURN       = 13,
            RETROK_PAUSE        = 19,
            RETROK_ESCAPE       = 27,
            RETROK_SPACE        = 32,
            RETROK_EXCLAIM      = 33,
            RETROK_QUOTEDBL     = 34,
            RETROK_HASH         = 35,
            RETROK_DOLLAR       = 36,
            RETROK_AMPERSAND    = 38,
            RETROK_QUOTE        = 39,
            RETROK_LEFTPAREN    = 40,
            RETROK_RIGHTPAREN   = 41,
            RETROK_ASTERISK     = 42,
            RETROK_PLUS         = 43,
            RETROK_COMMA        = 44,
            RETROK_MINUS        = 45,
            RETROK_PERIOD       = 46,
            RETROK_SLASH        = 47,
            RETROK_0            = 48,
            RETROK_1            = 49,
            RETROK_2            = 50,
            RETROK_3            = 51,
            RETROK_4            = 52,
            RETROK_5            = 53,
            RETROK_6            = 54,
            RETROK_7            = 55,
            RETROK_8            = 56,
            RETROK_9            = 57,
            RETROK_COLON        = 58,
            RETROK_SEMICOLON    = 59,
            RETROK_LESS         = 60,
            RETROK_EQUALS       = 61,
            RETROK_GREATER      = 62,
            RETROK_QUESTION     = 63,
            RETROK_AT           = 64,
            RETROK_LEFTBRACKET  = 91,
            RETROK_BACKSLASH    = 92,
            RETROK_RIGHTBRACKET = 93,
            RETROK_CARET        = 94,
            RETROK_UNDERSCORE   = 95,
            RETROK_BACKQUOTE    = 96,
            RETROK_a            = 97,
            RETROK_b            = 98,
            RETROK_c            = 99,
            RETROK_d            = 100,
            RETROK_e            = 101,
            RETROK_f            = 102,
            RETROK_g            = 103,
            RETROK_h            = 104,
            RETROK_i            = 105,
            RETROK_j            = 106,
            RETROK_k            = 107,
            RETROK_l            = 108,
            RETROK_m            = 109,
            RETROK_n            = 110,
            RETROK_o            = 111,
            RETROK_p            = 112,
            RETROK_q            = 113,
            RETROK_r            = 114,
            RETROK_s            = 115,
            RETROK_t            = 116,
            RETROK_u            = 117,
            RETROK_v            = 118,
            RETROK_w            = 119,
            RETROK_x            = 120,
            RETROK_y            = 121,
            RETROK_z            = 122,
            RETROK_LEFTBRACE    = 123,
            RETROK_BAR          = 124,
            RETROK_RIGHTBRACE   = 125,
            RETROK_TILDE        = 126,
            RETROK_DELETE       = 127,

            RETROK_KP0          = 256,
            RETROK_KP1          = 257,
            RETROK_KP2          = 258,
            RETROK_KP3          = 259,
            RETROK_KP4          = 260,
            RETROK_KP5          = 261,
            RETROK_KP6          = 262,
            RETROK_KP7          = 263,
            RETROK_KP8          = 264,
            RETROK_KP9          = 265,
            RETROK_KP_PERIOD    = 266,
            RETROK_KP_DIVIDE    = 267,
            RETROK_KP_MULTIPLY  = 268,
            RETROK_KP_MINUS     = 269,
            RETROK_KP_PLUS      = 270,
            RETROK_KP_ENTER     = 271,
            RETROK_KP_EQUALS    = 272,

            RETROK_UP           = 273,
            RETROK_DOWN         = 274,
            RETROK_RIGHT        = 275,
            RETROK_LEFT         = 276,
            RETROK_INSERT       = 277,
            RETROK_HOME         = 278,
            RETROK_END          = 279,
            RETROK_PAGEUP       = 280,
            RETROK_PAGEDOWN     = 281,

            RETROK_F1           = 282,
            RETROK_F2           = 283,
            RETROK_F3           = 284,
            RETROK_F4           = 285,
            RETROK_F5           = 286,
            RETROK_F6           = 287,
            RETROK_F7           = 288,
            RETROK_F8           = 289,
            RETROK_F9           = 290,
            RETROK_F10          = 291,
            RETROK_F11          = 292,
            RETROK_F12          = 293,
            RETROK_F13          = 294,
            RETROK_F14          = 295,
            RETROK_F15          = 296,

            RETROK_NUMLOCK      = 300,
            RETROK_CAPSLOCK     = 301,
            RETROK_SCROLLOCK    = 302,
            RETROK_RSHIFT       = 303,
            RETROK_LSHIFT       = 304,
            RETROK_RCTRL        = 305,
            RETROK_LCTRL        = 306,
            RETROK_RALT         = 307,
            RETROK_LALT         = 308,
            RETROK_RMETA        = 309,
            RETROK_LMETA        = 310,
            RETROK_LSUPER       = 311,
            RETROK_RSUPER       = 312,
            RETROK_MODE         = 313,
            RETROK_COMPOSE      = 314,

            RETROK_HELP         = 315,
            RETROK_PRINT        = 316,
            RETROK_SYSREQ       = 317,
            RETROK_BREAK        = 318,
            RETROK_MENU         = 319,
            RETROK_POWER        = 320,
            RETROK_EURO         = 321,
            RETROK_UNDO         = 322,
            RETROK_OEM_102      = 323,

            RETROK_LAST
        }

        public enum retro_mod : uint
        {
            RETROKMOD_NONE      = 0x0000,

            RETROKMOD_SHIFT     = 0x01,
            RETROKMOD_CTRL      = 0x02,
            RETROKMOD_ALT       = 0x04,
            RETROKMOD_META      = 0x08,

            RETROKMOD_NUMLOCK   = 0x10,
            RETROKMOD_CAPSLOCK  = 0x20,
            RETROKMOD_SCROLLOCK = 0x40
        }

        public const int RETRO_ENVIRONMENT_EXPERIMENTAL = 0x10000;
        public const int RETRO_ENVIRONMENT_PRIVATE      = 0x20000;

        // RetroArch Extensions
        public const int RETRO_ENVIRONMENT_RETROARCH_START_BLOCK = 0x800000;

        // NOTE(Tom): Original defines as an enum
        public enum retro_environment : uint
        {
            RETRO_ENVIRONMENT_SET_ROTATION                                = 1,
            RETRO_ENVIRONMENT_GET_OVERSCAN                                = 2,
            RETRO_ENVIRONMENT_GET_CAN_DUPE                                = 3,
            RETRO_ENVIRONMENT_SET_MESSAGE                                 = 6,
            RETRO_ENVIRONMENT_SHUTDOWN                                    = 7,
            RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL                       = 8,
            RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY                        = 9,
            RETRO_ENVIRONMENT_SET_PIXEL_FORMAT                            = 10,
            RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS                       = 11,
            RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK                       = 12,
            RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE                  = 13,
            RETRO_ENVIRONMENT_SET_HW_RENDER                               = 14,
            RETRO_ENVIRONMENT_GET_VARIABLE                                = 15,
            RETRO_ENVIRONMENT_SET_VARIABLES                               = 16,
            RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE                         = 17,
            RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME                         = 18,
            RETRO_ENVIRONMENT_GET_LIBRETRO_PATH                           = 19,
            RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK                     = 21,
            RETRO_ENVIRONMENT_SET_AUDIO_CALLBACK                          = 22,
            RETRO_ENVIRONMENT_GET_RUMBLE_INTERFACE                        = 23,
            RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES               = 24,
            RETRO_ENVIRONMENT_GET_SENSOR_INTERFACE                        = 25 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_CAMERA_INTERFACE                        = 26 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_LOG_INTERFACE                           = 27,
            RETRO_ENVIRONMENT_GET_PERF_INTERFACE                          = 28,
            RETRO_ENVIRONMENT_GET_LOCATION_INTERFACE                      = 29,
            RETRO_ENVIRONMENT_GET_CONTENT_DIRECTORY                       = 30,
            RETRO_ENVIRONMENT_GET_CORE_ASSETS_DIRECTORY                   = 30,
            RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY                          = 31,
            RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO                          = 32,
            RETRO_ENVIRONMENT_SET_PROC_ADDRESS_CALLBACK                   = 33,
            RETRO_ENVIRONMENT_SET_SUBSYSTEM_INFO                          = 34,
            RETRO_ENVIRONMENT_SET_CONTROLLER_INFO                         = 35,
            RETRO_ENVIRONMENT_SET_MEMORY_MAPS                             = 36 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_SET_GEOMETRY                                = 37,
            RETRO_ENVIRONMENT_GET_USERNAME                                = 38,
            RETRO_ENVIRONMENT_GET_LANGUAGE                                = 39,
            RETRO_ENVIRONMENT_GET_CURRENT_SOFTWARE_FRAMEBUFFER            = 40 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE                     = 41 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_SET_SUPPORT_ACHIEVEMENTS                    = 42 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE = 43 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_SET_SERIALIZATION_QUIRKS                    = 44,
            RETRO_ENVIRONMENT_SET_HW_SHARED_CONTEXT                       = 44 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_VFS_INTERFACE                           = 45 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_LED_INTERFACE                           = 46 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_AUDIO_VIDEO_ENABLE                      = 47 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_MIDI_INTERFACE                          = 48 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_FASTFORWARDING                          = 49 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_TARGET_REFRESH_RATE                     = 50 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_INPUT_BITMASKS                          = 51 | RETRO_ENVIRONMENT_EXPERIMENTAL,
            RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION                    = 52,
            RETRO_ENVIRONMENT_SET_CORE_OPTIONS                            = 53,
            RETRO_ENVIRONMENT_SET_CORE_OPTIONS_INTL                       = 54,
            RETRO_ENVIRONMENT_SET_CORE_OPTIONS_DISPLAY                    = 55,
            RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER                     = 56,
            RETRO_ENVIRONMENT_GET_DISK_CONTROL_INTERFACE_VERSION          = 57,
            RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE              = 58,
            RETRO_ENVIRONMENT_GET_MESSAGE_INTERFACE_VERSION               = 59,
            RETRO_ENVIRONMENT_SET_MESSAGE_EXT                             = 60,
            RETRO_ENVIRONMENT_GET_INPUT_MAX_USERS                         = 61,

            // RetroArch Extensions
            RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND                = 2 | RETRO_ENVIRONMENT_RETROARCH_START_BLOCK,
            RETRO_ENVIRONMENT_GET_CLEAR_ALL_THREAD_WAITS_CB               = 3 | RETRO_ENVIRONMENT_RETROARCH_START_BLOCK,
            RETRO_ENVIRONMENT_POLL_TYPE_OVERRIDE                          = 4 | RETRO_ENVIRONMENT_RETROARCH_START_BLOCK
        }

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

        // typedef const char *(RETRO_CALLCONV *retro_vfs_get_path_t)(struct retro_vfs_file_handle *stream);
        public delegate string retro_vfs_get_path_t(IntPtr stream);
        // typedef struct retro_vfs_file_handle *(RETRO_CALLCONV *retro_vfs_open_t)(const char* path, unsigned mode, unsigned hints);
        public delegate IntPtr retro_vfs_open_t(string path, uint mode, uint hints);
        // typedef int (RETRO_CALLCONV *retro_vfs_close_t) (struct retro_vfs_file_handle *stream);
        public delegate int retro_vfs_close_t(IntPtr stream);
        // typedef int64_t (RETRO_CALLCONV *retro_vfs_size_t)(struct retro_vfs_file_handle *stream);
        public delegate int64_t retro_vfs_size_t(IntPtr stream);
        // typedef int64_t (RETRO_CALLCONV *retro_vfs_truncate_t)(struct retro_vfs_file_handle *stream, int64_t length);
        public delegate int64_t retro_vfs_truncate_t(IntPtr stream, int64_t length);
        // typedef int64_t (RETRO_CALLCONV *retro_vfs_tell_t)(struct retro_vfs_file_handle *stream);
        public delegate int64_t retro_vfs_tell_t(IntPtr stream);
        // typedef int64_t (RETRO_CALLCONV *retro_vfs_seek_t)(struct retro_vfs_file_handle *stream, int64_t offset, int seek_position);
        public delegate int64_t retro_vfs_seek_t(IntPtr stream, int64_t offset, int seek_position);
        // typedef int64_t (RETRO_CALLCONV *retro_vfs_read_t)(struct retro_vfs_file_handle *stream, void* s, uint64_t len);
        public delegate int64_t retro_vfs_read_t(IntPtr stream, IntPtr s, int64_t len);
        // typedef int64_t (RETRO_CALLCONV *retro_vfs_write_t)(struct retro_vfs_file_handle *stream, const void* s, uint64_t len);
        public delegate int64_t retro_vfs_write_t(IntPtr stream, IntPtr s, int64_t len);
        // typedef int (RETRO_CALLCONV *retro_vfs_flush_t)(struct retro_vfs_file_handle *stream);
        public delegate int retro_vfs_flush_t(IntPtr stream);
        // typedef int (RETRO_CALLCONV *retro_vfs_remove_t)(const char* path);
        public delegate int retro_vfs_remove_t(string path);
        // typedef int (RETRO_CALLCONV *retro_vfs_rename_t)(const char* old_path, const char* new_path);
        public delegate int retro_vfs_rename_t(string old_path, string new_path);
        // typedef int (RETRO_CALLCONV *retro_vfs_stat_t)(const char* path, int32_t *size);
        public delegate int retro_vfs_stat_t(string path, ref int32_t size);
        // typedef int (RETRO_CALLCONV *retro_vfs_mkdir_t)(const char* dir);
        public delegate int retro_vfs_mkdir_t(string dir);
        // typedef struct retro_vfs_dir_handle *(RETRO_CALLCONV *retro_vfs_opendir_t)(const char* dir, bool include_hidden);
        public delegate IntPtr retro_vfs_opendir_t(string dir, bool include_hidden);
        // typedef bool (RETRO_CALLCONV *retro_vfs_readdir_t)(struct retro_vfs_dir_handle *dirstream);
        public delegate bool retro_vfs_readdir_t(IntPtr dirstream);
        // typedef const char*(RETRO_CALLCONV *retro_vfs_dirent_get_name_t)(struct retro_vfs_dir_handle *dirstream);
        public delegate string retro_vfs_dirent_get_name_t(IntPtr dirstream);
        // typedef bool (RETRO_CALLCONV *retro_vfs_dirent_is_dir_t)(struct retro_vfs_dir_handle *dirstream);
        public delegate bool retro_vfs_dirent_is_dir_t(IntPtr dirstream);
        // typedef int (RETRO_CALLCONV *retro_vfs_closedir_t)(struct retro_vfs_dir_handle *dirstream);
        public delegate int retro_vfs_closedir_t(IntPtr dirstream);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_vfs_interface
        {
            public retro_vfs_get_path_t get_path;
            public retro_vfs_open_t open;
            public retro_vfs_close_t close;
            public retro_vfs_size_t size;
            public retro_vfs_tell_t tell;
            public retro_vfs_seek_t seek;
            public retro_vfs_read_t read;
            public retro_vfs_write_t write;
            public retro_vfs_flush_t flush;
            public retro_vfs_remove_t remove;
            public retro_vfs_rename_t rename;

            public retro_vfs_truncate_t truncate;

            public retro_vfs_stat_t stat;
            public retro_vfs_mkdir_t mkdir;
            public retro_vfs_opendir_t opendir;
            public retro_vfs_readdir_t readdir;
            public retro_vfs_dirent_get_name_t dirent_get_name;
            public retro_vfs_dirent_is_dir_t dirent_is_dir;
            public retro_vfs_closedir_t closedir;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_vfs_interface_info
        {
            public uint32_t required_interface_version;
            public IntPtr iface; // retro_vfs_interface*
        }

        public enum retro_hw_render_interface_type : uint
        {
            RETRO_HW_RENDER_INTERFACE_VULKAN    = 0,
            RETRO_HW_RENDER_INTERFACE_D3D9      = 1,
            RETRO_HW_RENDER_INTERFACE_D3D10     = 2,
            RETRO_HW_RENDER_INTERFACE_D3D11     = 3,
            RETRO_HW_RENDER_INTERFACE_D3D12     = 4,
            RETRO_HW_RENDER_INTERFACE_GSKIT_PS2 = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_hw_render_interface
        {
            public retro_hw_render_interface_type interface_type;
            public uint interface_version;
        };

        // typedef void (RETRO_CALLCONV *retro_set_led_state_t)(int led, int state);
        public delegate void retro_set_led_state_t(int led, int state);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_led_interface
        {
            public retro_set_led_state_t set_led_state;
        }

        // typedef bool (RETRO_CALLCONV *retro_midi_input_enabled_t)(void);
        public delegate bool retro_midi_input_enabled_t();
        // typedef bool (RETRO_CALLCONV *retro_midi_output_enabled_t)(void);
        public delegate bool retro_midi_output_enabled_t();
        // typedef bool (RETRO_CALLCONV *retro_midi_read_t)(uint8_t*byte);
        public delegate bool retro_midi_read_t(IntPtr byte_);
        // typedef bool (RETRO_CALLCONV *retro_midi_write_t)(uint8_t byte, uint32_t delta_time);
        public delegate bool retro_midi_write_t(uint8_t byte_, uint32_t delta_time);
        // typedef bool (RETRO_CALLCONV *retro_midi_flush_t) (void);
        public delegate bool retro_midi_flush_t();

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_midi_interface
        {
            public retro_midi_input_enabled_t input_enabled;
            public retro_midi_output_enabled_t output_enabled;
            public retro_midi_read_t read;
            public retro_midi_write_t write;
            public retro_midi_flush_t flush;
        }

        public enum retro_hw_render_context_negotiation_interface_type
        {
            RETRO_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE_VULKAN = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_hw_render_context_negotiation_interface
        {
            public retro_hw_render_context_negotiation_interface_type interface_type;
            public uint interface_version;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_memory_descriptor
        {
            public uint64_t flags;
            public void* ptr;
            public size_t offset;
            public size_t start;
            public size_t select;
            public size_t disconnect;
            public size_t len;
            public char* addrspace;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_memory_map
        {
            public retro_memory_descriptor* descriptors;
            public uint num_descriptors;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_controller_description
        {
            public char* desc;
            public uint id;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_controller_info
        {
            public retro_controller_description* types;
            public uint num_types;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_subsystem_memory_info
        {
            public char* extension;
            public uint type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_subsystem_rom_info
        {
            public char* desc;
            public char* valid_extensions;
            public bool need_fullpath;
            public bool block_extract;
            public bool required;
            public retro_subsystem_memory_info* memory;
            public uint num_memory;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_subsystem_info
        {
            public char* desc;
            public char* ident;
            public retro_subsystem_rom_info* roms;
            public uint num_roms;
            public uint id;
        }

        // typedef retro_proc_address_t (RETRO_CALLCONV *retro_get_proc_address_t)(const char* sym);
        public delegate IntPtr retro_get_proc_address_t(char* sym);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_get_proc_address_interface
        {
            public retro_get_proc_address_t get_proc_address;
        }

        public enum retro_log_level
        {
            RETRO_LOG_DEBUG = 0,
            RETRO_LOG_INFO  = 1,
            RETRO_LOG_WARN  = 2,
            RETRO_LOG_ERROR = 3
        }

        // typedef void (RETRO_CALLCONV* retro_log_printf_t)(enum retro_log_level level, const char* fmt, ...);
        public delegate void retro_log_printf_t(retro_log_level level, [In, MarshalAs(UnmanagedType.LPStr)] string format, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_log_callback
        {
            public IntPtr log; // retro_log_printf_t
        }

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

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_perf_counter
        {
            public char* ident;
            public retro_perf_tick_t start;
            public retro_perf_tick_t total;
            public retro_perf_tick_t call_cnt;
            public bool registered;
        }

        // typedef retro_time_t (RETRO_CALLCONV *retro_perf_get_time_usec_t)(void);
        public delegate retro_time_t retro_perf_get_time_usec_t();
        // typedef retro_perf_tick_t (RETRO_CALLCONV *retro_perf_get_counter_t)(void);
        public delegate retro_perf_tick_t retro_perf_get_counter_t();
        // typedef uint64_t (RETRO_CALLCONV *retro_get_cpu_features_t)(void);
        public delegate uint64_t retro_get_cpu_features_t();
        // typedef void (RETRO_CALLCONV *retro_perf_log_t) (void);
        public delegate void retro_perf_log_t();
        // typedef void (RETRO_CALLCONV *retro_perf_register_t)(struct retro_perf_counter *counter);
        public delegate void retro_perf_register_t(ref retro_perf_counter counter);
        // typedef void (RETRO_CALLCONV *retro_perf_start_t)(struct retro_perf_counter *counter);
        public delegate void retro_perf_start_t(ref retro_perf_counter counter);
        // typedef void (RETRO_CALLCONV *retro_perf_stop_t)(struct retro_perf_counter *counter);
        public delegate void retro_perf_stop_t(ref retro_perf_counter counter);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_perf_callback
        {
            public retro_perf_get_time_usec_t get_time_usec;
            public retro_get_cpu_features_t get_cpu_features;
            public retro_perf_get_counter_t get_perf_counter;
            public retro_perf_register_t perf_register;
            public retro_perf_start_t perf_start;
            public retro_perf_stop_t perf_stop;
            public retro_perf_log_t perf_log;
        }

        public enum retro_sensor_action : int
        {
            RETRO_SENSOR_ACCELEROMETER_ENABLE = 0,
            RETRO_SENSOR_ACCELEROMETER_DISABLE,
            RETRO_SENSOR_GYROSCOPE_ENABLE,
            RETRO_SENSOR_GYROSCOPE_DISABLE,
            RETRO_SENSOR_ILLUMINANCE_ENABLE,
            RETRO_SENSOR_ILLUMINANCE_DISABLE
        }

        public const int RETRO_SENSOR_ACCELEROMETER_X = 0;
        public const int RETRO_SENSOR_ACCELEROMETER_Y = 1;
        public const int RETRO_SENSOR_ACCELEROMETER_Z = 2;
        public const int RETRO_SENSOR_GYROSCOPE_X     = 3;
        public const int RETRO_SENSOR_GYROSCOPE_Y     = 4;
        public const int RETRO_SENSOR_GYROSCOPE_Z     = 5;
        public const int RETRO_SENSOR_ILLUMINANCE     = 6;

        // typedef bool (RETRO_CALLCONV* retro_set_sensor_state_t) (unsigned port, enum retro_sensor_action action, unsigned rate);
        public delegate bool retro_set_sensor_state_t(uint port, retro_sensor_action action, uint rate);
        // typedef float (RETRO_CALLCONV* retro_sensor_get_input_t) (unsigned port, unsigned id);
        public delegate float retro_sensor_get_input_t(uint port, uint id);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_sensor_interface
        {
            public retro_set_sensor_state_t set_sensor_state;
            public retro_sensor_get_input_t get_sensor_input;
        }

        public enum retro_camera_buffer : int
        {
            RETRO_CAMERA_BUFFER_OPENGL_TEXTURE = 0,
            RETRO_CAMERA_BUFFER_RAW_FRAMEBUFFER
        }

        // typedef bool (RETRO_CALLCONV *retro_camera_start_t) (void);
        public delegate bool retro_camera_start_t();
        // typedef void (RETRO_CALLCONV *retro_camera_stop_t) (void);
        public delegate void retro_camera_stop_t();
        // typedef void (RETRO_CALLCONV *retro_camera_lifetime_status_t) (void);
        public delegate void retro_camera_lifetime_status_t();
        // typedef void (RETRO_CALLCONV *retro_camera_frame_raw_framebuffer_t) (const uint32_t* buffer, unsigned width, unsigned height, size_t pitch);
        public delegate void retro_camera_frame_raw_framebuffer_t(UIntPtr buffer, uint width, uint height, size_t pitch);
        // typedef void (RETRO_CALLCONV *retro_camera_frame_opengl_texture_t) (unsigned texture_id, unsigned texture_target, const float* affine);
        public delegate void retro_camera_frame_opengl_texture_t(uint texture_id, uint texture_target, ref float affine);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_camera_callback
        {
            public uint64_t caps;
            public uint width;
            public uint height;
            public retro_camera_start_t start;
            public retro_camera_stop_t stop;
            public retro_camera_frame_raw_framebuffer_t frame_raw_framebuffer;
            public retro_camera_frame_opengl_texture_t frame_opengl_texture;
            public retro_camera_lifetime_status_t initialized;
            public retro_camera_lifetime_status_t deinitialized;
        }

        // typedef void (RETRO_CALLCONV *retro_location_set_interval_t) (unsigned interval_ms, unsigned interval_distance);
        public delegate void retro_location_set_interval_t(uint interval_ms, uint interval_distance);
        // typedef bool (RETRO_CALLCONV *retro_location_start_t) (void);
        public delegate bool retro_location_start_t();
        // typedef void (RETRO_CALLCONV *retro_location_stop_t) (void);
        public delegate void retro_location_stop_t();
        // typedef bool (RETRO_CALLCONV *retro_location_get_position_t) (double* lat, double* lon, double* horiz_accuracy, double* vert_accuracy);
        public delegate bool retro_location_get_position_t(ref double lat, ref double lon, ref double horiz_accuracy, ref double vert_accuracy);
        // typedef void (RETRO_CALLCONV *retro_location_lifetime_status_t) (void);
        public delegate void retro_location_lifetime_status_t();

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_location_callback
        {
            public retro_location_start_t start;
            public retro_location_stop_t stop;
            public retro_location_get_position_t get_position;
            public retro_location_set_interval_t set_interval;

            public retro_location_lifetime_status_t initialized;
            public retro_location_lifetime_status_t deinitialized;
        }

        public enum retro_rumble_effect
        {
            RETRO_RUMBLE_STRONG = 0,
            RETRO_RUMBLE_WEAK   = 1
        }

        // typedef bool (RETRO_CALLCONV *retro_set_rumble_state_t)(unsigned port, enum retro_rumble_effect effect, uint16_t strength);
        public delegate bool retro_set_rumble_state_t(uint port, retro_rumble_effect effect, uint16_t strength);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_rumble_interface
        {
            public retro_set_rumble_state_t set_rumble_state;
        }

        // typedef void (RETRO_CALLCONV *retro_audio_callback_t)(void);
        public delegate void retro_audio_callback_t();
        // typedef void (RETRO_CALLCONV *retro_audio_set_state_callback_t)(bool enabled);
        public delegate void retro_audio_set_state_callback_t(bool enabled);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_audio_callback
        {
            public retro_audio_callback_t callback;
            public retro_audio_set_state_callback_t set_state;
        }

        // typedef void (RETRO_CALLCONV *retro_frame_time_callback_t)(retro_usec_t usec);
        public delegate void retro_frame_time_callback_t(retro_usec_t usec);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_frame_time_callback
        {
            public IntPtr callback; // retro_frame_time_callback_t
            public retro_usec_t reference;
        }

        public const int RETRO_HW_FRAME_BUFFER_VALID = -1;

        // typedef void (RETRO_CALLCONV *retro_hw_context_reset_t)(void);
        public delegate void retro_hw_context_reset_t();
        // typedef uintptr_t (RETRO_CALLCONV *retro_hw_get_current_framebuffer_t)(void);
        public delegate uintptr_t retro_hw_get_current_framebuffer_t();
        // typedef retro_proc_address_t (RETRO_CALLCONV *retro_hw_get_proc_address_t)(const char* sym);
        public delegate retro_proc_address_t retro_hw_get_proc_address_t(string sym);

        public enum retro_hw_context_type : int
        {
            RETRO_HW_CONTEXT_NONE             = 0,
            RETRO_HW_CONTEXT_OPENGL           = 1,
            RETRO_HW_CONTEXT_OPENGLES2        = 2,
            RETRO_HW_CONTEXT_OPENGL_CORE      = 3,
            RETRO_HW_CONTEXT_OPENGLES3        = 4,
            RETRO_HW_CONTEXT_OPENGLES_VERSION = 5,
            RETRO_HW_CONTEXT_VULKAN           = 6,
            RETRO_HW_CONTEXT_DIRECT3D         = 7
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_hw_render_callback
        {
            public retro_hw_context_type context_type;
            public IntPtr context_reset; // retro_hw_context_reset_t
            public IntPtr get_current_framebuffer; // retro_hw_get_current_framebuffer_t
            public IntPtr get_proc_address; // retro_hw_get_proc_address_t
            public bool depth;
            public bool stencil;
            public bool bottom_left_origin;
            public uint version_major;
            public uint version_minor;
            public bool cache_context;
            public IntPtr context_destroy; // retro_hw_context_reset_t
            public bool debug_context;
        }

        // typedef void (RETRO_CALLCONV *retro_keyboard_event_t)(bool down, unsigned keycode, uint32_t character, uint16_t key_modifiers);
        public delegate void retro_keyboard_event_t(bool down, uint keycode, uint32_t character, uint16_t key_modifiers);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_keyboard_callback
        {
            public retro_keyboard_event_t callback;
        }

        //typedef bool (RETRO_CALLCONV *retro_set_eject_state_t)(bool ejected);
        public delegate bool retro_set_eject_state_t(bool ejected);
        //typedef bool (RETRO_CALLCONV *retro_get_eject_state_t)(void);
        public delegate bool retro_get_eject_state_t();
        //typedef unsigned (RETRO_CALLCONV *retro_get_image_index_t)(void);
        public delegate uint retro_get_image_index_t();
        //typedef bool (RETRO_CALLCONV *retro_set_image_index_t)(unsigned index);
        public delegate bool retro_set_image_index_t(uint index);
        //typedef unsigned (RETRO_CALLCONV *retro_get_num_images_t)(void);
        public delegate uint retro_get_num_images_t();

        //typedef bool (RETRO_CALLCONV *retro_replace_image_index_t)(unsigned index, const struct retro_game_info *info);
        public delegate bool retro_replace_image_index_t(uint index, ref retro_game_info info);
        //typedef bool (RETRO_CALLCONV *retro_add_image_index_t)(void);
        public delegate bool retro_add_image_index_t();
        //typedef bool (RETRO_CALLCONV *retro_set_initial_image_t)(unsigned index, const char *path);
        public delegate bool retro_set_initial_image_t(uint index, char* path);
        //typedef bool (RETRO_CALLCONV *retro_get_image_path_t)(unsigned index, char *path, size_t len);
        public delegate bool retro_get_image_path_t(uint index, char* path, size_t len);
        //typedef bool (RETRO_CALLCONV *retro_get_image_label_t)(unsigned index, char *label, size_t len);
        public delegate bool retro_get_image_label_t(uint index, char* label, size_t len);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_disk_control_callback
        {
            public retro_set_eject_state_t set_eject_state;
            public retro_get_eject_state_t get_eject_state;
            public retro_get_image_index_t get_image_index;
            public retro_set_image_index_t set_image_index;
            public retro_get_num_images_t get_num_images;
            public retro_replace_image_index_t replace_image_index;
            public retro_add_image_index_t add_image_index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_disk_control_ext_callback
        {
            public retro_set_eject_state_t set_eject_state;
            public retro_get_eject_state_t get_eject_state;
            public retro_get_image_index_t get_image_index;
            public retro_set_image_index_t set_image_index;
            public retro_get_num_images_t get_num_images;
            public retro_replace_image_index_t replace_image_index;
            public retro_add_image_index_t add_image_index;
            public retro_set_initial_image_t set_initial_image;
            public retro_get_image_path_t get_image_path;
            public retro_get_image_label_t get_image_label;
        }

        public enum retro_pixel_format : int
        {
            RETRO_PIXEL_FORMAT_0RGB1555 = 0,
            RETRO_PIXEL_FORMAT_XRGB8888 = 1,
            RETRO_PIXEL_FORMAT_RGB565   = 2
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_message
        {
            public char* msg;
            public uint frames;
        }

        public enum retro_message_target
        {
            RETRO_MESSAGE_TARGET_ALL = 0,
            RETRO_MESSAGE_TARGET_OSD,
            RETRO_MESSAGE_TARGET_LOG
        }

        public enum retro_message_type
        {
            RETRO_MESSAGE_TYPE_NOTIFICATION = 0,
            RETRO_MESSAGE_TYPE_NOTIFICATION_ALT,
            RETRO_MESSAGE_TYPE_STATUS,
            RETRO_MESSAGE_TYPE_PROGRESS
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_message_ext
        {
            public char* msg;
            public uint duration;
            public uint priority;
            public retro_log_level level;
            public retro_message_target target;
            public retro_message_type type;
            public int8_t progress;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_input_descriptor
        {
            public uint port;
            public uint device;
            public uint index;
            public uint id;
            public char* desc;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_system_info
        {
            public char* library_name;
            public char* library_version;
            public char* valid_extensions;
            public bool need_fullpath;
            public bool block_extract;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_game_geometry
        {
            public uint base_width;
            public uint base_height;
            public uint max_width;
            public uint max_height;
            public float aspect_ratio;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_system_timing
        {
            public double fps;
            public double sample_rate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_system_av_info
        {
            public retro_game_geometry geometry;
            public retro_system_timing timing;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_variable
        {
            public char* key;
            public char* value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_core_option_display
        {
            public char* key;
            public bool visible;
        }

        public const int RETRO_NUM_CORE_OPTION_VALUES_MAX = 128;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_core_option_value
        {
            public char* value;
            public char* label;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_core_option_definition
        {
            public char* key;
            public char* desc;
            public char* info;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = RETRO_NUM_CORE_OPTION_VALUES_MAX)]
            public retro_core_option_value[] values; // retro_core_option_value[RETRO_NUM_CORE_OPTION_VALUES_MAX]
            public char* default_value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_core_options_intl
        {
            public IntPtr us;    // retro_core_option_definition*
            public IntPtr local; // retro_core_option_definition*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct retro_game_info
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string path; // const char*
            public IntPtr data; // const void*
            public size_t size;
            [MarshalAs(UnmanagedType.LPStr)]
            public string meta; // const char*
        }

        public const int RETRO_MEMORY_ACCESS_WRITE = 1 << 0;
        public const int RETRO_MEMORY_ACCESS_READ  = 1 << 1;
        public const int RETRO_MEMORY_TYPE_CACHED  = 1 << 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_framebuffer
        {
            public void* data;
            public uint width;
            public uint height;
            public size_t pitch;
            public retro_pixel_format format;
            public uint access_flags;
            public uint memory_flags;
        }

        // typedef bool (RETRO_CALLCONV *retro_environment_t)(unsigned cmd, void *data);
        public delegate bool retro_environment_t(retro_environment cmd, void* data);

        // typedef void (RETRO_CALLCONV *retro_video_refresh_t)(const void *data, unsigned width, unsigned height, size_t pitch);
        public delegate void retro_video_refresh_t(void* data, uint width, uint height, size_t pitch);

        // typedef void (RETRO_CALLCONV *retro_audio_sample_t)(int16_t left, int16_t right);
        public delegate void retro_audio_sample_t(int16_t left, int16_t right);

        // typedef size_t (RETRO_CALLCONV *retro_audio_sample_batch_t)(const int16_t *data, size_t frames);
        public delegate size_t retro_audio_sample_batch_t(int16_t* data, size_t frames);

        // typedef void (RETRO_CALLCONV *retro_input_poll_t)(void);
        public delegate void retro_input_poll_t();

        // typedef int16_t (RETRO_CALLCONV* retro_input_state_t)(unsigned port, unsigned device, unsigned index, unsigned id);
        public delegate int16_t retro_input_state_t(uint port, uint device, uint index, uint id);

        // RETRO_API void retro_set_environment(retro_environment_t);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_set_environment_t(retro_environment_t cb);

        // RETRO_API void retro_set_video_refresh(retro_video_refresh_t);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_set_video_refresh_t(retro_video_refresh_t cb);

        // RETRO_API void retro_set_audio_sample(retro_audio_sample_t);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_set_audio_sample_t(retro_audio_sample_t cb);

        // RETRO_API void retro_set_audio_sample_batch(retro_audio_sample_batch_t);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_set_audio_sample_batch_t(retro_audio_sample_batch_t cb);

        // RETRO_API void retro_set_input_poll(retro_input_poll_t);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_set_input_poll_t(retro_input_poll_t cb);

        // RETRO_API void retro_set_input_state(retro_input_state_t);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_set_input_state_t(retro_input_state_t cb);

        // RETRO_API void retro_init(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_init_t();

        // RETRO_API void retro_deinit(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_deinit_t();

        // RETRO_API unsigned retro_api_version(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint retro_api_version_t();

        // RETRO_API void retro_get_system_info(struct retro_system_info *info);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_get_system_info_t(out retro_system_info info);

        // RETRO_API void retro_get_system_av_info(struct retro_system_av_info *info);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_get_system_av_info_t(out retro_system_av_info info);

        // RETRO_API void retro_set_controller_port_device(unsigned port, unsigned device);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_set_controller_port_device_t(uint port, uint device);

        // RETRO_API void retro_reset(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_reset_t();

        // RETRO_API void retro_run(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_run_t();

        // RETRO_API size_t retro_serialize_size(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate size_t retro_serialize_size_t();

        // RETRO_API bool retro_serialize(void* data, size_t size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool retro_serialize_t(void* data, size_t size);

        // RETRO_API bool retro_unserialize(const void* data, size_t size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool retro_unserialize_t(void* data, size_t size);

        // RETRO_API void retro_cheat_reset(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_cheat_reset_t();

        // RETRO_API void retro_cheat_set(unsigned index, bool enabled, const char* code);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_cheat_set_t(uint index, bool enabled, char* code);

        // RETRO_API bool retro_load_game(const struct retro_game_info *game);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool retro_load_game_t(ref retro_game_info game);

        // RETRO_API bool retro_load_game_special(unsigned game_type, const struct retro_game_info *info, size_t num_info);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool retro_load_game_special_t(uint game_type, ref retro_game_info info, size_t num_info);

        // RETRO_API void retro_unload_game(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_unload_game_t();

        // RETRO_API unsigned retro_get_region(void);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint retro_get_region_t();

        // RETRO_API void* retro_get_memory_data(unsigned id);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* retro_get_memory_data_t(uint id);

        // RETRO_API size_t retro_get_memory_size(unsigned id);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate size_t retro_get_memory_size_t(uint id);
#pragma warning restore IDE1006 // Naming Styles
    }
}
