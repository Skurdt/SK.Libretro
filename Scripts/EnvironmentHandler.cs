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
using System.Collections.Generic;
using System.Threading;

namespace SK.Libretro
{
    internal sealed class EnvironmentHandler
    {
        private delegate bool CallbackDelegate(RETRO_ENVIRONMENT cmd, IntPtr data);

        private readonly Wrapper _wrapper;
        private readonly retro_environment_t _callback;

        private readonly Thread _thread;

        public EnvironmentHandler(Wrapper wrapper)
        {
            _callback = EnvironmentCallback;
            _wrapper  = wrapper;
            _thread   = Thread.CurrentThread;
            lock (instancePerHandle)
            {
                instancePerHandle.Add(_thread, this);
            }
        }

        ~EnvironmentHandler()
        {
            lock(instancePerHandle)
            {
                instancePerHandle.Remove(_thread);
            }
        }

        public void SetCoreCallback(retro_set_environment_t setEnvironment) => setEnvironment(NativeEnvironmentCallback);

        public class MonoPInvokeCallbackAttribute : System.Attribute
        {
            private Type type;
            public MonoPInvokeCallbackAttribute(Type t) { type = t; }
        }

        // IL2CPP does not support marshaling delegates that point to instance methods to native code.
        // Using static method and per instance table.
        private static Dictionary<Thread, EnvironmentHandler> instancePerHandle = new Dictionary<Thread, EnvironmentHandler>();

        [MonoPInvokeCallbackAttribute(typeof(CallbackDelegate))]
        private static bool NativeEnvironmentCallback(RETRO_ENVIRONMENT cmd, IntPtr data)
        {
            EnvironmentHandler instance;
            bool ok;
            lock (instancePerHandle)
            {
                ok = instancePerHandle.TryGetValue(Thread.CurrentThread, out instance);
            }
            if (ok)
            {
                return instance._callback(cmd, data);
            }
            return false;
        }

