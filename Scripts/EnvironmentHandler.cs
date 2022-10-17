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
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class EnvironmentHandler
    {
        private readonly Wrapper _wrapper;
        private readonly retro_environment_t _callback;

        public EnvironmentHandler(Wrapper wrapper)
        {
            _callback = EnvironmentCallback;
            _wrapper  = wrapper;
        }

        public void SetCoreCallback(retro_set_environment_t setEnvironment) => setEnvironment(_callback);

        private bool EnvironmentCallback(RETRO_ENVIRONMENT cmd, IntPtr data) => cmd switch
        {
            /************************************************************************************************
             * Frontend to core
             */
            RETRO_ENVIRONMENT.GET_OVERSCAN                                => _wrapper.Graphics.GetOverscan(data),
            RETRO_ENVIRONMENT.GET_CAN_DUPE                                => _wrapper.Graphics.GetCanDupe(data),
            RETRO_ENVIRONMENT.GET_SYSTEM_DIRECTORY                        => _wrapper.GetSystemDirectory(data),
            RETRO_ENVIRONMENT.GET_VARIABLE                                => _wrapper.Options.GetVariable(data),
            RETRO_ENVIRONMENT.GET_VARIABLE_UPDATE                         => _wrapper.Options.GetVariableUpdate(data),
            RETRO_ENVIRONMENT.GET_LIBRETRO_PATH                           => _wrapper.GetLibretroPath(data),
            RETRO_ENVIRONMENT.GET_RUMBLE_INTERFACE                        => _wrapper.Input.GetRumbleInterface(data),
            RETRO_ENVIRONMENT.GET_INPUT_DEVICE_CAPABILITIES               => _wrapper.Input.GetInputDeviceCapabilities(data),
            RETRO_ENVIRONMENT.GET_SENSOR_INTERFACE                        => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_CAMERA_INTERFACE                        => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_LOG_INTERFACE                           => GetLogInterface(data),
            RETRO_ENVIRONMENT.GET_PERF_INTERFACE                          => EnvironmentNotImplemented(cmd)/*_wrapper.Perf.GetPerfInterface(data)*/,
            RETRO_ENVIRONMENT.GET_LOCATION_INTERFACE                      => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_CORE_ASSETS_DIRECTORY                   => _wrapper.GetCoreAssetsDirectory(data),
            RETRO_ENVIRONMENT.GET_SAVE_DIRECTORY                          => _wrapper.GetSaveDirectory(data),
            RETRO_ENVIRONMENT.GET_USERNAME                                => _wrapper.GetUsername(data),
            RETRO_ENVIRONMENT.GET_LANGUAGE                                => _wrapper.GetLanguage(data),
            RETRO_ENVIRONMENT.GET_CURRENT_SOFTWARE_FRAMEBUFFER            => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_HW_RENDER_INTERFACE                     => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_VFS_INTERFACE                           => _wrapper.VFS.GetVfsInterface(data),
            RETRO_ENVIRONMENT.GET_LED_INTERFACE                           => _wrapper.Led.GetLedInterface(data),
            RETRO_ENVIRONMENT.GET_AUDIO_VIDEO_ENABLE                      => GetAudioVideoEnable(data),
            RETRO_ENVIRONMENT.GET_MIDI_INTERFACE                          => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_FASTFORWARDING                          => GetFastForwarding(data),
            RETRO_ENVIRONMENT.GET_TARGET_REFRESH_RATE                     => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_INPUT_BITMASKS                          => _wrapper.Input.GetInputBitmasks(data),
            RETRO_ENVIRONMENT.GET_CORE_OPTIONS_VERSION                    => _wrapper.Options.GetCoreOptionsVersion(data),
            RETRO_ENVIRONMENT.GET_PREFERRED_HW_RENDER                     => _wrapper.Graphics.GetPreferredHwRender(data),
            RETRO_ENVIRONMENT.GET_DISK_CONTROL_INTERFACE_VERSION          => _wrapper.Disk.GetDiskControlInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_MESSAGE_INTERFACE_VERSION               => GetMessageInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_INPUT_MAX_USERS                         => _wrapper.Input.GetInputMaxUsers(data),
            RETRO_ENVIRONMENT.GET_GAME_INFO_EXT                           => _wrapper.Game.GetGameInfoExt(data),
            RETRO_ENVIRONMENT.GET_THROTTLE_STATE                          => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_SAVESTATE_CONTEXT                       => EnvironmentNotImplemented(cmd),

            /************************************************************************************************
             * Core to frontend
             */
            RETRO_ENVIRONMENT.SET_ROTATION                                => _wrapper.Game.SetRotation(data),
            RETRO_ENVIRONMENT.SET_MESSAGE                                 => SetMessage(data),
            RETRO_ENVIRONMENT.SHUTDOWN                                    => _wrapper.Shutdown(),
            RETRO_ENVIRONMENT.SET_PERFORMANCE_LEVEL                       => _wrapper.Core.SetPerformanceLevel(data),
            RETRO_ENVIRONMENT.SET_PIXEL_FORMAT                            => _wrapper.Graphics.SetPixelFormat(data),
            RETRO_ENVIRONMENT.SET_INPUT_DESCRIPTORS                       => _wrapper.Input.SetInputDescriptors(data),
            RETRO_ENVIRONMENT.SET_KEYBOARD_CALLBACK                       => _wrapper.Input.SetKeyboardCallback(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_INTERFACE                  => _wrapper.Disk.SetDiskControlInterface(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER                               => _wrapper.Graphics.SetHwRender(data),
            RETRO_ENVIRONMENT.SET_VARIABLES                               => _wrapper.Options.SetVariables(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_NO_GAME                         => _wrapper.Core.SetSupportNoGame(data),
            RETRO_ENVIRONMENT.SET_FRAME_TIME_CALLBACK                     => SetFrameTimeCallback(data),
            RETRO_ENVIRONMENT.SET_AUDIO_CALLBACK                          => _wrapper.Audio.SetAudioCallback(data),
            RETRO_ENVIRONMENT.SET_SYSTEM_AV_INFO                          => _wrapper.Game.SetSystemAvInfo(data),
            RETRO_ENVIRONMENT.SET_PROC_ADDRESS_CALLBACK                   => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_SUBSYSTEM_INFO                          => _wrapper.Core.SetSubsystemInfo(data),
            RETRO_ENVIRONMENT.SET_CONTROLLER_INFO                         => _wrapper.Input.SetControllerInfo(data),
            RETRO_ENVIRONMENT.SET_MEMORY_MAPS                             => _wrapper.Memory.SetMemoryMaps(data),
            RETRO_ENVIRONMENT.SET_GEOMETRY                                => _wrapper.Graphics.SetGeometry(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_ACHIEVEMENTS                    => _wrapper.Core.SetSupportAchievements(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_SERIALIZATION_QUIRKS                    => _wrapper.Serialization.SetSerializationQuirks(data),
            RETRO_ENVIRONMENT.SET_HW_SHARED_CONTEXT                       => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS                            => _wrapper.Options.SetCoreOptions(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_INTL                       => _wrapper.Options.SetCoreOptionsIntl(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_DISPLAY                    => _wrapper.Options.SetCoreOptionsDisplay(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_EXT_INTERFACE              => _wrapper.Disk.SetDiskControlExtInterface(data),
            RETRO_ENVIRONMENT.SET_MESSAGE_EXT                             => SetMessageExt(data),
            RETRO_ENVIRONMENT.SET_AUDIO_BUFFER_STATUS_CALLBACK            => _wrapper.Audio.SetAudioBufferStatusCallback(data),
            RETRO_ENVIRONMENT.SET_MINIMUM_AUDIO_LATENCY                   => _wrapper.Audio.SetMinimumAudioLatency(data),
            RETRO_ENVIRONMENT.SET_FASTFORWARDING_OVERRIDE                 => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CONTENT_INFO_OVERRIDE                   => _wrapper.Game.SetContentInfoOverride(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_V2                         => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_V2_INTL                    => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_UPDATE_DISPLAY_CALLBACK    => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_VARIABLE                                => EnvironmentNotImplemented(cmd),

            _                                                             => EnvironmentUnknown(cmd)
        };

        /************************************************************************************************
        * Frontend to core
        */
        private bool GetLogInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_log_callback callback = data.ToStructure<retro_log_callback>();
            callback.log = Marshal.GetFunctionPointerForDelegate<retro_log_printf_t>(LogInterface.RetroLogPrintf);
            Marshal.StructureToPtr(callback, data, true);
            return true;
        }

        private bool GetAudioVideoEnable(IntPtr data)
        {
            if (data.IsNull())
                return false;

            int bits = 0;
            bits    |= 1; // if video enabled
            bits    |= 2; // if audio enabled

            data.Write(bits);
            return true;
        }

        private bool GetFastForwarding(IntPtr data)
        {
            if (data.IsNotNull())
                data.Write(false);
            return false;
        }

        private bool GetMessageInterfaceVersion(IntPtr data)
        {
            if (data.IsNotNull())
                data.Write(MessageInterface.VERSION);
            return true;
        }

        /************************************************************************************************
        / Core to frontend
        /***********************************************************************************************/
        private bool SetMessage(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_message retroMessage = data.ToStructure<retro_message>();
            string message = retroMessage.msg.AsString();
            Logger.Instance.LogInfo($"<- Message: {message}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE));
            return true;
        }

        private bool SetFrameTimeCallback(IntPtr data)
        {
            if (data.IsNull())
                return false;

            Logger.Instance.LogInfo("Using FrameTime Callback");
            _wrapper.FrameTimeInterface = data.ToStructure<retro_frame_time_callback>();
            return true;
        }

        private bool SetMessageExt(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_message_ext messageExt = data.ToStructure<retro_message_ext>();
            Logger.Instance.LogInfo($"{messageExt.msg.AsString()}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt.duration}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt.priority}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt.level}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt.target}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt.type}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt.progress}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE_EXT));
            return true;
        }

        private static bool EnvironmentNotImplemented(RETRO_ENVIRONMENT cmd, bool log = true, bool defaultReturns = false)
        {
            if (!log)
                return defaultReturns;

            if (defaultReturns)
                Logger.Instance.LogWarning("Environment not implemented!", cmd.ToString());
            else
                Logger.Instance.LogError("Environment not implemented!", cmd.ToString());
            return defaultReturns;
        }

        private static bool EnvironmentUnknown(RETRO_ENVIRONMENT cmd)
        {
            Logger.Instance.LogError("Environment unknown!", cmd.ToString());
            return false;
        }
    }
}
