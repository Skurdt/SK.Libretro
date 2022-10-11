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

        private readonly List<retro_subsystem_info> _subsystemInfo = new();

        private readonly List<retro_input_descriptor> _inputDescriptors = new();
        private readonly List<retro_controller_info> _controllerInfo = new();
        private readonly List<retro_controller_description> _controllerDescriptions = new();

        private readonly List<retro_system_content_info_override> _systemContentInfoOverrides = new();

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
            RETRO_ENVIRONMENT.GET_OVERSCAN                                => GetOverscan(data),
            RETRO_ENVIRONMENT.GET_CAN_DUPE                                => GetCanDupe(data),
            RETRO_ENVIRONMENT.GET_SYSTEM_DIRECTORY                        => GetSystemDirectory(data),
            RETRO_ENVIRONMENT.GET_VARIABLE                                => GetVariable(data),
            RETRO_ENVIRONMENT.GET_VARIABLE_UPDATE                         => GetVariableUpdate(data),
            RETRO_ENVIRONMENT.GET_LIBRETRO_PATH                           => GetLibretroPath(data),
            RETRO_ENVIRONMENT.GET_RUMBLE_INTERFACE                        => GetRumbleInterface(data),
            RETRO_ENVIRONMENT.GET_INPUT_DEVICE_CAPABILITIES               => GetInputDeviceCapabilities(data),
            RETRO_ENVIRONMENT.GET_SENSOR_INTERFACE                        => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_CAMERA_INTERFACE                        => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_LOG_INTERFACE                           => GetLogInterface(data),
            RETRO_ENVIRONMENT.GET_PERF_INTERFACE                          => GetPerfInterface(data),
            RETRO_ENVIRONMENT.GET_LOCATION_INTERFACE                      => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_CORE_ASSETS_DIRECTORY                   => GetCoreAssetsDirectory(data),
            RETRO_ENVIRONMENT.GET_SAVE_DIRECTORY                          => GetSaveDirectory(data),
            RETRO_ENVIRONMENT.GET_USERNAME                                => GetUsername(data),
            RETRO_ENVIRONMENT.GET_LANGUAGE                                => GetLanguage(data),
            RETRO_ENVIRONMENT.GET_CURRENT_SOFTWARE_FRAMEBUFFER            => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_HW_RENDER_INTERFACE                     => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_VFS_INTERFACE                           => GetVfsInterface(data),
            RETRO_ENVIRONMENT.GET_LED_INTERFACE                           => GetLedInterface(data),
            RETRO_ENVIRONMENT.GET_AUDIO_VIDEO_ENABLE                      => GetAudioVideoEnable(data),
            RETRO_ENVIRONMENT.GET_MIDI_INTERFACE                          => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_FASTFORWARDING                          => GetFastForwarding(data),
            RETRO_ENVIRONMENT.GET_TARGET_REFRESH_RATE                     => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_INPUT_BITMASKS                          => GetInputBitmasks(data),
            RETRO_ENVIRONMENT.GET_CORE_OPTIONS_VERSION                    => GetCoreOptionsVersion(data),
            RETRO_ENVIRONMENT.GET_PREFERRED_HW_RENDER                     => GetPreferredHwRender(data),
            RETRO_ENVIRONMENT.GET_DISK_CONTROL_INTERFACE_VERSION          => GetDiskControlInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_MESSAGE_INTERFACE_VERSION               => GetMessageInterfaceVersion(data),
            RETRO_ENVIRONMENT.GET_INPUT_MAX_USERS                         => GetInputMaxUsers(data),
            RETRO_ENVIRONMENT.GET_GAME_INFO_EXT                           => GetGameInfoExt(data),
            RETRO_ENVIRONMENT.GET_THROTTLE_STATE                          => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.GET_SAVESTATE_CONTEXT                       => ENVIRONMENT_NOT_IMPLEMENTED(cmd),

            /************************************************************************************************
             * Core to frontend
             */
            RETRO_ENVIRONMENT.SET_ROTATION                                => SetRotation(data),
            RETRO_ENVIRONMENT.SET_MESSAGE                                 => SetMessage(data),
            RETRO_ENVIRONMENT.SHUTDOWN                                    => Shutdown(data),
            RETRO_ENVIRONMENT.SET_PERFORMANCE_LEVEL                       => SetPerformanceLevel(data),
            RETRO_ENVIRONMENT.SET_PIXEL_FORMAT                            => SetPixelFormat(data),
            RETRO_ENVIRONMENT.SET_INPUT_DESCRIPTORS                       => SetInputDescriptors(data),
            RETRO_ENVIRONMENT.SET_KEYBOARD_CALLBACK                       => SetKeyboardCallback(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_INTERFACE                  => SetDiskControlInterface(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER                               => SetHwRender(data),
            RETRO_ENVIRONMENT.SET_VARIABLES                               => SetVariables(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_NO_GAME                         => SetSupportNoGame(data),
            RETRO_ENVIRONMENT.SET_FRAME_TIME_CALLBACK                     => SetFrameTimeCallback(data),
            RETRO_ENVIRONMENT.SET_AUDIO_CALLBACK                          => SetAudioCallback(data),
            RETRO_ENVIRONMENT.SET_SYSTEM_AV_INFO                          => SetSystemAvInfo(data),
            RETRO_ENVIRONMENT.SET_PROC_ADDRESS_CALLBACK                   => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_SUBSYSTEM_INFO                          => SetSubsystemInfo(data),
            RETRO_ENVIRONMENT.SET_CONTROLLER_INFO                         => SetControllerInfo(data),
            RETRO_ENVIRONMENT.SET_MEMORY_MAPS                             => SetMemoryMaps(data),
            RETRO_ENVIRONMENT.SET_GEOMETRY                                => SetGeometry(data),
            RETRO_ENVIRONMENT.SET_SUPPORT_ACHIEVEMENTS                    => SetSupportAchievements(data),
            RETRO_ENVIRONMENT.SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_SERIALIZATION_QUIRKS                    => SetSerializationQuirks(data),
            RETRO_ENVIRONMENT.SET_HW_SHARED_CONTEXT                       => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS                            => SetCoreOptions(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_INTL                       => SetCoreOptionsIntl(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_DISPLAY                    => SetCoreOptionsDisplay(data),
            RETRO_ENVIRONMENT.SET_DISK_CONTROL_EXT_INTERFACE              => SetDiskControlExtInterface(data),
            RETRO_ENVIRONMENT.SET_MESSAGE_EXT                             => SetMessageExt(data),
            RETRO_ENVIRONMENT.SET_AUDIO_BUFFER_STATUS_CALLBACK            => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_MINIMUM_AUDIO_LATENCY                   => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_FASTFORWARDING_OVERRIDE                 => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_CONTENT_INFO_OVERRIDE                   => SetContentInfoOverride(data),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_V2                         => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_V2_INTL                    => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_CORE_OPTIONS_UPDATE_DISPLAY_CALLBACK    => ENVIRONMENT_NOT_IMPLEMENTED(cmd),
            RETRO_ENVIRONMENT.SET_VARIABLE                                => ENVIRONMENT_NOT_IMPLEMENTED(cmd),

            _                                                                               => ENVIRONMENT_UNKNOWN(cmd)
        };

        /************************************************************************************************
        * Frontend to core
        */
        private bool GetOverscan(IntPtr data)
        {
            data.Write(_wrapper.OptionCropOverscan);
            return true;
        }

        private bool GetCanDupe(IntPtr data)
        {
            data.Write(true);
            return true;
        }

        private bool GetSystemDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{Wrapper.MainDirectory}/system");

            IntPtr stringPtr = _wrapper.GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetVariable(IntPtr data)
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

        private bool GetVariableUpdate(IntPtr data)
        {
            data.Write(_wrapper.UpdateVariables);

            if (!_wrapper.UpdateVariables)
                return false;

            _wrapper.UpdateVariables = false;
            return true;
        }

        private bool GetLibretroPath(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory(_wrapper.Core.Path);
            IntPtr stringPtr = _wrapper.GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetRumbleInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            Marshal.StructureToPtr(_wrapper.Input.RumbleInterface, data, true);
            return true;
        }

        private bool GetInputDeviceCapabilities(IntPtr data)
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

        private bool GetLogInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_log_callback callback = data.ToStructure<retro_log_callback>();
            callback.log = Marshal.GetFunctionPointerForDelegate<retro_log_printf_t>(LogInterface.RetroLogPrintf);
            Marshal.StructureToPtr(callback, data, true);
            return true;
        }

        private bool GetPerfInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Perf = new();
            Marshal.StructureToPtr(_wrapper.Perf.Callback, data, true);
            return true;
        }

        private bool GetCoreAssetsDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{Wrapper.CoreAssetsDirectory}/{_wrapper.Core.Name}");
            IntPtr stringPtr = _wrapper.GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetSaveDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{Wrapper.SavesDirectory}/{_wrapper.Core.Name}");
            IntPtr stringPtr = _wrapper.GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetUsername(IntPtr data)
        {
            if (data.IsNull())
                return false;

            IntPtr stringPtr = _wrapper.GetUnsafeString(_wrapper.OptionUserName);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetLanguage(IntPtr data)
        {
            if (data.IsNull())
                return false;

            IntPtr stringPtr = _wrapper.GetUnsafeString(_wrapper.OptionLanguage.ToString());
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        private bool GetVfsInterface(IntPtr data)
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

        private bool GetLedInterface(IntPtr data)
        {

            if (data.IsNull())
                return false;

            _wrapper.Led = new();
            Marshal.StructureToPtr(_wrapper.Led.Interface, data, true);
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
            data.Write(false);
            return false;
        }

        private bool GetInputBitmasks(IntPtr data)
        {
            data.Write(true);
            return true;
        }

        private bool GetCoreOptionsVersion(IntPtr data)
        {
            data.Write(API_VERSION);
            return true;
        }

        private bool GetPreferredHwRender(IntPtr data)
        {
            data.Write((uint)retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE);
            return true;
        }

        private bool GetMessageInterfaceVersion(IntPtr data)
        {
            data.Write(MessageInterface.VERSION);
            return true;
        }

        private bool GetInputMaxUsers(IntPtr data)
        {
            data.Write(Input.MAX_USERS_SUPPORTED);
            return true;
        }

        private bool GetGameInfoExt(IntPtr data)
        {
            if (data.IsNull())
                return false;

            Marshal.StructureToPtr(_wrapper.Game.GameInfoExt, data, true);
            return true;
        }

        private bool GetDiskControlInterfaceVersion(IntPtr data)
        {
            data.Write(DiskInterface.VERSION);
            return true;
        }

        /************************************************************************************************
        / Core to frontend
        /***********************************************************************************************/
        private bool SetRotation(IntPtr data)
        {
            if (data.IsNull())
                return false;

            // Values: 0,  1,   2,   3
            // Result: 0, 90, 180, 270 degrees
            _wrapper.Core.Rotation = (int)data.ReadUInt32() * 90;
            return _wrapper.Graphics.UseCoreRotation;
        }

        private bool SetMessage(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_message retroMessage = data.ToStructure<retro_message>();
            string message = retroMessage.msg.AsString();
            Logger.Instance.LogInfo($"<- Message: {message}", nameof(RETRO_ENVIRONMENT.SET_MESSAGE));
            return true;
        }

        private bool SetPerformanceLevel(IntPtr data)
        {
            _wrapper.Core.PerformanceLevel = data.ReadInt32();
            return true;
        }

        private bool SetPixelFormat(IntPtr data)
        {
            if (data.IsNull())
                return false;

            int pixelFormat = data.ReadInt32();
            _wrapper.Graphics.PixelFormat = pixelFormat switch
            {
                0 or 1 or 2 => (retro_pixel_format)pixelFormat,
                _ => retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN
            };
            return _wrapper.Graphics.PixelFormat != retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN;
        }

        private bool SetInputDescriptors(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _inputDescriptors.Clear();

            retro_input_descriptor descriptor = data.ToStructure<retro_input_descriptor>();
            while (descriptor is not null && !descriptor.desc.IsNull())
            {
                _inputDescriptors.Add(descriptor);

                if (descriptor.device is RETRO_DEVICE.JOYPAD)
                {
                    string desc = descriptor.desc.AsString();
                    _wrapper.Input.ButtonDescriptions[descriptor.port, descriptor.id] = desc;
                }
                else if (descriptor.device is RETRO_DEVICE.ANALOG)
                {
                    RETRO_DEVICE_ID_ANALOG id = (RETRO_DEVICE_ID_ANALOG)descriptor.id;
                    switch (id)
                    {
                        case RETRO_DEVICE_ID_ANALOG.X:
                        {
                            RETRO_DEVICE_INDEX_ANALOG index = (RETRO_DEVICE_INDEX_ANALOG)descriptor.index;
                            switch (index)
                            {
                                case RETRO_DEVICE_INDEX_ANALOG.LEFT:
                                {
                                    string desc = descriptor.desc.AsString();
                                    _wrapper.Input.ButtonDescriptions[descriptor.port, (int)Input.CustomBinds.ANALOG_LEFT_X_PLUS]  = desc;
                                    _wrapper.Input.ButtonDescriptions[descriptor.port, (int)Input.CustomBinds.ANALOG_LEFT_X_MINUS] = desc;
                                }
                                break;
                                case RETRO_DEVICE_INDEX_ANALOG.RIGHT:
                                {
                                    string desc = descriptor.desc.AsString();
                                    _wrapper.Input.ButtonDescriptions[descriptor.port, (int)Input.CustomBinds.ANALOG_RIGHT_X_PLUS]  = desc;
                                    _wrapper.Input.ButtonDescriptions[descriptor.port, (int)Input.CustomBinds.ANALOG_RIGHT_X_MINUS] = desc;
                                }
                                break;
                            }
                        }
                        break;
                        case RETRO_DEVICE_ID_ANALOG.Y:
                        {
                            RETRO_DEVICE_INDEX_ANALOG index = (RETRO_DEVICE_INDEX_ANALOG)descriptor.index;
                            switch (index)
                            {
                                case RETRO_DEVICE_INDEX_ANALOG.LEFT:
                                {
                                    string desc = descriptor.desc.AsString();
                                    _wrapper.Input.ButtonDescriptions[descriptor.port, (int)Input.CustomBinds.ANALOG_LEFT_Y_PLUS]  = desc;
                                    _wrapper.Input.ButtonDescriptions[descriptor.port, (int)Input.CustomBinds.ANALOG_LEFT_Y_MINUS] = desc;
                                }
                                break;
                                case RETRO_DEVICE_INDEX_ANALOG.RIGHT:
                                {
                                    string desc = descriptor.desc.AsString();
                                    _wrapper.Input.ButtonDescriptions[descriptor.port, (int)Input.CustomBinds.ANALOG_RIGHT_Y_PLUS]  = desc;
                                    _wrapper.Input.ButtonDescriptions[descriptor.port, (int)Input.CustomBinds.ANALOG_RIGHT_Y_MINUS] = desc;
                                }
                                break;
                            }
                        }
                        break;
                    }
                }

                data += Marshal.SizeOf(descriptor);
                data.ToStructure(descriptor);
            }

            _wrapper.Input.HasInputDescriptors = true;
            return true;
        }

        private bool SetKeyboardCallback(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _wrapper.Input.KeyboardCallback = data.ToStructure<retro_keyboard_callback>();
            return true;
        }

        private bool SetDiskControlInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_disk_control_callback callback = data.ToStructure<retro_disk_control_callback>();
            _wrapper.Disk = new(_wrapper, callback);
            return true;
        }

        private bool SetHwRender(IntPtr data)
        {
            if (data.IsNull() || _wrapper.Core.HwAccelerated)
                return false;

            retro_hw_render_callback callback = data.ToStructure<retro_hw_render_callback>();
            if (callback.context_type is not retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL and not retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE)
                return false;

            _wrapper.OpenGL = new();
            if (!_wrapper.OpenGL.Init())
                return false;

            callback.get_current_framebuffer = Marshal.GetFunctionPointerForDelegate(_wrapper.OpenGL.GetCurrentFrameBufferCallback);
            callback.get_proc_address        = Marshal.GetFunctionPointerForDelegate(_wrapper.OpenGL.GetProcAddressCallback);

            _wrapper.HwRenderInterface = callback;
            Marshal.StructureToPtr(_wrapper.HwRenderInterface, data, false);

            _wrapper.Core.HwAccelerated = true;
            return true;
        }

        private bool SetVariables(IntPtr data)
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

        private bool SetSupportNoGame(IntPtr data)
        {
            _wrapper.Core.SupportNoGame = data.IsTrue();
            return true;
        }

        private bool SetFrameTimeCallback(IntPtr data)
        {
            Logger.Instance.LogInfo("Using FrameTime Callback");
            _wrapper.FrameTimeInterface = data.ToStructure<retro_frame_time_callback>();
            return true;
        }

        private bool SetAudioCallback(IntPtr data)
        {
            _wrapper.Audio.AudioCallback = data.ToStructure<retro_audio_callback>();
            return true;
        }

        private bool SetSystemAvInfo(IntPtr data)
        {
            _wrapper.Game.SystemAVInfo = data.ToStructure<retro_system_av_info>();
            return true;
        }

        private bool SetSubsystemInfo(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _subsystemInfo.Clear();

            retro_subsystem_info subsystemInfo = data.ToStructure<retro_subsystem_info>();
            while (!subsystemInfo.desc.IsNull())
            {
                _subsystemInfo.Add(subsystemInfo);

                data += Marshal.SizeOf(subsystemInfo);
                data.ToStructure(subsystemInfo);
            }

            return true;
        }

        private bool SetControllerInfo(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _controllerInfo.Clear();
            _controllerDescriptions.Clear();

            retro_controller_info controllerInfo = data.ToStructure<retro_controller_info>();
            while (!controllerInfo.types.IsNull())
            {
                _controllerInfo.Add(controllerInfo);

                for (int deviceIndex = 0; deviceIndex < controllerInfo.num_types; ++deviceIndex)
                {
                    retro_controller_description controllerDescription = controllerInfo.types.ToStructure<retro_controller_description>();
                    _controllerDescriptions.Add(controllerDescription);

                    controllerInfo.types += Marshal.SizeOf(controllerDescription);
                    controllerInfo.types.ToStructure(controllerDescription);
                }

                data += Marshal.SizeOf(controllerInfo);
                data.ToStructure(controllerInfo);
            }

            return true;
        }

        private bool SetMemoryMaps(IntPtr data)
        {
            _wrapper.Memory = new MemoryMap(data.ToStructure<retro_memory_map>());
            return true;
        }

        private bool SetGeometry(IntPtr data)
        {
            if (data != null)
            {
                retro_game_geometry geometry = data.ToStructure<retro_game_geometry>();
                if (_wrapper.Game.SystemAVInfo.geometry.base_width != geometry.base_width
                 || _wrapper.Game.SystemAVInfo.geometry.base_height != geometry.base_height
                 || _wrapper.Game.SystemAVInfo.geometry.aspect_ratio != geometry.aspect_ratio)
                {
                    _wrapper.Game.SystemAVInfo.geometry = geometry;
                    // TODO: Set video aspect ratio if needed
                }
            }
            return true;
        }

        private bool SetSupportAchievements(IntPtr data)
        {
            _wrapper.Core.SupportsAchievements = data.IsTrue();
            return false;
        }

        private bool SetSerializationQuirks(IntPtr data)
        {
            ulong quirks = data.ReadUInt64();
            // quirks |= Header.RETRO_SERIALIZATION_QUIRK_FRONT_VARIABLE_SIZE;
            _wrapper.Serialization.SetQuirks(quirks);
            return true;
        }

        private bool SetCoreOptions(IntPtr data) =>
            SetCoreOptionsInternal(data);

        private bool SetCoreOptionsIntl(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_core_options_intl intl = data.ToStructure<retro_core_options_intl>();
            bool result = SetCoreOptionsInternal(intl.local);
            if (!result)
                result = SetCoreOptionsInternal(intl.us);
            return result;
        }

        private bool SetCoreOptionsDisplay(IntPtr data)
        {
            retro_core_option_display coreOptionDisplay = data.ToStructure<retro_core_option_display>();
            if (coreOptionDisplay.key.IsNull())
                return false;

            string key = coreOptionDisplay.key.AsString();
            _wrapper.Core.CoreOptions[key]?.SetVisibility(coreOptionDisplay.visible);
            return true;
        }

        private bool SetDiskControlExtInterface(IntPtr data)
        {
            retro_disk_control_ext_callback callback = data.ToStructure<retro_disk_control_ext_callback>();
            _wrapper.Disk = new(_wrapper, callback);
            return true;
        }

        private bool Shutdown(IntPtr _) =>
            false;

        private bool SetMessageExt(IntPtr data)
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

        private bool SetContentInfoOverride(IntPtr data)
        {
            _systemContentInfoOverrides.Clear();

            retro_system_content_info_override infoOverride = data.ToStructure<retro_system_content_info_override>();
            while (infoOverride is not null && infoOverride.extensions.IsNotNull())
            {
                _systemContentInfoOverrides.Add(infoOverride);

                string extensionsString = infoOverride.extensions.AsString();
                string[] extensions     = extensionsString.Split('|');
                foreach (string extension in extensions)
                    _wrapper.Game.ContentOverrides.Add(extension, infoOverride.need_fullpath, infoOverride.persistent_data);

                data += Marshal.SizeOf(infoOverride);
                data.ToStructure(infoOverride);
            }

            return true;
        }

        private bool SetCoreOptionsInternal(IntPtr data)
        {
            try
            {
                _wrapper.Core.DeserializeOptions();

                if (data == IntPtr.Zero)
                    return false;

                Type type = typeof(retro_core_option_values);
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                FieldInfo[] fields = type.GetFields(bindingFlags);

                retro_core_option_definition optionDefinition = Marshal.PtrToStructure<retro_core_option_definition>(data);
                while (optionDefinition is not null && optionDefinition.key != IntPtr.Zero)
                {
                    string key = Marshal.PtrToStringAnsi(optionDefinition.key);
                    string description = optionDefinition.desc != IntPtr.Zero ? Marshal.PtrToStringAnsi(optionDefinition.desc) : "";
                    string info = optionDefinition.info != IntPtr.Zero ? Marshal.PtrToStringAnsi(optionDefinition.info) : "";
                    string defaultValue = optionDefinition.default_value != IntPtr.Zero ? Marshal.PtrToStringAnsi(optionDefinition.default_value) : "";

                    List<string> possibleValues = new();
                    for (int i = 0; i < fields.Length; ++i)
                    {
                        FieldInfo fieldInfo = fields[i];
                        if (fieldInfo.GetValue(optionDefinition.values) is not retro_core_option_value optionValue || optionValue.value == IntPtr.Zero)
                            continue;

                        possibleValues.Add(Marshal.PtrToStringAnsi(optionValue.value));
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
                    Marshal.PtrToStructure(data, optionDefinition);
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
