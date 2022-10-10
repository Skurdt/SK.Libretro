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

namespace SK.Libretro
{
    // NOTE(Tom): Original defines as an enum
    internal enum retro_environment
    {
        RETRO_ENVIRONMENT_EXPERIMENTAL                                = 0x10000,
        RETRO_ENVIRONMENT_PRIVATE                                     = 0x20000,

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
        RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK            = 62,
        RETRO_ENVIRONMENT_SET_MINIMUM_AUDIO_LATENCY                   = 63,
        RETRO_ENVIRONMENT_SET_FASTFORWARDING_OVERRIDE                 = 64,
        RETRO_ENVIRONMENT_SET_CONTENT_INFO_OVERRIDE                   = 65,
        RETRO_ENVIRONMENT_GET_GAME_INFO_EXT                           = 66,
        RETRO_ENVIRONMENT_SET_CORE_OPTIONS_V2                         = 67,
        RETRO_ENVIRONMENT_SET_CORE_OPTIONS_V2_INTL                    = 68,
        RETRO_ENVIRONMENT_SET_CORE_OPTIONS_UPDATE_DISPLAY_CALLBACK    = 69,
        RETRO_ENVIRONMENT_SET_VARIABLE                                = 70,
        RETRO_ENVIRONMENT_GET_THROTTLE_STATE                          = 71 | RETRO_ENVIRONMENT_EXPERIMENTAL,
        RETRO_ENVIRONMENT_GET_SAVESTATE_CONTEXT                       = 72 | RETRO_ENVIRONMENT_EXPERIMENTAL
    }
}
