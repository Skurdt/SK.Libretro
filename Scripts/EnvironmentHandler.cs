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
using System.Threading;

namespace SK.Libretro
{
    internal sealed class EnvironmentHandler
    {
        private static readonly retro_environment_t _callback = EnvironmentCallback;

        private readonly Wrapper _wrapper;

        public EnvironmentHandler(Wrapper wrapper) => _wrapper = wrapper;

        public void SetCoreCallback(retro_set_environment_t setEnvironment) => setEnvironment(_callback);

        [MonoPInvokeCallback(typeof(retro_environment_t))]
        private static bool EnvironmentCallback(RETRO_ENVIRONMENT cmd, IntPtr data) => Wrapper.TryGetInstance(Thread.CurrentThread, out Wrapper wrapper) && cmd switch
        {
            /************************************************************************************************
            * Frontend to core
            */
            RETRO_ENVIRONMENT.GET_OVERSCAN                                => wrapper.GraphicsHandler.GetOverscan(data),
            RETRO_ENVIRONMENT.GET_CAN_DUPE                                => wrapper.GraphicsHandler.GetCanDupe(data),
            RETRO_ENVIRONMENT.GET_SYSTEM_DIRECTORY                        => wrapper.GetSystemDirectory(data),
            RETRO_ENVIRONMENT.GET_VARIABLE                                => wrapper.OptionsHandler.GetVariable(data),
            RETRO_ENVIRONMENT.GET_VARIABLE_UPDATE                         => wrapper.OptionsHandler.GetVariableUpdate(data),
            RETRO_ENVIRONMENT.GET_LIBRETRO_PATH                           => wrapper.GetLibretroPath(data),
            RETRO_ENVIRONMENT.GET_RUMBLE_INTERFACE                        => wrapper.InputHandler.GetRumbleInterface(data),
            RETRO_ENVIRONMENT.GET_INPUT_DEVICE_CAPABILITIES               => wrapper.InputHandler.GetInputDeviceCapabilities(data),
            RETRO_ENVIRONMENT.GET_SENSOR_INTERFACE                        => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_CAMERA_INTERFACE                        => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_LOG_INTERFACE                           => wrapper.LogHandler.GetLogInterface(data),
            RETRO_ENVIRONMENT.GET_PERF_INTERFACE                          => wrapper.PerfHandler.GetPerfInterface(data),
            RETRO_ENVIRONMENT.GET_LOCATION_INTERFACE                      => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_CORE_ASSETS_DIRECTORY                   => wrapper.GetCoreAssetsDirectory(data),
            RETRO_ENVIRONMENT.GET_SAVE_DIRECTORY                          => wrapper.SerializationHandler.GetSaveDirectory(data),
            RETRO_ENVIRONMENT.GET_USERNAME                                => wrapper.GetUsername(data),
            RETRO_ENVIRONMENT.GET_LANGUAGE                                => wrapper.GetLanguage(data),
            RETRO_ENVIRONMENT.GET_CURRENT_SOFTWARE_FRAMEBUFFER            => wrapper.GraphicsHandler.GetCurrentSoftwareFramebuffer(),
            RETRO_ENVIRONMENT.GET_HW_RENDER_INTERFACE                     => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_VFS_INTERFACE                           => wrapper.VFSHandler.GetVfsInterface(data),
            RETRO_ENVIRONMENT.GET_LED_INTERFACE                           => wrapper.LedHandler.GetLedInterface(data),
            RETRO_ENVIRONMENT.GET_AUDIO_VIDEO_ENABLE                      => wrapper.EnvironmentHandler.GetAudioVideoEnable(data),
            RETRO_ENVIRONMENT.GET_MIDI_INTERFACE                          => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_FASTFORWARDING                          => wrapper.EnvironmentHandler.GetFastForwarding(data),
            RETRO_ENVIRONMENT.GET_TARGET_REFRESH_RATE                     => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_INPUT_BITMASKS                          => wrapper.InputHandler.GetInputBitmasks(data),
            RETRO_ENVIRONMENT.GET_CORE_OPTIONS_VERSION                    => wrapper.OptionsHandler.GetCoreOptionsVersion(data),
            RETRO_ENVIRONMENT.GET_PREFERRED_HW_RENDER                     => wrapper.GraphicsHandler.GetPreferredHwRender(data),
            RETRO_ENVIRONMENT.GET_DISK_CONTROL_INTERFACE_VERSION          => wrapper.DiskHandler.GetDiskControlInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_MESSAGE_INTERFACE_VERSION               => wrapper.MessageHandler.GetMessageInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_INPUT_MAX_USERS                         => wrapper.InputHandler.GetInputMaxUsers(data),
            RETRO_ENVIRONMENT.GET_GAME_INFO_EXT                           => wrapper.Game.GetGameInfoExt(data),
            RETRO_ENVIRONMENT.GET_THROTTLE_STATE                          => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_SAVESTATE_CONTEXT                       => EnvironmentNotImplemented(cmd),

            /************************************************************************************************
            * Core to frontend
            */
            RETRO_ENVIRONMENT.SET_ROTATION                                => wrapper.Game.SetRotation(data),
            RETRO_ENVIRONMENT.SET_MESSAGE                                 => wrapper.MessageHandler.SetMessage(data),
            RETRO_ENVIRONMENT.SHUTDOWN                                    => wrapper.Shutdown(),
            RETRO_ENVIRONMENT.SET_PERFORMANCE_LEVEL                       => wrapper.Core.SetPerformanceLevel(data),
            RETRO_ENVIRONMENT.SET_PIXEL_FORMAT                            => wrapper.GraphicsHandler.SetPixelFormat(data),
            RETRO_ENVIRONMENT.SET_INPUT_DESCRIPTORS                       => wrapper.InputHandler.SetInputDescriptors(data),
            RETRO_ENVIRONMENT.SET_KEYBOARD_CALLBACK                       => wrapper.InputHandler.SetKeyboardCallback(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_INTERFACE                  => wrapper.DiskHandler.SetDiskControlInterface(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER                               => wrapper.GraphicsHandler.SetHwRender(data),
            RETRO_ENVIRONMENT.SET_VARIABLES                               => wrapper.OptionsHandler.SetVariables(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_NO_GAME                         => wrapper.Core.SetSupportNoGame(data),
            RETRO_ENVIRONMENT.SET_FRAME_TIME_CALLBACK                     => wrapper.EnvironmentHandler.SetFrameTimeCallback(data),
            RETRO_ENVIRONMENT.SET_AUDIO_CALLBACK                          => wrapper.AudioHandler.SetAudioCallback(data),
            RETRO_ENVIRONMENT.SET_SYSTEM_AV_INFO                          => wrapper.Game.SetSystemAvInfo(data),
            RETRO_ENVIRONMENT.SET_PROC_ADDRESS_CALLBACK                   => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_SUBSYSTEM_INFO                          => wrapper.Core.SetSubsystemInfo(data),
            RETRO_ENVIRONMENT.SET_CONTROLLER_INFO                         => wrapper.InputHandler.SetControllerInfo(data),
            RETRO_ENVIRONMENT.SET_MEMORY_MAPS                             => wrapper.MemoryHandler.SetMemoryMaps(data),
            RETRO_ENVIRONMENT.SET_GEOMETRY                                => wrapper.GraphicsHandler.SetGeometry(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_ACHIEVEMENTS                    => wrapper.Core.SetSupportAchievements(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_SERIALIZATION_QUIRKS                    => wrapper.SerializationHandler.SetSerializationQuirks(data),
            RETRO_ENVIRONMENT.SET_HW_SHARED_CONTEXT                       => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS                            => wrapper.OptionsHandler.SetCoreOptions(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_INTL                       => wrapper.OptionsHandler.SetCoreOptionsIntl(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_DISPLAY                    => wrapper.OptionsHandler.SetCoreOptionsDisplay(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_EXT_INTERFACE              => wrapper.DiskHandler.SetDiskControlExtInterface(data),
            RETRO_ENVIRONMENT.SET_MESSAGE_EXT                             => wrapper.MessageHandler.SetMessageExt(data),
            RETRO_ENVIRONMENT.SET_AUDIO_BUFFER_STATUS_CALLBACK            => wrapper.AudioHandler.SetAudioBufferStatusCallback(data),
            RETRO_ENVIRONMENT.SET_MINIMUM_AUDIO_LATENCY                   => wrapper.AudioHandler.SetMinimumAudioLatency(data),
            RETRO_ENVIRONMENT.SET_FASTFORWARDING_OVERRIDE                 => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CONTENT_INFO_OVERRIDE                   => wrapper.Game.SetContentInfoOverride(data),
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

            _wrapper.LogHandler.LogInfo("Using FrameTime Callback", nameof(RETRO_ENVIRONMENT.SET_FRAME_TIME_CALLBACK));
            _wrapper.FrameTimeInterface = data.ToStructure<retro_frame_time_callback>();
            return true;
        }

        /************************************************************************************************
        / Utilities
        /***********************************************************************************************/
        private static bool EnvironmentNotImplemented(RETRO_ENVIRONMENT cmd, bool log = true, bool defaultReturns = false)
        {
            if (!log)
                return defaultReturns;

            if (!Wrapper.TryGetInstance(Thread.CurrentThread, out Wrapper wrapper))
                return defaultReturns;

            if (defaultReturns)
                wrapper.LogHandler.LogWarning("Environment not implemented!", cmd.ToString());
            else
                wrapper.LogHandler.LogError("Environment not implemented!", cmd.ToString());
            return defaultReturns;
        }

        private static bool EnvironmentUnknown(RETRO_ENVIRONMENT cmd)
        {
            if (Wrapper.TryGetInstance(Thread.CurrentThread, out Wrapper wrapper))
                wrapper.LogHandler.LogError("Environment unknown!", cmd.ToString());
            return false;
        }
    }
}
