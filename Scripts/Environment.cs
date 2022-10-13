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

using SK.Libretro.Header;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using static SK.Libretro.Header.RETRO;

namespace SK.Libretro
{
    internal sealed class Environment
    {
        public readonly retro_environment_t Callback;

        private readonly Wrapper _wrapper;

        public Environment(Wrapper wrapper)
        {
            Callback = CallbackCall;
            _wrapper = wrapper;
        }

        public bool CallbackCall(RETRO_ENVIRONMENT cmd, IntPtr data) => cmd switch
        {
            /************************************************************************************************
             * Frontend to core
             */
            RETRO_ENVIRONMENT.GET_OVERSCAN                                => GetOverscan(ref data),
            RETRO_ENVIRONMENT.GET_CAN_DUPE                                => GetCanDupe(ref data),
            RETRO_ENVIRONMENT.GET_SYSTEM_DIRECTORY                        => GetSystemDirectory(ref data),
            RETRO_ENVIRONMENT.GET_VARIABLE                                => GetVariable(ref data),
            RETRO_ENVIRONMENT.GET_VARIABLE_UPDATE                         => GetVariableUpdate(ref data),
            RETRO_ENVIRONMENT.GET_LIBRETRO_PATH                           => GetLibretroPath(ref data),
            RETRO_ENVIRONMENT.GET_RUMBLE_INTERFACE                        => GetRumbleInterface(ref data),
            RETRO_ENVIRONMENT.GET_INPUT_DEVICE_CAPABILITIES               => GetInputDeviceCapabilities(ref data),
            RETRO_ENVIRONMENT.GET_SENSOR_INTERFACE                        => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_CAMERA_INTERFACE                        => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_LOG_INTERFACE                           => GetLogInterface(ref data),
            RETRO_ENVIRONMENT.GET_PERF_INTERFACE                          => GetPerfInterface(ref data),
            RETRO_ENVIRONMENT.GET_LOCATION_INTERFACE                      => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_CORE_ASSETS_DIRECTORY                   => GetCoreAssetsDirectory(ref data),
            RETRO_ENVIRONMENT.GET_SAVE_DIRECTORY                          => GetSaveDirectory(ref data),
            RETRO_ENVIRONMENT.GET_USERNAME                                => GetUsername(ref data),
            RETRO_ENVIRONMENT.GET_LANGUAGE                                => GetLanguage(ref data),
            RETRO_ENVIRONMENT.GET_CURRENT_SOFTWARE_FRAMEBUFFER            => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_HW_RENDER_INTERFACE                     => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_VFS_INTERFACE                           => GetVfsInterface(ref data),
            RETRO_ENVIRONMENT.GET_LED_INTERFACE                           => GetLedInterface(ref data),
            RETRO_ENVIRONMENT.GET_AUDIO_VIDEO_ENABLE                      => GetAudioVideoEnable(ref data),
            RETRO_ENVIRONMENT.GET_MIDI_INTERFACE                          => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_FASTFORWARDING                          => GetFastForwarding(ref data),
            RETRO_ENVIRONMENT.GET_TARGET_REFRESH_RATE                     => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_INPUT_BITMASKS                          => GetInputBitmasks(ref data),
            RETRO_ENVIRONMENT.GET_CORE_OPTIONS_VERSION                    => GetCoreOptionsVersion(ref data),
            RETRO_ENVIRONMENT.GET_PREFERRED_HW_RENDER                     => GetPreferredHwRender(ref data),
            RETRO_ENVIRONMENT.GET_DISK_CONTROL_INTERFACE_VERSION          => GetDiskControlInterfaceVersion(ref data),
            RETRO_ENVIRONMENT.GET_MESSAGE_INTERFACE_VERSION               => GetMessageInterfaceVersion(ref data),
            RETRO_ENVIRONMENT.GET_INPUT_MAX_USERS                         => GetInputMaxUsers(ref data),
            RETRO_ENVIRONMENT.GET_GAME_INFO_EXT                           => GetGameInfoExt(ref data),
            RETRO_ENVIRONMENT.GET_THROTTLE_STATE                          => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_SAVESTATE_CONTEXT                       => ENVIRONMENT_NOT_IMPLEMENTED(cmd),

            /************************************************************************************************
             * Core to frontend
             */
            RETRO_ENVIRONMENT.SET_ROTATION                                => SetRotation(ref data),
            RETRO_ENVIRONMENT.SET_MESSAGE                                 => SetMessage(ref data),
            RETRO_ENVIRONMENT.SHUTDOWN                                    => Shutdown(),
            RETRO_ENVIRONMENT.SET_PERFORMANCE_LEVEL                       => SetPerformanceLevel(ref data),
            RETRO_ENVIRONMENT.SET_PIXEL_FORMAT                            => SetPixelFormat(ref data),
            RETRO_ENVIRONMENT.SET_INPUT_DESCRIPTORS                       => SetInputDescriptors(ref data),
            RETRO_ENVIRONMENT.SET_KEYBOARD_CALLBACK                       => SetKeyboardCallback(ref data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_INTERFACE                  => SetDiskControlInterface(ref data),
            RETRO_ENVIRONMENT.SET_HW_RENDER                               => SetHwRender(ref data),
            RETRO_ENVIRONMENT.SET_VARIABLES                               => SetVariables(ref data),
            RETRO_ENVIRONMENT.SET_SUPPORT_NO_GAME                         => SetSupportNoGame(ref data),
            RETRO_ENVIRONMENT.SET_FRAME_TIME_CALLBACK                     => SetFrameTimeCallback(ref data),
            RETRO_ENVIRONMENT.SET_AUDIO_CALLBACK                          => SetAudioCallback(ref data),
            RETRO_ENVIRONMENT.SET_SYSTEM_AV_INFO                          => SetSystemAvInfo(ref data),
            RETRO_ENVIRONMENT.SET_PROC_ADDRESS_CALLBACK                   => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_SUBSYSTEM_INFO                          => SetSubsystemInfo(ref data),
            RETRO_ENVIRONMENT.SET_CONTROLLER_INFO                         => SetControllerInfo(ref data),
            RETRO_ENVIRONMENT.SET_MEMORY_MAPS                             => SetMemoryMaps(ref data),
            RETRO_ENVIRONMENT.SET_GEOMETRY                                => SetGeometry(ref data),
            RETRO_ENVIRONMENT.SET_SUPPORT_ACHIEVEMENTS                    => SetSupportAchievements(ref data),
            RETRO_ENVIRONMENT.SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_SERIALIZATION_QUIRKS                    => SetSerializationQuirks(ref data),
            RETRO_ENVIRONMENT.SET_HW_SHARED_CONTEXT                       => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS                            => SetCoreOptions(ref data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_INTL                       => SetCoreOptionsIntl(ref data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_DISPLAY                    => SetCoreOptionsDisplay(ref data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_EXT_INTERFACE              => SetDiskControlExtInterface(ref data),
            RETRO_ENVIRONMENT.SET_MESSAGE_EXT                             => SetMessageExt(ref data),
            RETRO_ENVIRONMENT.SET_AUDIO_BUFFER_STATUS_CALLBACK            => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_MINIMUM_AUDIO_LATENCY                   => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_FASTFORWARDING_OVERRIDE                 => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_CONTENT_INFO_OVERRIDE                   => SetContentInfoOverride(ref data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_V2                         => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_V2_INTL                    => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_UPDATE_DISPLAY_CALLBACK    => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_VARIABLE                                => ENVIRONMENT_NOT_IMPLEMENTED(cmd),

            _                                                                               => ENVIRONMENT_UNKNOWN(cmd)
        };

        /************************************************************************************************
        * Frontend to core
        */
        private bool GetOverscan(ref IntPtr data)
        {
            data.Write(_wrapper.Settings.CropOverscan);
            return true;
        }

        private bool GetCanDupe(ref IntPtr data)
        {
            data.Write(true);
            return true;
        }

        private bool GetSystemDirectory(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{Wrapper.MainDirectory}/system");

            IntPtr stringPtr = _wrapper.GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetVariable(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_variable outVariable = data.ToStructure<retro_variable>();
            if (outVariable.key.IsNull())
                return false;

            string key = outVariable.key.AsString();
            if (_wrapper.Core.GameOptions is null || !_wrapper.Core.GameOptions.TryGetValue(key, out CoreOption coreOption))
            {
                if (_wrapper.Core.CoreOptions is null)
                {
                    Logger.Instance.LogWarning($"Core didn't set its options. Requested key: {key}", nameof(RETRO_ENVIRONMENT.GET_VARIABLE));
                    return false;
                }

                if (!_wrapper.Core.CoreOptions.TryGetValue(key, out coreOption))
                {
                    Logger.Instance.LogWarning($"Core option '{key}' not found.", nameof(RETRO_ENVIRONMENT.GET_VARIABLE));
                    return false;
                }
            }

            outVariable.value = _wrapper.GetUnsafeString(coreOption.CurrentValue);
            Marshal.StructureToPtr(outVariable, data, false);
            return true;
        }

        private bool GetVariableUpdate(ref IntPtr data)
        {
            data.Write(_wrapper.UpdateVariables);

            if (!_wrapper.UpdateVariables)
                return false;

            _wrapper.UpdateVariables = false;
            return true;
        }

        private bool GetLibretroPath(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory(_wrapper.Core.Path);
            IntPtr stringPtr = _wrapper.GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetRumbleInterface(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            Marshal.StructureToPtr(_wrapper.Input.RumbleInterface, data, true);
            return true;
        }

        private bool GetInputDeviceCapabilities(ref IntPtr data)
        {
            ulong bits = (1 << (int)RETRO_DEVICE.JOYPAD)
                       | (1 << (int)RETRO_DEVICE.MOUSE)
                       | (1 << (int)RETRO_DEVICE.KEYBOARD)
                       | (1 << (int)RETRO_DEVICE.LIGHTGUN)
                       | (1 << (int)RETRO_DEVICE.ANALOG)
                       | (1 << (int)RETRO_DEVICE.POINTER);

            data.Write(bits);
            return true;
        }

        private bool GetLogInterface(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_log_callback callback = data.ToStructure<retro_log_callback>();
            callback.log = Marshal.GetFunctionPointerForDelegate<retro_log_printf_t>(LogInterface.RetroLogPrintf);
            Marshal.StructureToPtr(callback, data, true);
            return true;
        }

        private bool GetPerfInterface(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Perf = new();
            Marshal.StructureToPtr(_wrapper.Perf.Callback, data, true);
            return true;
        }

        private bool GetCoreAssetsDirectory(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{Wrapper.CoreAssetsDirectory}/{_wrapper.Core.Name}");
            IntPtr stringPtr = _wrapper.GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetSaveDirectory(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{Wrapper.SavesDirectory}/{_wrapper.Core.Name}");
            IntPtr stringPtr = _wrapper.GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetUsername(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            IntPtr stringPtr = _wrapper.GetUnsafeString(_wrapper.Settings.UserName);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetLanguage(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            IntPtr stringPtr = _wrapper.GetUnsafeString(_wrapper.Settings.Language.ToString());
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetVfsInterface(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            //retro_vfs_interface_info interfaceInfo = data.ToStructure<retro_vfs_interface_info>();
            //if (interfaceInfo.required_interface_version > 2)
            //    return false;

            //if (_interfacePtr.IsNull())
            //{
            //    _interfacePtr = Marshal.AllocHGlobal(Marshal.SizeOf<retro_vfs_interface>());
            //    Marshal.StructureToPtr(_interface, _interfacePtr, false);
            //}

            //interfaceInfo.iface = _interfacePtr;
            //Marshal.StructureToPtr(interfaceInfo, data, true);
            //return true;

            return false;
        }

        private bool GetLedInterface(ref IntPtr data)
        {

            if (data.IsNull())
                return false;

            _wrapper.Led = new();
            Marshal.StructureToPtr(_wrapper.Led.Interface, data, true);
            return true;
        }

        private bool GetAudioVideoEnable(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            int bits = 0;
            bits    |= 1; // if video enabled
            bits    |= 2; // if audio enabled

            data.Write(bits);
            return true;
        }

        private bool GetFastForwarding(ref IntPtr data)
        {
            data.Write(false);
            return false;
        }

        private bool GetInputBitmasks(ref IntPtr data)
        {
            data.Write(true);
            return true;
        }

        private bool GetCoreOptionsVersion(ref IntPtr data)
        {
            data.Write(API_VERSION);
            return true;
        }

        private bool GetPreferredHwRender(ref IntPtr data)
        {
            data.Write((uint)retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE);
            return true;
        }

        private bool GetMessageInterfaceVersion(ref IntPtr data)
        {
            data.Write(MessageInterface.VERSION);
            return true;
        }

        private bool GetInputMaxUsers(ref IntPtr data)
        {
            data.Write(Input.MAX_USERS_SUPPORTED);
            return true;
        }

        private bool GetGameInfoExt(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Game.GetGameInfoExt(ref data);
            return true;
        }

        private bool GetDiskControlInterfaceVersion(ref IntPtr data)
        {
            data.Write(DiskInterface.VERSION);
            return true;
        }

        /************************************************************************************************
        / Core to frontend
        /***********************************************************************************************/
        private bool SetRotation(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.EnvironmentVariables.SetRotation((int)data.ReadUInt32());
            return _wrapper.Settings.UseCoreRotation;
        }

        private bool SetMessage(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_message retroMessage = data.ToStructure<retro_message>();
            string message = retroMessage.msg.AsString();
            Logger.Instance.LogInfo($"<- Message: {message}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE));
            return true;
        }

        private bool SetPerformanceLevel(ref IntPtr data)
        {
            _wrapper.EnvironmentVariables.SetPerformanceLevel(data.ReadInt32());
            return true;
        }

        private bool SetPixelFormat(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Graphics.SetPixelFormat(data.ReadInt32());
            return _wrapper.Graphics.PixelFormat != retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN;
        }

        private bool SetInputDescriptors(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Input.SetInputDescriptors(ref data);
            return true;
        }

        private bool SetKeyboardCallback(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Input.KeyboardCallback = data.ToStructure<retro_keyboard_callback>();
            return true;
        }

        private bool SetDiskControlInterface(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_disk_control_callback callback = data.ToStructure<retro_disk_control_callback>();
            _wrapper.Disk = new(_wrapper, callback);
            return true;
        }

        private bool SetHwRender(ref IntPtr data)
        {
            if (data.IsNull() || _wrapper.EnvironmentVariables.HwAccelerated)
                return false;

            retro_hw_render_callback callback = data.ToStructure<retro_hw_render_callback>();
            if (callback.context_type is not retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL and not retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE)
                return false;

            _wrapper.OpenGLHelperWindow = new();
            if (!_wrapper.OpenGLHelperWindow.Init())
                return false;

            callback.get_current_framebuffer = _wrapper.OpenGLHelperWindow.GetCurrentFrameBuffer.GetFunctionPointer();
            callback.get_proc_address        = _wrapper.OpenGLHelperWindow.GetProcAddress.GetFunctionPointer();

            _wrapper.HwRenderInterface = callback;
            Marshal.StructureToPtr(_wrapper.HwRenderInterface, data, false);

            _wrapper.EnvironmentVariables.HwAccelerated = true;
            return true;
        }

        private bool SetVariables(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            try
            {
                _wrapper.Core.DeserializeOptions();

                retro_variable variable = data.ToStructure<retro_variable>();
                while (variable is not null && variable.key.IsNotNull() && variable.value.IsNotNull())
                {
                    string key         = variable.key.AsString();
                    string inValue     = variable.value.AsString();
                    string[] lineSplit = inValue.Split(';');
                    if (_wrapper.Core.CoreOptions[key] is null)
                        _wrapper.Core.CoreOptions[key] = lineSplit.Length > 3 ? new CoreOption(lineSplit) : new CoreOption(key, lineSplit);
                    else
                    {
                        if (lineSplit.Length > 3)
                            _wrapper.Core.CoreOptions[key].Update(lineSplit);
                        else
                            _wrapper.Core.CoreOptions[key].Update(key, lineSplit);
                    }

                    data += Marshal.SizeOf(variable);
                    data.ToStructure(variable);
                }

                _wrapper.Core.SerializeOptions();
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                return false;
            }
        }

        private bool SetSupportNoGame(ref IntPtr data)
        {
            _wrapper.EnvironmentVariables.SetSupportNoGame(data.IsTrue());
            return true;
        }

        private bool SetFrameTimeCallback(ref IntPtr data)
        {
            Logger.Instance.LogInfo("Using FrameTime Callback");
            _wrapper.FrameTimeInterface = data.ToStructure<retro_frame_time_callback>();
            return true;
        }

        private bool SetAudioCallback(ref IntPtr data)
        {
            _wrapper.Audio.AudioCallback = data.ToStructure<retro_audio_callback>();
            return true;
        }

        private bool SetSystemAvInfo(ref IntPtr data)
        {
            _wrapper.Game.SetSystemAVInfo(data.ToStructure<retro_system_av_info>());
            return true;
        }

        private bool SetSubsystemInfo(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.EnvironmentVariables.SetSubsystemInfo(ref data);
            return true;
        }

        private bool SetControllerInfo(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Input.SetControllerInfo(ref data);
            return true;
        }

        private bool SetMemoryMaps(ref IntPtr data)
        {
            _wrapper.Memory = new MemoryMap(data.ToStructure<retro_memory_map>());
            return true;
        }

        private bool SetGeometry(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Game.SetGeometry(data.ToStructure<retro_game_geometry>());
            return true;
        }

        private bool SetSupportAchievements(ref IntPtr data)
        {
            _wrapper.EnvironmentVariables.SetSupportsAchievements(data.IsTrue());
            return false;
        }

        private bool SetSerializationQuirks(ref IntPtr data)
        {
            ulong quirks = data.ReadUInt64();
            // quirks |= Header.RETRO_SERIALIZATION_QUIRK_FRONT_VARIABLE_SIZE;
            _wrapper.Serialization.SetQuirks(quirks);
            return true;
        }

        private bool SetCoreOptions(ref IntPtr data) => SetCoreOptionsInternal(ref data);

        private bool SetCoreOptionsIntl(ref IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_core_options_intl intl = data.ToStructure<retro_core_options_intl>();
            bool result = SetCoreOptionsInternal(ref intl.local);
            if (!result)
                result = SetCoreOptionsInternal(ref intl.us);
            return result;
        }

        private bool SetCoreOptionsDisplay(ref IntPtr data)
        {
            retro_core_option_display coreOptionDisplay = data.ToStructure<retro_core_option_display>();
            if (coreOptionDisplay.key.IsNull())
                return false;

            string key = coreOptionDisplay.key.AsString();
            _wrapper.Core.CoreOptions[key]?.SetVisibility(coreOptionDisplay.visible);
            return true;
        }

        private bool SetDiskControlExtInterface(ref IntPtr data)
        {
            retro_disk_control_ext_callback callback = data.ToStructure<retro_disk_control_ext_callback>();
            _wrapper.Disk = new(_wrapper, callback);
            return true;
        }

        private bool Shutdown() => false;

        private bool SetMessageExt(ref IntPtr data)
        {
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

        private bool SetContentInfoOverride(ref IntPtr data)
        {
            _wrapper.Game.SetContentInfoOverride(ref data);
            return true;
        }

        private bool SetCoreOptionsInternal(ref IntPtr data)
        {
            try
            {
                _wrapper.Core.DeserializeOptions();

                if (data.IsNull())
                    return false;

                Type type = typeof(retro_core_option_values);
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                FieldInfo[] fields = type.GetFields(bindingFlags);

                retro_core_option_definition optionDefinition = data.ToStructure<retro_core_option_definition>();
                while (optionDefinition is not null && optionDefinition.key.IsNotNull())
                {
                    string key          = optionDefinition.key.AsString();
                    string description  = optionDefinition.desc.AsString();
                    string info         = optionDefinition.info.AsString();
                    string defaultValue = optionDefinition.default_value.AsString();

                    List<string> possibleValues = new();
                    for (int i = 0; i < fields.Length; ++i)
                    {
                        FieldInfo fieldInfo = fields[i];
                        if (fieldInfo.GetValue(optionDefinition.values) is not retro_core_option_value optionValue || optionValue.value.IsNull())
                            continue;

                        possibleValues.Add(optionValue.value.AsString());
                    }

                    string value = "";
                    if (!string.IsNullOrWhiteSpace(defaultValue))
                        value = defaultValue;
                    else if (possibleValues.Count > 0)
                        value = possibleValues[0];

                    if (_wrapper.Core.CoreOptions[key] == null)
                        _wrapper.Core.CoreOptions[key] = new CoreOption(key, description, info, value, possibleValues.ToArray());
                    else
                        _wrapper.Core.CoreOptions[key].Update(key, description, info, possibleValues.ToArray());

                    data += Marshal.SizeOf(optionDefinition);
                    data.ToStructure(optionDefinition);
                }

                _wrapper.Core.SerializeOptions();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool ENVIRONMENT_NOT_IMPLEMENTED(RETRO_ENVIRONMENT cmd, bool log = true, bool defaultReturns = false)
        {
            if (!log)
                return defaultReturns;

            if (defaultReturns)
                Logger.Instance.LogWarning("Environment not implemented!", cmd.ToString());
            else
                Logger.Instance.LogError("Environment not implemented!", cmd.ToString());
            return defaultReturns;
        }

        private static bool ENVIRONMENT_UNKNOWN(RETRO_ENVIRONMENT cmd)
        {
            Logger.Instance.LogError("Environment unknown!", cmd.ToString());
            return false;
        }
    }
}
