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
using System.IO;
using System.Runtime.InteropServices;
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroEnvironment
    {
        public bool UpdateVariables = false;

        private readonly LibretroWrapper _wrapper;

        // TODO(Tom): Should probably not be here...
        private bool _supportsAchievements;

        public LibretroEnvironment(LibretroWrapper wrapper)
        {
            _wrapper = wrapper;
        }

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
                case retro_environment.RETRO_ENVIRONMENT_GET_RUMBLE_INTERFACE:                        return GetRumbleInterface();
                case retro_environment.RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES:               return GetInputDeviceCapabilities();
                case retro_environment.RETRO_ENVIRONMENT_GET_SENSOR_INTERFACE:                        return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_CAMERA_INTERFACE:                        return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_LOG_INTERFACE:                           return GetLogInterface();
                case retro_environment.RETRO_ENVIRONMENT_GET_PERF_INTERFACE:                          return GetPerfInterface();
                case retro_environment.RETRO_ENVIRONMENT_GET_LOCATION_INTERFACE:                      return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_CORE_ASSETS_DIRECTORY:                   return GetCoreAssetsDirectory();
                case retro_environment.RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:                          return GetSaveDirectory();
                case retro_environment.RETRO_ENVIRONMENT_GET_USERNAME:                                return GetUsername();
                case retro_environment.RETRO_ENVIRONMENT_GET_LANGUAGE:                                return GetLanguage();
                case retro_environment.RETRO_ENVIRONMENT_GET_CURRENT_SOFTWARE_FRAMEBUFFER:            return ENVIRONMENT_NOT_IMPLEMENTED(false, false);
                case retro_environment.RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE:                     return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_VFS_INTERFACE:                           return GetVfsInterface();
                case retro_environment.RETRO_ENVIRONMENT_GET_LED_INTERFACE:                           return GetLedInterface();
                case retro_environment.RETRO_ENVIRONMENT_GET_AUDIO_VIDEO_ENABLE:                      return GetAudioVideoEnable();
                case retro_environment.RETRO_ENVIRONMENT_GET_MIDI_INTERFACE:                          return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_FASTFORWARDING:                          return GetFastForwarding();
                case retro_environment.RETRO_ENVIRONMENT_GET_TARGET_REFRESH_RATE:                     return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_GET_INPUT_BITMASKS:                          return GetInputBitmasks();
                case retro_environment.RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION:                    return GetCoreOptionsVersion();
                case retro_environment.RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER:                     return GetPreferredHwRender();
                case retro_environment.RETRO_ENVIRONMENT_GET_DISK_CONTROL_INTERFACE_VERSION:          return GetDiskControlInterfaceVersion();
                case retro_environment.RETRO_ENVIRONMENT_GET_MESSAGE_INTERFACE_VERSION:               return GetMessageInterfaceVersion();
                case retro_environment.RETRO_ENVIRONMENT_GET_INPUT_MAX_USERS:                         return ENVIRONMENT_NOT_IMPLEMENTED();

                /************************************************************************************************
                 * Data passed from the core to the frontend
                 */
                case retro_environment.RETRO_ENVIRONMENT_SET_ROTATION:                                return SetRotation();
                case retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE:                                 return SetMessage();
                case retro_environment.RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL:                       return SetPerformanceLevel();
                case retro_environment.RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:                            return SetPixelFormat();
                case retro_environment.RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS:                       return SetInputDescriptors();
                case retro_environment.RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK:                       return SetKeyboardCallback();
                case retro_environment.RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE:                  return SetDiskControlInterface();
                case retro_environment.RETRO_ENVIRONMENT_SET_HW_RENDER:                               return SetHwRender();
                case retro_environment.RETRO_ENVIRONMENT_SET_VARIABLES:                               return SetVariables();
                case retro_environment.RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME:                         return SetSupportNoGame();
                case retro_environment.RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK:                     return SetFrameTimeCallback();
                case retro_environment.RETRO_ENVIRONMENT_SET_AUDIO_CALLBACK:                          return SetAudioCallback();
                case retro_environment.RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO:                          return SetSystemAvInfo();
                case retro_environment.RETRO_ENVIRONMENT_SET_PROC_ADDRESS_CALLBACK:                   return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_SUBSYSTEM_INFO:                          return SetSubsystemInfo();
                case retro_environment.RETRO_ENVIRONMENT_SET_CONTROLLER_INFO:                         return SetControllerInfo();
                case retro_environment.RETRO_ENVIRONMENT_SET_MEMORY_MAPS:                             return SetMemoryMaps();
                case retro_environment.RETRO_ENVIRONMENT_SET_GEOMETRY:                                return SetGeometry();
                case retro_environment.RETRO_ENVIRONMENT_SET_SUPPORT_ACHIEVEMENTS:                    return SetSupportAchievements();
                case retro_environment.RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE: return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_SERIALIZATION_QUIRKS:                    return SetSerializationQuirks();
                case retro_environment.RETRO_ENVIRONMENT_SET_HW_SHARED_CONTEXT:                       return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS:                            return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS_INTL:                       return ENVIRONMENT_NOT_IMPLEMENTED();
                case retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS_DISPLAY:                    return ENVIRONMENT_NOT_IMPLEMENTED(true, false);
                case retro_environment.RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE:              return SetDiskControlExtInterface();
                case retro_environment.RETRO_ENVIRONMENT_SHUTDOWN:                                    return Shutdown();
                case retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT:                             return SetMessageExt();

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
            bool ENVIRONMENT_NOT_IMPLEMENTED(bool defaultReturns = false, bool log = true)
            {
                if (!log)
                    return defaultReturns;

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
                //Logger.Instance.LogInfo($"-> Crop Overscan: {_wrapper.OptionCropOverscan}", $"{cmd}");
                return true;
            }

            bool GetCanDupe()
            {
                if (data != null)
                    *(bool*)data = true;
                //Logger.Instance.LogInfo($"-> Can Dupe: True", $"{cmd}");
                return true;
            }

            bool GetSystemDirectory()
            {
                string path = FileSystem.GetAbsolutePath(Path.Combine(LibretroWrapper.SystemDirectory, _wrapper.Core.Name));
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(path);
                //Logger.Instance.LogInfo($"-> SystemDirectory: {path}", $"{cmd}");
                return true;
            }

            bool GetVariable()
            {
                if (data == null)
                {
                    Logger.Instance.LogWarning($"Variable data is null.", $"{cmd}");
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
                    *(bool*)data = UpdateVariables;
                UpdateVariables = false;
                return true;
            }

            bool GetLibretroPath()
            {
                string path = FileSystem.GetAbsolutePath(Path.Combine(LibretroWrapper.CoresDirectory, _wrapper.Core.Name));
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(path);
                //Logger.Instance.LogInfo($"-> LibretroPath: {path}", $"{cmd}");
                return true;
            }

            bool GetRumbleInterface()
            {
                Marshal.StructureToPtr(_wrapper.Input.RumbleInterface, (IntPtr)data, true);
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

            bool GetPerfInterface()
            {
                _wrapper.Perf = new LibretroPerfInterface((IntPtr)data);
                return true;
            }

            bool GetCoreAssetsDirectory()
            {
                string path = FileSystem.GetAbsolutePath(Path.Combine(LibretroWrapper.CoreAssetsDirectory, _wrapper.Core.Name));
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(path);
                //Logger.Instance.LogInfo($"-> CoreAssetsDirectory: {path}", $"{cmd}");
                return true;
            }

            bool GetSaveDirectory()
            {
                string path = FileSystem.GetAbsolutePath(Path.Combine(LibretroWrapper.SavesDirectory, _wrapper.Core.Name));
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(path);
                //Logger.Instance.LogInfo($"-> SaveDirectory: {path}", $"{cmd}");
                return true;
            }

            bool GetUsername()
            {
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(_wrapper.OptionUserName);
                //Logger.Instance.LogInfo($"-> UserName: {_wrapper.OptionUserName}", $"{cmd}");
                return true;
            }

            bool GetLanguage()
            {
                if (data != null)
                    *(char**)data = _wrapper.GetUnsafeString(_wrapper.OptionLanguage.ToString());
                //Logger.Instance.LogInfo($"-> Language: {_wrapper.OptionLanguage}", $"{cmd}");
                return true;
            }

            bool GetVfsInterface()
            {
                if (data != null)
                {
                    retro_vfs_interface_info* interfaceInfo = (retro_vfs_interface_info*)data;
                    interfaceInfo->iface = IntPtr.Zero;
                    Logger.Instance.LogWarning($"VFS not implemented (Core asked for VFS API v{interfaceInfo->required_interface_version}).");
                }
                return false;
            }

            bool GetLedInterface()
            {
                _wrapper.Led = new LibretroLedInterface((IntPtr)data);
                return true;
            }

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

            bool GetFastForwarding()
            {
                if (data != null)
                    *(bool*)data = false;
                return false;
            }

            bool GetInputBitmasks()
            {
                if (data != null)
                    *(bool*)data = true;
                return true;
            }

            bool GetCoreOptionsVersion()
            {
                if (data != null)
                    *(uint*)data = 0;

                return true;
            }

            bool GetPreferredHwRender()
            {
                if (data != null)
                {
                    //*(uint*)data = (uint)retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL;
                    *(uint*)data = (uint)retro_hw_context_type.RETRO_HW_CONTEXT_NONE;
                }
                return true;
            }

            bool GetMessageInterfaceVersion()
            {
                if (data != null)
                    *(uint*)data = LibretroMessageInterface.VERSION;
                return true;
            }

            bool GetDiskControlInterfaceVersion()
            {
                if (data != null)
                    *(uint*)data = LibretroDiskInterface.VERSION;
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
                    Logger.Instance.LogInfo($"<- Message: {UnsafeStringUtils.CharsToString(((retro_message*)data)->msg)}", $"{cmd}");
                return true;
            }

            bool SetPerformanceLevel()
            {
                if (data != null)
                    _wrapper.Core.PerformanceLevel = *(int*)data;
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
                        //Logger.Instance.LogInfo($"<- PixelFormat: {_wrapper.Game.PixelFormat}", $"{cmd}");
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

            bool SetKeyboardCallback()
            {
                if (data == null)
                    return false;

                _wrapper.Input.KeyboardCallback = Marshal.PtrToStructure<retro_keyboard_callback>((IntPtr)data);
                return true;
            }

            bool SetDiskControlInterface()
            {
                if (data == null)
                    return false;

                retro_disk_control_callback inCallback = Marshal.PtrToStructure<retro_disk_control_callback>((IntPtr)data);
                _wrapper.Disk = new LibretroDiskInterface(inCallback);
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

                    _wrapper.Core.CoreOptions = LibretroCoreOptions.CoreOptionsList.Cores.Find(x => x.CoreName.Equals(_wrapper.Core.Name, StringComparison.OrdinalIgnoreCase));
                    if (_wrapper.Core.CoreOptions is null)
                    {
                        _wrapper.Core.CoreOptions = new LibretroCoreOptions { CoreName = _wrapper.Core.Name };
                        LibretroCoreOptions.CoreOptionsList.Cores.Add(_wrapper.Core.CoreOptions);
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

                    LibretroCoreOptions.SaveCoreOptionsFile();
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Instance.LogException(e);
                    return false;
                }
            }

            bool SetSupportNoGame()
            {
                if (data != null)
                    _wrapper.Core.SupportNoGame = *(bool*)data;
                return true;
            }

            bool SetFrameTimeCallback()
            {
                Logger.Instance.LogInfo("Using FrameTime Callback");
                if (data != null)
                    _wrapper.FrameTimeInterface = *(retro_frame_time_callback*)data;
                return true;
            }

            bool SetAudioCallback()
            {
                if (data != null)
                    _wrapper.Audio.AudioCallback = Marshal.PtrToStructure<retro_audio_callback>((IntPtr)data);
                return true;
            }

            bool SetSystemAvInfo()
            {
                if (data != null)
                    _wrapper.Game.SystemAVInfo = *(retro_system_av_info*)data;
                return true;
            }

            bool SetSubsystemInfo()
            {
                if (data == null)
                    return false;

                retro_subsystem_info* subsystemInfoPtr = (retro_subsystem_info*)data;

                int subsystemIndex = 0;
                while (subsystemInfoPtr[subsystemIndex].ident != IntPtr.Zero)
                {
                    retro_subsystem_info subsystemInfo = subsystemInfoPtr[subsystemIndex];
                    //Logger.Instance.LogInfo("Subsystem:");
                    //Logger.Instance.LogInfo($"  Desc: {Marshal.PtrToStringAnsi(subsystemInfo.desc)}");
                    //Logger.Instance.LogInfo($"  Ident: {Marshal.PtrToStringAnsi(subsystemInfo.ident)}");
                    //Logger.Instance.LogInfo($"  NumRoms: {subsystemInfo.num_roms}");
                    //Logger.Instance.LogInfo($"  ID: {subsystemInfo.id}");

                    for (int romIndex = 0; romIndex < subsystemInfo.num_roms; ++romIndex)
                    {
                        retro_subsystem_rom_info romInfo = subsystemInfo.roms[romIndex];
                        //Logger.Instance.LogInfo($"    Rom:");
                        //Logger.Instance.LogInfo($"      Desc: {Marshal.PtrToStringAnsi(romInfo.desc)}");
                        //Logger.Instance.LogInfo($"      ValidExtensions: {Marshal.PtrToStringAnsi(romInfo.valid_extensions)}");
                        //Logger.Instance.LogInfo($"      NeedFullPath: {romInfo.need_fullpath}");
                        //Logger.Instance.LogInfo($"      BlockExtract: {romInfo.block_extract}");
                        //Logger.Instance.LogInfo($"      Required: {romInfo.required}");
                        //Logger.Instance.LogInfo($"      NumMemory: {romInfo.num_memory}");

                        for (int memoryIndex = 0; memoryIndex < romInfo.num_memory; ++memoryIndex)
                        {
                            retro_subsystem_memory_info memoryInfo = romInfo.memory[memoryIndex];
                            //Logger.Instance.LogInfo($"      Memory:");
                            //Logger.Instance.LogInfo($"        Extension: {Marshal.PtrToStringAnsi(memoryInfo.extension)}");
                            //Logger.Instance.LogInfo($"        Type: {memoryInfo.type}");
                        }
                    }
                    ++subsystemIndex;
                }

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
                    //Logger.Instance.LogInfo($"# Controller port: {numPorts + 1}", $"{cmd}");
                    //for (int j = 0; j < inControllerInfo[numPorts].num_types; ++j)
                    //{
                    //    string desc = UnsafeStringUtils.CharsToString(inControllerInfo[numPorts].types[j].desc);
                    //    uint id     = inControllerInfo[numPorts].types[j].id;
                        //Logger.Instance.LogInfo($"    {desc} (ID: {id})", $"{cmd}");
                    //}
                }

                _wrapper.Core.ControllerPorts = new retro_controller_info[numPorts];
                for (int j = 0; j < numPorts; ++j)
                    _wrapper.Core.ControllerPorts[j] = inControllerInfo[j];

                return true;
            }

            bool SetMemoryMaps()
            {
                if (data != null)
                    _wrapper.Memory = new LibretroMemory(*(retro_memory_map*)data);
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

            bool SetSupportAchievements()
            {
                if (data != null)
                    _supportsAchievements = *(bool*)data;
                return false;
            }

            bool SetSerializationQuirks()
            {
                if (data != null)
                    _wrapper.Serialization.SetQuirks(*(ulong*)data);
                return true;
            }

            bool SetDiskControlExtInterface()
            {
                if (data == null)
                    return false;

                retro_disk_control_ext_callback inCallback = Marshal.PtrToStructure<retro_disk_control_ext_callback>((IntPtr)data);
                _wrapper.Disk = new LibretroDiskInterface(inCallback);
                return true;
            }

            bool Shutdown() => true;

            bool SetMessageExt()
            {
                if (data != null)
                {
                    retro_message_ext* messageExt = (retro_message_ext*)data;
                    Logger.Instance.LogInfo($"{UnsafeStringUtils.CharsToString(messageExt->msg)}", $"{cmd}");
                    Logger.Instance.LogInfo($"{messageExt->duration}", $"{cmd}");
                    Logger.Instance.LogInfo($"{messageExt->priority}", $"{cmd}");
                    Logger.Instance.LogInfo($"{messageExt->level}", $"{cmd}");
                    Logger.Instance.LogInfo($"{messageExt->target}", $"{cmd}");
                    Logger.Instance.LogInfo($"{messageExt->type}", $"{cmd}");
                    Logger.Instance.LogInfo($"{messageExt->progress}", $"{cmd}");
                }
                return true;
            }
            #endregion
        }
    }
}