        private bool EnvironmentCallback(RETRO_ENVIRONMENT cmd, IntPtr data) => cmd switch
        {
            /************************************************************************************************
             * Frontend to core
             */
            RETRO_ENVIRONMENT.GET_OVERSCAN                                => _wrapper.GraphicsHandler.GetOverscan(data),
            RETRO_ENVIRONMENT.GET_CAN_DUPE                                => _wrapper.GraphicsHandler.GetCanDupe(data),
            RETRO_ENVIRONMENT.GET_SYSTEM_DIRECTORY                        => _wrapper.GetSystemDirectory(data),
            RETRO_ENVIRONMENT.GET_VARIABLE                                => _wrapper.OptionsHandler.GetVariable(data),
            RETRO_ENVIRONMENT.GET_VARIABLE_UPDATE                         => _wrapper.OptionsHandler.GetVariableUpdate(data),
            RETRO_ENVIRONMENT.GET_LIBRETRO_PATH                           => _wrapper.GetLibretroPath(data),
            RETRO_ENVIRONMENT.GET_RUMBLE_INTERFACE                        => _wrapper.InputHandler.GetRumbleInterface(data),
            RETRO_ENVIRONMENT.GET_INPUT_DEVICE_CAPABILITIES               => _wrapper.InputHandler.GetInputDeviceCapabilities(data),
            RETRO_ENVIRONMENT.GET_SENSOR_INTERFACE                        => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_CAMERA_INTERFACE                        => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_LOG_INTERFACE                           => _wrapper.LogHandler.GetLogInterface(data),
            RETRO_ENVIRONMENT.GET_PERF_INTERFACE                          => EnvironmentNotImplemented(cmd)/*_wrapper.Perf.GetPerfInterface(data)*/,
            RETRO_ENVIRONMENT.GET_LOCATION_INTERFACE                      => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_CORE_ASSETS_DIRECTORY                   => _wrapper.GetCoreAssetsDirectory(data),
            RETRO_ENVIRONMENT.GET_SAVE_DIRECTORY                          => _wrapper.SerializationHandler.GetSaveDirectory(data),
            RETRO_ENVIRONMENT.GET_USERNAME                                => _wrapper.GetUsername(data),
            RETRO_ENVIRONMENT.GET_LANGUAGE                                => _wrapper.GetLanguage(data),
            RETRO_ENVIRONMENT.GET_CURRENT_SOFTWARE_FRAMEBUFFER            => _wrapper.GraphicsHandler.GetCurrentSoftwareFramebuffer(),
            RETRO_ENVIRONMENT.GET_HW_RENDER_INTERFACE                     => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_VFS_INTERFACE                           => _wrapper.VFSHandler.GetVfsInterface(data),
            RETRO_ENVIRONMENT.GET_LED_INTERFACE                           => _wrapper.LedHandler.GetLedInterface(data),
            RETRO_ENVIRONMENT.GET_AUDIO_VIDEO_ENABLE                      => GetAudioVideoEnable(data),
            RETRO_ENVIRONMENT.GET_MIDI_INTERFACE                          => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_FASTFORWARDING                          => GetFastForwarding(data),
            RETRO_ENVIRONMENT.GET_TARGET_REFRESH_RATE                     => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_INPUT_BITMASKS                          => _wrapper.InputHandler.GetInputBitmasks(data),
            RETRO_ENVIRONMENT.GET_CORE_OPTIONS_VERSION                    => _wrapper.OptionsHandler.GetCoreOptionsVersion(data),
            RETRO_ENVIRONMENT.GET_PREFERRED_HW_RENDER                     => _wrapper.GraphicsHandler.GetPreferredHwRender(data),
            RETRO_ENVIRONMENT.GET_DISK_CONTROL_INTERFACE_VERSION          => _wrapper.DiskHandler.GetDiskControlInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_MESSAGE_INTERFACE_VERSION               => _wrapper.MessageHandler.GetMessageInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_INPUT_MAX_USERS                         => _wrapper.InputHandler.GetInputMaxUsers(data),
            RETRO_ENVIRONMENT.GET_GAME_INFO_EXT                           => _wrapper.Game.GetGameInfoExt(data),
            RETRO_ENVIRONMENT.GET_THROTTLE_STATE                          => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.GET_SAVESTATE_CONTEXT                       => EnvironmentNotImplemented(cmd),

            /************************************************************************************************
             * Core to frontend
             */
            RETRO_ENVIRONMENT.SET_ROTATION                                => _wrapper.Game.SetRotation(data),
            RETRO_ENVIRONMENT.SET_MESSAGE                                 => _wrapper.MessageHandler.SetMessage(data),
            RETRO_ENVIRONMENT.SHUTDOWN                                    => _wrapper.Shutdown(),
            RETRO_ENVIRONMENT.SET_PERFORMANCE_LEVEL                       => _wrapper.Core.SetPerformanceLevel(data),
            RETRO_ENVIRONMENT.SET_PIXEL_FORMAT                            => _wrapper.GraphicsHandler.SetPixelFormat(data),
            RETRO_ENVIRONMENT.SET_INPUT_DESCRIPTORS                       => _wrapper.InputHandler.SetInputDescriptors(data),
            RETRO_ENVIRONMENT.SET_KEYBOARD_CALLBACK                       => _wrapper.InputHandler.SetKeyboardCallback(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_INTERFACE                  => _wrapper.DiskHandler.SetDiskControlInterface(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER                               => _wrapper.GraphicsHandler.SetHwRender(data),
            RETRO_ENVIRONMENT.SET_VARIABLES                               => _wrapper.OptionsHandler.SetVariables(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_NO_GAME                         => _wrapper.Core.SetSupportNoGame(data),
            RETRO_ENVIRONMENT.SET_FRAME_TIME_CALLBACK                     => SetFrameTimeCallback(data),
            RETRO_ENVIRONMENT.SET_AUDIO_CALLBACK                          => _wrapper.AudioHandler.SetAudioCallback(data),
            RETRO_ENVIRONMENT.SET_SYSTEM_AV_INFO                          => _wrapper.Game.SetSystemAvInfo(data),
            RETRO_ENVIRONMENT.SET_PROC_ADDRESS_CALLBACK                   => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_SUBSYSTEM_INFO                          => _wrapper.Core.SetSubsystemInfo(data),
            RETRO_ENVIRONMENT.SET_CONTROLLER_INFO                         => _wrapper.InputHandler.SetControllerInfo(data),
            RETRO_ENVIRONMENT.SET_MEMORY_MAPS                             => _wrapper.MemoryHandler.SetMemoryMaps(data),
            RETRO_ENVIRONMENT.SET_GEOMETRY                                => _wrapper.GraphicsHandler.SetGeometry(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_ACHIEVEMENTS                    => _wrapper.Core.SetSupportAchievements(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_SERIALIZATION_QUIRKS                    => _wrapper.SerializationHandler.SetSerializationQuirks(data),
            RETRO_ENVIRONMENT.SET_HW_SHARED_CONTEXT                       => EnvironmentNotImplemented(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS                            => _wrapper.OptionsHandler.SetCoreOptions(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_INTL                       => _wrapper.OptionsHandler.SetCoreOptionsIntl(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_DISPLAY                    => _wrapper.OptionsHandler.SetCoreOptionsDisplay(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_EXT_INTERFACE              => _wrapper.DiskHandler.SetDiskControlExtInterface(data),
            RETRO_ENVIRONMENT.SET_MESSAGE_EXT                             => _wrapper.MessageHandler.SetMessageExt(data),
            RETRO_ENVIRONMENT.SET_AUDIO_BUFFER_STATUS_CALLBACK            => _wrapper.AudioHandler.SetAudioBufferStatusCallback(data),
            RETRO_ENVIRONMENT.SET_MINIMUM_AUDIO_LATENCY                   => _wrapper.AudioHandler.SetMinimumAudioLatency(data),
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

        /************************************************************************************************
        / Core to frontend
        /***********************************************************************************************/
        private bool SetFrameTimeCallback(IntPtr data)
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
        private bool EnvironmentNotImplemented(RETRO_ENVIRONMENT cmd, bool log = true, bool defaultReturns = false)
        {
            if (!log)
                return defaultReturns;

            if (defaultReturns)
                _wrapper.LogHandler.LogWarning("Environment not implemented!", cmd.ToString());
            else
                _wrapper.LogHandler.LogError("Environment not implemented!", cmd.ToString());
            return defaultReturns;
        }

        private bool EnvironmentUnknown(RETRO_ENVIRONMENT cmd)
        {
            _wrapper.LogHandler.LogError("Environment unknown!", cmd.ToString());
            return false;
        }
    }
}
