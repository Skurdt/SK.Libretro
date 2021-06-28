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

using SK.Libretro.Utilities;
using SK.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroEnvironment
    {
        public bool UpdateVariables = false;

        private readonly LibretroWrapper _wrapper;

        // TEMP_HACK
        private sealed class CoresUsingOptionsIntlList
        {
            public List<string> Cores = null;
        }

        public LibretroEnvironment(LibretroWrapper wrapper) => _wrapper = wrapper;

        public unsafe bool Callback(retro_environment cmd, void* data)
        {
            switch (cmd)
            {
                /************************************************************************************************
                 * Data passed from the frontend to the core
                 */
                case retro_environment.RETRO_ENVIRONMENT_GET_OVERSCAN:                                return GetOverscan();
                case retro_environment.RETRO_ENVIRONMENT_GET_CAN_DUPE:                                return GetCanDupe();
                case retro_environment.RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY:                        return GetSystemDirectory();
                case retro_environment.RETRO_ENVIRONMENT_GET_VARIABLE:                                return GetVariable();
                case retro_environment.RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE:                         return GetVariableUpdate();
                case retro_environment.RETRO_ENVIRONMENT_GET_LIBRETRO_PATH:                           return GetLibretroPath();
                case retro_environment.RETRO_ENVIRONMENT_GET_RUMBLE_INTERFACE:                        return ENVIRONMENT_NOT_IMPLEMENTED(true);
                case retro_environment.RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES:               return GetInputDeviceCapabilities();
                case retro_environment.RETRO_ENVIRONMENT_GET_SENSOR_INTERFACE:                        return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_CAMERA_INTERFACE:                        return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_LOG_INTERFACE:                           return GetLogInterface();
                case retro_environment.RETRO_ENVIRONMENT_GET_PERF_INTERFACE:                          return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_LOCATION_INTERFACE:                      return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_CORE_ASSETS_DIRECTORY:                   return GetCoreAssetsDirectory();
                case retro_environment.RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:                          return GetSaveDirectory();
                case retro_environment.RETRO_ENVIRONMENT_GET_USERNAME:                                return GetUsername();
                case retro_environment.RETRO_ENVIRONMENT_GET_LANGUAGE:                                return GetLanguage();
                case retro_environment.RETRO_ENVIRONMENT_GET_CURRENT_SOFTWARE_FRAMEBUFFER:            return GetCurrentSoftwareFramebuffer();
                case retro_environment.RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE:                     return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_VFS_INTERFACE:                           return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_LED_INTERFACE:                           return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_AUDIO_VIDEO_ENABLE:                      return GetAudioVideoEnable();
                case retro_environment.RETRO_ENVIRONMENT_GET_MIDI_INTERFACE:                          return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_FASTFORWARDING:                          return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_TARGET_REFRESH_RATE:                     return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_INPUT_BITMASKS:                          return GetInputBitmasks();
                case retro_environment.RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION:                    return GetCoreOptionsVersion();
                case retro_environment.RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER:                     return GetPreferredHwRender();
                case retro_environment.RETRO_ENVIRONMENT_GET_DISK_CONTROL_INTERFACE_VERSION:          return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_MESSAGE_INTERFACE_VERSION:               return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_INPUT_MAX_USERS:                         return ENVIRONMENT_NOT_IMPLEMENTED();

                /************************************************************************************************
                 * Data passed from the core to the frontend
                 */
                case retro_environment.RETRO_ENVIRONMENT_SET_ROTATION:                                return SetRotation();
                case retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE:                                 return SetMessage();
                case retro_environment.RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL:                       return SetPerformanceLevel();
                case retro_environment.RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:                            return SetPixelFormat();
                case retro_environment.RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS:                       return SetInputDescriptors();
                case retro_environment.RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK:                       return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE:                  return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_HW_RENDER:                               return SetHwRender();
                case retro_environment.RETRO_ENVIRONMENT_SET_VARIABLES:                               return SetVariables();
                case retro_environment.RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME:                         return SetSupportNoGame();
                case retro_environment.RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK:                     return SetFrameTimeCallback();
                case retro_environment.RETRO_ENVIRONMENT_SET_AUDIO_CALLBACK:                          return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO:                          return SetSystemAvInfo();
                case retro_environment.RETRO_ENVIRONMENT_SET_PROC_ADDRESS_CALLBACK:                   return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_SUBSYSTEM_INFO:                          return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_CONTROLLER_INFO:                         return SetControllerInfo();
                case retro_environment.RETRO_ENVIRONMENT_SET_MEMORY_MAPS:                             return ENVIRONMENT_NOT_IMPLEMENTED(true);
                case retro_environment.RETRO_ENVIRONMENT_SET_GEOMETRY:                                return SetGeometry();
                case retro_environment.RETRO_ENVIRONMENT_SET_SUPPORT_ACHIEVEMENTS:                    return ENVIRONMENT_NOT_IMPLEMENTED(true);
                case retro_environment.RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE: return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_SERIALIZATION_QUIRKS:                    return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_HW_SHARED_CONTEXT:                       return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS:                            return SetCoreOptions();
                case retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS_INTL:                       return SetCoreOptionsIntl();
                case retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS_DISPLAY:                    return ENVIRONMENT_NOT_IMPLEMENTED(true);
                case retro_environment.RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE:              return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SHUTDOWN:                                    return Shutdown();
                case retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT:                             return ENVIRONMENT_NOT_IMPLEMENTED();

                /************************************************************************************************
                 * RetroArch Extensions
                 */
                case retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND:                return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_CLEAR_ALL_THREAD_WAITS_CB:               return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_POLL_TYPE_OVERRIDE:                          return ENVIRONMENT_NOT_IMPLEMENTED();

                default:
                {
                    Logger.Instance.LogError($"Environment unknown: {cmd}", "LibretroEnvironment.Callback");
                    return false;
                }
            }

            /************************************************************************************************
             * Temporary placeholder... hopefully...
             */
            bool ENVIRONMENT_NOT_IMPLEMENTED(bool defaultReturns = false)
            {
                if (defaultReturns)
                    Logger.Instance.LogWarning("Environment not implemented!", cmd.ToString());
                else
                    Logger.Instance.LogError("Environment not implemented!", cmd.ToString());
                return defaultReturns;
            }

            /************************************************************************************************
             * Data passed from the frontend to the core
             */
            #region FrontendToCore
            bool GetOverscan()
            {
                if (data != null)
                    *(bool*)data = _wrapper.OptionCropOverscan;
                Logger.Instance.LogInfo($"-> Crop Overscan: {_wrapper.OptionCropOverscan}", $"{cmd}");
                return true;
            }

            bool GetCanDupe()
            {
                if (data != null)
                    *(bool*)data = true;
                Logger.Instance.LogInfo("-> CanDupe: true", $"{cmd}");
                return true;
            }

            bool GetSystemDirectory()
            {
                string path = FileSystem.GetAbsolutePath(Path.Combine(LibretroWrapper.SystemDirectory, _wrapper.Core.Name));
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(path);
                Logger.Instance.LogInfo($"-> SystemDirectory: {path}", $"{cmd}");
                return true;
            }

            bool GetVariable()
            {
                if (data == null)
                {
                    Logger.Instance.LogWarning($"Variable data == null.", $"{cmd}");
                    return false;
                }

                retro_variable* outVariable = (retro_variable*)data;
                string key                  = UnsafeStringUtils.CharsToString(outVariable->key);

                if (_wrapper.Core.CoreOptions is null)
                {
                    Logger.Instance.LogWarning($"Core didn't set its options. Requested key: {key}", $"{cmd}");
                    return false;
                }

                string coreOption = _wrapper.Core.CoreOptions.Options.Find(x => x.StartsWith(key, StringComparison.OrdinalIgnoreCase));
                if (coreOption is null)
                {
                    Logger.Instance.LogWarning($"Core option '{key}' not found.", $"{cmd}");
                    return false;
                }

                outVariable->value = _wrapper.GetUnsafeString(coreOption.Split(';')[1]);
                return true;
            }

            bool GetVariableUpdate()
            {
                if (data != null)
                {
                    *(bool*)data    = UpdateVariables;
                    UpdateVariables = false;
                }
                return true;
            }

            bool GetLibretroPath()
            {
                string path = FileSystem.GetAbsolutePath(Path.Combine(LibretroWrapper.CoresDirectory, _wrapper.Core.Name));
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(path);
                Logger.Instance.LogInfo($"-> LibretroPath: {path}", $"{cmd}");
                return true;
            }

            bool GetInputDeviceCapabilities()
            {
                if (data != null)
                    *(ulong*)data = (1 << (int)RETRO_DEVICE_JOYPAD) | (1 << (int)RETRO_DEVICE_ANALOG);
                return true;
            }

            bool GetLogInterface()
            {
                if (data != null)
                    ((retro_log_callback*)data)->log = Marshal.GetFunctionPointerForDelegate<retro_log_printf_t>(LibretroLog.RetroLogPrintf);
                return true;
            }

            bool GetCoreAssetsDirectory()
            {
                string path = FileSystem.GetAbsolutePath(Path.Combine(LibretroWrapper.CoreAssetsDirectory, _wrapper.Core.Name));
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(path);
                Logger.Instance.LogInfo($"-> CoreAssetsDirectory: {path}", $"{cmd}");
                return true;
            }

            bool GetSaveDirectory()
            {
                string path = FileSystem.GetAbsolutePath(Path.Combine(LibretroWrapper.SavesDirectory, _wrapper.Core.Name));
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(path);
                Logger.Instance.LogInfo($"-> SaveDirectory: {path}", $"{cmd}");
                return true;
            }

            bool GetUsername()
            {
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(_wrapper.OptionUserName);
                Logger.Instance.LogInfo($"-> UserName: {_wrapper.OptionUserName}", $"{cmd}");
                return true;
            }

            bool GetLanguage()
            {
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(_wrapper.OptionLanguage.ToString());
                Logger.Instance.LogInfo($"-> Language: {_wrapper.OptionLanguage}", $"{cmd}");
                return true;
            }

            bool GetCurrentSoftwareFramebuffer() => false;

            bool GetAudioVideoEnable()
            {
                if (data != null)
                {
                    int mask = 0;
                    mask |= 1; // if video enabled
                    mask |= 2; // if audio enabled
                    *(int*)data = mask;
                }
                return true;
            }

            bool GetInputBitmasks()
            {
                if (data != null)
                    *(bool*)data = false;
                Logger.Instance.LogInfo("-> Input Bitmasks: False", $"{cmd}");
                return false;
            }

            bool GetCoreOptionsVersion()
            {
                if (data == null)
                    return false;

                // TEMP_HACK
                string filePath = Path.GetFullPath(Path.Combine(LibretroWrapper.MainDirectory, "cores_using_options_intl.json"));
                CoresUsingOptionsIntlList coresUsingOptionsIntl = FileSystem.DeserializeFromJson<CoresUsingOptionsIntlList>(filePath);
                *(uint*)data = coresUsingOptionsIntl is null || coresUsingOptionsIntl.Cores is null || !coresUsingOptionsIntl.Cores.Contains(_wrapper.Core.Name)
                             ? RETRO_API_VERSION
                             : 0;
                return true;
            }

            bool GetPreferredHwRender()
            {
                //*(uint*)data = (uint)retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL;
                //return true;

                *(uint*)data = (uint)retro_hw_context_type.RETRO_HW_CONTEXT_NONE;
                return true;
            }
            #endregion

            /************************************************************************************************
            / Data passed from the core to the frontend
            /***********************************************************************************************/
            #region CoreToFrontend
            bool SetRotation()
            {
                // Values: 0,  1,   2,   3
                // Result: 0, 90, 180, 270 degrees
                if (data != null)
                    _wrapper.Core.Rotation = (int)*(uint*)data * 90;
                Logger.Instance.LogInfo($"<- Rotation: {_wrapper.Core.Rotation}", $"{cmd}");
                // return true;
                return false;
            }

            // TODO(Tom): Do I need something from this?
            bool SetMessage()
            {
                if (data != null)
                    Logger.Instance.LogWarning($"<- Message: {UnsafeStringUtils.CharsToString(((retro_message*)data)->msg)}", $"{cmd}");
                return true;
            }

            bool SetPerformanceLevel()
            {
                if (data != null)
                    _wrapper.Core.PerformanceLevel = *(int*)data;
                Logger.Instance.LogInfo($"<- PerformanceLevel: {_wrapper.Core.PerformanceLevel}", $"{cmd}");
                return true;
            }

            bool SetPixelFormat()
            {
                if (data == null)
                    return false;

                retro_pixel_format* inPixelFormat = (retro_pixel_format*)data;
                switch (*inPixelFormat)
                {
                    case retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555:
                    case retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888:
                    case retro_pixel_format.RETRO_PIXEL_FORMAT_RGB565:
                    {
                        _wrapper.Game.PixelFormat = *inPixelFormat;
                        Logger.Instance.LogInfo($"<- PixelFormat: {_wrapper.Game.PixelFormat}", $"{cmd}");
                        return true;
                    }
                }

                return false;
            }

            bool SetInputDescriptors()
            {
                if (data == null)
                    return true;

                retro_input_descriptor* inInputDescriptors = (retro_input_descriptor*)data;
                uint id;
                for (; inInputDescriptors->desc != null; ++inInputDescriptors)
                {
                    uint port = inInputDescriptors->port;
                    if (port >= LibretroInput.MAX_USERS)
                        continue;

                    uint device = inInputDescriptors->device;
                    if (device != RETRO_DEVICE_JOYPAD && device != RETRO_DEVICE_ANALOG)
                        continue;

                    id = inInputDescriptors->id;
                    if (id >= LibretroInput.FIRST_CUSTOM_BIND)
                        continue;

                    string descText = UnsafeStringUtils.CharsToString(inInputDescriptors->desc);
                    uint index = inInputDescriptors->index;
                    if (device == RETRO_DEVICE_ANALOG)
                    {
                        switch (id)
                        {
                            case RETRO_DEVICE_ID_ANALOG_X:
                            {
                                switch (index)
                                {
                                    case RETRO_DEVICE_INDEX_ANALOG_LEFT:
                                    {
                                        _wrapper.Game.ButtonDescriptions[port, (int)LibretroInput.CustomBinds.ANALOG_LEFT_X_PLUS] = descText;
                                        _wrapper.Game.ButtonDescriptions[port, (int)LibretroInput.CustomBinds.ANALOG_LEFT_X_MINUS] = descText;
                                    }
                                    break;
                                    case RETRO_DEVICE_INDEX_ANALOG_RIGHT:
                                    {
                                        _wrapper.Game.ButtonDescriptions[port, (int)LibretroInput.CustomBinds.ANALOG_RIGHT_X_PLUS] = descText;
                                        _wrapper.Game.ButtonDescriptions[port, (int)LibretroInput.CustomBinds.ANALOG_RIGHT_X_MINUS] = descText;
                                    }
                                    break;
                                }
                            }
                            break;
                            case RETRO_DEVICE_ID_ANALOG_Y:
                            {
                                switch (index)
                                {
                                    case RETRO_DEVICE_INDEX_ANALOG_LEFT:
                                    {
                                        _wrapper.Game.ButtonDescriptions[port, (int)LibretroInput.CustomBinds.ANALOG_LEFT_Y_PLUS] = descText;
                                        _wrapper.Game.ButtonDescriptions[port, (int)LibretroInput.CustomBinds.ANALOG_LEFT_Y_MINUS] = descText;
                                    }
                                    break;
                                    case RETRO_DEVICE_INDEX_ANALOG_RIGHT:
                                    {
                                        _wrapper.Game.ButtonDescriptions[port, (int)LibretroInput.CustomBinds.ANALOG_RIGHT_Y_PLUS] = descText;
                                        _wrapper.Game.ButtonDescriptions[port, (int)LibretroInput.CustomBinds.ANALOG_RIGHT_Y_MINUS] = descText;
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    else
                        _wrapper.Game.ButtonDescriptions[port, id] = descText;
                }

                _wrapper.Game.HasInputDescriptors = true;

                return true;
            }

            bool SetHwRender()
            {
                if (data == null || _wrapper.Core.HwAccelerated)
                    return false;

                retro_hw_render_callback* inCallback = (retro_hw_render_callback*)data;

                if (inCallback->context_type != retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL && inCallback->context_type != retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE)
                    return false;

                _wrapper.OpenGL = new LibretroOpenGL();
                if (!_wrapper.OpenGL.Init())
                    return false;

                inCallback->get_current_framebuffer = Marshal.GetFunctionPointerForDelegate(_wrapper.OpenGL.GetCurrentFrameBufferCallback);
                inCallback->get_proc_address        = Marshal.GetFunctionPointerForDelegate(_wrapper.OpenGL.GetProcAddressCallback);

                _wrapper.HwRenderInterface = Marshal.PtrToStructure<retro_hw_render_callback>((IntPtr)data);

                _wrapper.Core.HwAccelerated = true;

                return true;
            }

            bool SetVariables()
            {
                if (data == null)
                    return true;

                try
                {
                    retro_variable* inVariable = (retro_variable*)data;

                    _wrapper.Core.CoreOptions = LibretroWrapper.CoreOptionsList.Cores.Find(x => x.CoreName.Equals(_wrapper.Core.Name, StringComparison.OrdinalIgnoreCase));
                    if (_wrapper.Core.CoreOptions is null)
                    {
                        _wrapper.Core.CoreOptions = new LibretroCoreOptions { CoreName = _wrapper.Core.Name };
                        LibretroWrapper.CoreOptionsList.Cores.Add(_wrapper.Core.CoreOptions);
                    }

                    while (inVariable->key != null)
                    {
                        string key        = UnsafeStringUtils.CharsToString(inVariable->key);
                        string coreOption = _wrapper.Core.CoreOptions.Options.Find(x => x.StartsWith(key, StringComparison.OrdinalIgnoreCase));
                        if (coreOption is null)
                        {
                            string inValue                = UnsafeStringUtils.CharsToString(inVariable->value);
                            string[] descriptionAndValues = inValue.Split(';');
                            string[] possibleValues       = descriptionAndValues[1].Trim().Split('|');
                            string defaultValue           = possibleValues[0];
                            string value                  = defaultValue;
                            coreOption                    = $"{key};{value};{string.Join("|", possibleValues)};";
                            _wrapper.Core.CoreOptions.Options.Add(coreOption);
                        }
                        ++inVariable;
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.LogException(e);
                }

                LibretroWrapper.SaveCoreOptionsFile();

                return true;
            }

            bool SetSupportNoGame()
            {
                if (data != null)
                    _wrapper.Core.SupportNoGame = *(bool*)data;
                return true;
            }

            bool SetFrameTimeCallback()
            {
                if (data != null)
                    _wrapper.FrameTimeInterface = *(retro_frame_time_callback*)data;
                return true;
            }

            bool SetSystemAvInfo()
            {
                if (data != null)
                    _wrapper.Game.SystemAVInfo = *(retro_system_av_info*)data;
                return true;
            }

            bool SetControllerInfo()
            {
                if (data == null)
                    return true;

                retro_controller_info* inControllerInfo = (retro_controller_info*)data;

                int numPorts;
                for (numPorts = 0; inControllerInfo[numPorts].types != null; ++numPorts)
                {
                    Logger.Instance.LogInfo($"# Controller port: {numPorts + 1}", $"{cmd}");
                    for (int j = 0; j < inControllerInfo[numPorts].num_types; ++j)
                    {
                        string desc = UnsafeStringUtils.CharsToString(inControllerInfo[numPorts].types[j].desc);
                        uint id     = inControllerInfo[numPorts].types[j].id;
                        Logger.Instance.LogInfo($"    {desc} (ID: {id})", $"{cmd}");
                    }
                }

                _wrapper.Core.ControllerPorts = new retro_controller_info[numPorts];
                for (int j = 0; j < numPorts; ++j)
                    _wrapper.Core.ControllerPorts[j] = inControllerInfo[j];

                return true;
            }

            bool SetGeometry()
            {
                if (data != null)
                {
                    retro_game_geometry* inGeometry = (retro_game_geometry*)data;

                    if (_wrapper.Game.SystemAVInfo.geometry.base_width != inGeometry->base_width
                     || _wrapper.Game.SystemAVInfo.geometry.base_height != inGeometry->base_height
                     || _wrapper.Game.SystemAVInfo.geometry.aspect_ratio != inGeometry->aspect_ratio)
                    {
                        _wrapper.Game.SystemAVInfo.geometry = *inGeometry;
                        // TODO: Set video aspect ratio
                    }
                }

                return true;
            }

            bool SetCoreOptions() => data != null && SetCoreOptionsInternal((long)data);

            // FIXME: I can't read into the pointer to the retro_core_options_intl.us array at this time.
            bool SetCoreOptionsIntl()
            {
                if (data == null)
                    return false;

                retro_core_options_intl inOptionsIntl = Marshal.PtrToStructure<retro_core_options_intl>((IntPtr)data);
                return SetCoreOptionsInternal(inOptionsIntl.us.ToInt64());
            }

            bool Shutdown() => true;
            #endregion
        }

        private unsafe bool SetCoreOptionsInternal(long data)
        {
            _wrapper.Core.CoreOptions = LibretroWrapper.CoreOptionsList.Cores.Find(x => x.CoreName.Equals(_wrapper.Core.Name, StringComparison.OrdinalIgnoreCase));
            if (_wrapper.Core.CoreOptions is null)
            {
                _wrapper.Core.CoreOptions = new LibretroCoreOptions { CoreName = _wrapper.Core.Name };
                LibretroWrapper.CoreOptionsList.Cores.Add(_wrapper.Core.CoreOptions);
            }

            try
            {
                for (int i = 0; i < RETRO_NUM_CORE_OPTION_VALUES_MAX; ++i)
                {
                    IntPtr ins = new IntPtr(data + (i * Marshal.SizeOf<retro_core_option_definition>()));
                    retro_core_option_definition defs = Marshal.PtrToStructure<retro_core_option_definition>(ins);
                    if (defs.key == null)
                        break;

                    string key = UnsafeStringUtils.CharsToString(defs.key);

                    string coreOption = _wrapper.Core.CoreOptions.Options.Find(x => x.StartsWith(key, StringComparison.OrdinalIgnoreCase));
                    if (!(coreOption is null))
                        continue;

                    string defaultValue = UnsafeStringUtils.CharsToString(defs.default_value);

                    List<string> possibleValues = new List<string>();
                    for (int j = 0; j < defs.values.Length; j++)
                    {
                        retro_core_option_value val = defs.values[j];
                        if (val.value != null)
                            possibleValues.Add(UnsafeStringUtils.CharsToString(val.value));
                    }

                    string value = "";
                    if (!string.IsNullOrEmpty(defaultValue))
                        value = defaultValue;
                    else if (possibleValues.Count > 0)
                        value = possibleValues[0];

                    coreOption = $"{key};{value};{string.Join("|", possibleValues)}";

                    _wrapper.Core.CoreOptions.Options.Add(coreOption);
                }

                LibretroWrapper.SaveCoreOptionsFile();
            }
            catch (Exception e)
            {
                Logger.Instance.LogError($"Failed to retrieve and write the core options, they must be entered manually in the configuration file for now... (Exception message: {e.Message})");
            }

            return true;
        }
    }
}
