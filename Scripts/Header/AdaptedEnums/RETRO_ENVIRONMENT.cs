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

namespace SK.Libretro.Header
{
    internal enum RETRO_ENVIRONMENT
    {
        EXPERIMENTAL                                = 0x10000,
        PRIVATE                                     = 0x20000,

        SET_ROTATION                                = 1,
        GET_OVERSCAN                                = 2,
        GET_CAN_DUPE                                = 3,
        SET_MESSAGE                                 = 6,
        SHUTDOWN                                    = 7,
        SET_PERFORMANCE_LEVEL                       = 8,
        GET_SYSTEM_DIRECTORY                        = 9,
        SET_PIXEL_FORMAT                            = 10,
        SET_INPUT_DESCRIPTORS                       = 11,
        SET_KEYBOARD_CALLBACK                       = 12,
        SET_DISK_CONTROL_INTERFACE                  = 13,
        SET_HW_RENDER                               = 14,
        GET_VARIABLE                                = 15,
        SET_VARIABLES                               = 16,
        GET_VARIABLE_UPDATE                         = 17,
        SET_SUPPORT_NO_GAME                         = 18,
        GET_LIBRETRO_PATH                           = 19,
        SET_FRAME_TIME_CALLBACK                     = 21,
        SET_AUDIO_CALLBACK                          = 22,
        GET_RUMBLE_INTERFACE                        = 23,
        GET_INPUT_DEVICE_CAPABILITIES               = 24,
        GET_SENSOR_INTERFACE                        = 25 | EXPERIMENTAL,
        GET_CAMERA_INTERFACE                        = 26 | EXPERIMENTAL,
        GET_LOG_INTERFACE                           = 27,
        GET_PERF_INTERFACE                          = 28,
        GET_LOCATION_INTERFACE                      = 29,
        GET_CONTENT_DIRECTORY                       = 30,
        GET_CORE_ASSETS_DIRECTORY                   = 30,
        GET_SAVE_DIRECTORY                          = 31,
        SET_SYSTEM_AV_INFO                          = 32,
        SET_PROC_ADDRESS_CALLBACK                   = 33,
        SET_SUBSYSTEM_INFO                          = 34,
        SET_CONTROLLER_INFO                         = 35,
        SET_MEMORY_MAPS                             = 36 | EXPERIMENTAL,
        SET_GEOMETRY                                = 37,
        GET_USERNAME                                = 38,
        GET_LANGUAGE                                = 39,
        GET_CURRENT_SOFTWARE_FRAMEBUFFER            = 40 | EXPERIMENTAL,
        GET_HW_RENDER_INTERFACE                     = 41 | EXPERIMENTAL,
        SET_SUPPORT_ACHIEVEMENTS                    = 42 | EXPERIMENTAL,
        SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE = 43 | EXPERIMENTAL,
        SET_SERIALIZATION_QUIRKS                    = 44,
        SET_HW_SHARED_CONTEXT                       = 44 | EXPERIMENTAL,
        GET_VFS_INTERFACE                           = 45 | EXPERIMENTAL,
        GET_LED_INTERFACE                           = 46 | EXPERIMENTAL,
        GET_AUDIO_VIDEO_ENABLE                      = 47 | EXPERIMENTAL,
        GET_MIDI_INTERFACE                          = 48 | EXPERIMENTAL,
        GET_FASTFORWARDING                          = 49 | EXPERIMENTAL,
        GET_TARGET_REFRESH_RATE                     = 50 | EXPERIMENTAL,
        GET_INPUT_BITMASKS                          = 51 | EXPERIMENTAL,
        GET_CORE_OPTIONS_VERSION                    = 52,
        SET_CORE_OPTIONS                            = 53,
        SET_CORE_OPTIONS_INTL                       = 54,
        SET_CORE_OPTIONS_DISPLAY                    = 55,
        GET_PREFERRED_HW_RENDER                     = 56,
        GET_DISK_CONTROL_INTERFACE_VERSION          = 57,
        SET_DISK_CONTROL_EXT_INTERFACE              = 58,
        GET_MESSAGE_INTERFACE_VERSION               = 59,
        SET_MESSAGE_EXT                             = 60,
        GET_INPUT_MAX_USERS                         = 61,
        SET_AUDIO_BUFFER_STATUS_CALLBACK            = 62,
        SET_MINIMUM_AUDIO_LATENCY                   = 63,
        SET_FASTFORWARDING_OVERRIDE                 = 64,
        SET_CONTENT_INFO_OVERRIDE                   = 65,
        GET_GAME_INFO_EXT                           = 66,
        SET_CORE_OPTIONS_V2                         = 67,
        SET_CORE_OPTIONS_V2_INTL                    = 68,
        SET_CORE_OPTIONS_UPDATE_DISPLAY_CALLBACK    = 69,
        SET_VARIABLE                                = 70,
        GET_THROTTLE_STATE                          = 71 | EXPERIMENTAL,
        GET_SAVESTATE_CONTEXT                       = 72 | EXPERIMENTAL
    }
}
