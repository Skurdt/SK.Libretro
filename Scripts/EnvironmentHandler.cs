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

using SK.Libretro.Header;
using System;

namespace SK.Libretro
{
    internal sealed class EnvironmentHandler
    {
        private static readonly retro_environment_t _callback = EnvironmentCallback;

        public void SetCoreCallback(retro_set_environment_t setEnvironment) => setEnvironment(_callback);

        [MonoPInvokeCallback(typeof(retro_environment_t))]
        private static bool EnvironmentCallback(RETRO_ENVIRONMENT cmd, IntPtr data) => cmd switch
        {
            /************************************************************************************************
            * Frontend to core
            */
            RETRO_ENVIRONMENT.GET_OVERSCAN                                => Wrapper.Instance.GraphicsHandler.GetOverscan(data),
            RETRO_ENVIRONMENT.GET_CAN_DUPE                                => Wrapper.Instance.GraphicsHandler.GetCanDupe(data),
            RETRO_ENVIRONMENT.GET_SYSTEM_DIRECTORY                        => Wrapper.Instance.GetSystemDirectory(data),
            RETRO_ENVIRONMENT.GET_VARIABLE                                => Wrapper.Instance.OptionsHandler.GetVariable(data),
            RETRO_ENVIRONMENT.GET_VARIABLE_UPDATE                         => Wrapper.Instance.OptionsHandler.GetVariableUpdate(data),
            RETRO_ENVIRONMENT.GET_LIBRETRO_PATH                           => Wrapper.Instance.GetLibretroPath(data),
            RETRO_ENVIRONMENT.GET_RUMBLE_INTERFACE                        => Wrapper.Instance.InputHandler.GetRumbleInterface(data),
            RETRO_ENVIRONMENT.GET_INPUT_DEVICE_CAPABILITIES               => Wrapper.Instance.InputHandler.GetInputDeviceCapabilities(data),
            RETRO_ENVIRONMENT.GET_SENSOR_INTERFACE                        => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_CAMERA_INTERFACE                        => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_LOG_INTERFACE                           => Wrapper.Instance.LogHandler.GetLogInterface(data),
            RETRO_ENVIRONMENT.GET_PERF_INTERFACE                          => Wrapper.Instance.PerfHandler.GetPerfInterface(data),
            RETRO_ENVIRONMENT.GET_LOCATION_INTERFACE                      => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_CORE_ASSETS_DIRECTORY                   => Wrapper.Instance.GetCoreAssetsDirectory(data),
            RETRO_ENVIRONMENT.GET_SAVE_DIRECTORY                          => Wrapper.Instance.SerializationHandler.GetSaveDirectory(data),
            RETRO_ENVIRONMENT.GET_USERNAME                                => Wrapper.Instance.GetUsername(data),
            RETRO_ENVIRONMENT.GET_LANGUAGE                                => Wrapper.Instance.GetLanguage(data),
            RETRO_ENVIRONMENT.GET_CURRENT_SOFTWARE_FRAMEBUFFER            => Wrapper.Instance.GraphicsHandler.GetCurrentSoftwareFramebuffer(data),
            RETRO_ENVIRONMENT.GET_HW_RENDER_INTERFACE                     => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_VFS_INTERFACE                           => Wrapper.Instance.VFSHandler.GetVfsInterface(data),
            RETRO_ENVIRONMENT.GET_LED_INTERFACE                           => Wrapper.Instance.LedHandler.GetLedInterface(data),
            RETRO_ENVIRONMENT.GET_AUDIO_VIDEO_ENABLE                      => Wrapper.Instance.EnvironmentHandler.GetAudioVideoEnable(data),
            RETRO_ENVIRONMENT.GET_MIDI_INTERFACE                          => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_FASTFORWARDING                          => Wrapper.Instance.EnvironmentHandler.GetFastForwarding(data),
            RETRO_ENVIRONMENT.GET_TARGET_REFRESH_RATE                     => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_INPUT_BITMASKS                          => Wrapper.Instance.InputHandler.GetInputBitmasks(data),
            RETRO_ENVIRONMENT.GET_CORE_OPTIONS_VERSION                    => Wrapper.Instance.OptionsHandler.GetCoreOptionsVersion(data),
            RETRO_ENVIRONMENT.GET_PREFERRED_HW_RENDER                     => Wrapper.Instance.GraphicsHandler.GetPreferredHwRender(data),
            RETRO_ENVIRONMENT.GET_DISK_CONTROL_INTERFACE_VERSION          => Wrapper.Instance.DiskHandler.GetDiskControlInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_MESSAGE_INTERFACE_VERSION               => Wrapper.Instance.MessageHandler.GetMessageInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_INPUT_MAX_USERS                         => Wrapper.Instance.InputHandler.GetInputMaxUsers(data),
            RETRO_ENVIRONMENT.GET_GAME_INFO_EXT                           => Wrapper.Instance.Game.GetGameInfoExt(data),
            RETRO_ENVIRONMENT.GET_THROTTLE_STATE                          => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_SAVESTATE_CONTEXT                       => EnvironmentNotImplemented(cmd),

            /************************************************************************************************
            * Core to frontend
            */
            RETRO_ENVIRONMENT.SET_ROTATION                                => Wrapper.Instance.Game.SetRotation(data),
            RETRO_ENVIRONMENT.SET_MESSAGE                                 => Wrapper.Instance.MessageHandler.SetMessage(data),
            RETRO_ENVIRONMENT.SHUTDOWN                                    => Wrapper.Instance.Shutdown(),
            RETRO_ENVIRONMENT.SET_PERFORMANCE_LEVEL                       => Wrapper.Instance.Core.SetPerformanceLevel(data),
            RETRO_ENVIRONMENT.SET_PIXEL_FORMAT                            => Wrapper.Instance.GraphicsHandler.SetPixelFormat(data),
            RETRO_ENVIRONMENT.SET_INPUT_DESCRIPTORS                       => Wrapper.Instance.InputHandler.SetInputDescriptors(data),
            RETRO_ENVIRONMENT.SET_KEYBOARD_CALLBACK                       => Wrapper.Instance.InputHandler.SetKeyboardCallback(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_INTERFACE                  => Wrapper.Instance.DiskHandler.SetDiskControlInterface(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER                               => Wrapper.Instance.GraphicsHandler.SetHwRender(data),
            RETRO_ENVIRONMENT.SET_VARIABLES                               => Wrapper.Instance.OptionsHandler.SetVariables(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_NO_GAME                         => Wrapper.Instance.Core.SetSupportNoGame(data),
            RETRO_ENVIRONMENT.SET_FRAME_TIME_CALLBACK                     => Wrapper.Instance.EnvironmentHandler.SetFrameTimeCallback(data),
            RETRO_ENVIRONMENT.SET_AUDIO_CALLBACK                          => Wrapper.Instance.AudioHandler.SetAudioCallback(data),
            RETRO_ENVIRONMENT.SET_SYSTEM_AV_INFO                          => Wrapper.Instance.Game.SetSystemAvInfo(data),
            RETRO_ENVIRONMENT.SET_PROC_ADDRESS_CALLBACK                   => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_SUBSYSTEM_INFO                          => Wrapper.Instance.Core.SetSubsystemInfo(data),
            RETRO_ENVIRONMENT.SET_CONTROLLER_INFO                         => Wrapper.Instance.InputHandler.SetControllerInfo(data),
            RETRO_ENVIRONMENT.SET_MEMORY_MAPS                             => Wrapper.Instance.MemoryHandler.SetMemoryMaps(data),
            RETRO_ENVIRONMENT.SET_GEOMETRY                                => Wrapper.Instance.GraphicsHandler.SetGeometry(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_ACHIEVEMENTS                    => Wrapper.Instance.Core.SetSupportAchievements(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_SERIALIZATION_QUIRKS                    => Wrapper.Instance.SerializationHandler.SetSerializationQuirks(data),
            RETRO_ENVIRONMENT.SET_HW_SHARED_CONTEXT                       => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS                            => Wrapper.Instance.OptionsHandler.SetCoreOptions(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_INTL                       => Wrapper.Instance.OptionsHandler.SetCoreOptionsIntl(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_DISPLAY                    => Wrapper.Instance.OptionsHandler.SetCoreOptionsDisplay(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_EXT_INTERFACE              => Wrapper.Instance.DiskHandler.SetDiskControlExtInterface(data),
            RETRO_ENVIRONMENT.SET_MESSAGE_EXT                             => Wrapper.Instance.MessageHandler.SetMessageExt(data),
            RETRO_ENVIRONMENT.SET_AUDIO_BUFFER_STATUS_CALLBACK            => Wrapper.Instance.AudioHandler.SetAudioBufferStatusCallback(data),
            RETRO_ENVIRONMENT.SET_MINIMUM_AUDIO_LATENCY                   => Wrapper.Instance.AudioHandler.SetMinimumAudioLatency(data),
            RETRO_ENVIRONMENT.SET_FASTFORWARDING_OVERRIDE                 => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CONTENT_INFO_OVERRIDE                   => Wrapper.Instance.Game.SetContentInfoOverride(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_V2                         => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_V2_INTL                    => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_UPDATE_DISPLAY_CALLBACK    => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_VARIABLE                                => EnvironmentNotImplemented(cmd),

            _                                                             => EnvironmentUnknown(cmd)
        };

        /************************************************************************************************
        * Frontend to core
        */
        public bool GetAudioVideoEnable(IntPtr data)
        {
            if (data.IsNull())
                return false;

            int bits = 0;
            bits    |= 1; // if video enabled
            bits    |= 2; // if audio enabled

            data.Write(bits);
            return true;
        }

        public bool GetFastForwarding(IntPtr data)
        {
            if (data.IsNotNull())
                data.Write(false);
            return false;
        }

        /************************************************************************************************
        / Core to frontend
        /***********************************************************************************************/
        public bool SetFrameTimeCallback(IntPtr data)
        {
            if (data.IsNull())
                return false;

            Wrapper.Instance.LogHandler.LogInfo("Using FrameTime Callback", nameof(RETRO_ENVIRONMENT.SET_FRAME_TIME_CALLBACK));
            Wrapper.Instance.FrameTimeInterface = data.ToStructure<retro_frame_time_callback>();
            return true;
        }

        /************************************************************************************************
        / Utilities
        /***********************************************************************************************/
        private static bool EnvironmentNotImplemented(RETRO_ENVIRONMENT cmd, bool log = true, bool defaultReturns = false)
        {
            if (!log)
                return defaultReturns;

            if (defaultReturns)
                Wrapper.Instance.LogHandler.LogWarning("Environment not implemented!", cmd.ToString());
            else
                Wrapper.Instance.LogHandler.LogError("Environment not implemented!", cmd.ToString());
            return defaultReturns;
        }

        private static bool EnvironmentUnknown(RETRO_ENVIRONMENT cmd)
        {
            Wrapper.Instance.LogHandler.LogError("Environment unknown!", cmd.ToString());
            return false;
        }
    }
}
