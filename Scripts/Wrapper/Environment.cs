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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static SK.Libretro.Header;

namespace SK.Libretro
{
    internal sealed unsafe class Environment
    {
        private delegate bool EnvironmentCallDelegate(void* data);

        private readonly Wrapper _wrapper;
        private readonly Dictionary<retro_environment, EnvironmentCallDelegate> _callbacks;

        // TODO(Tom): Should not be here...
        private bool _supportsAchievements;

        // TEMP_HACK
        [Serializable]
        private sealed class CoresUsingOptionsIntlList
        {
            public List<string> Cores = null;
        }

        public Environment(Wrapper wrapper)
        {
            _wrapper   = wrapper;
            _callbacks = new Dictionary<retro_environment, EnvironmentCallDelegate>
            {
                /************************************************************************************************
                 * Data passed from the frontend to the core
                 */
                { retro_environment.RETRO_ENVIRONMENT_GET_OVERSCAN,                                GetOverscan                    },
                { retro_environment.RETRO_ENVIRONMENT_GET_CAN_DUPE,                                GetCanDupe                     },
                { retro_environment.RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY,                        GetSystemDirectory             },
                { retro_environment.RETRO_ENVIRONMENT_GET_VARIABLE,                                GetVariable                    },
                { retro_environment.RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE,                         GetVariableUpdate              },
                { retro_environment.RETRO_ENVIRONMENT_GET_LIBRETRO_PATH,                           GetLibretroPath                },
                { retro_environment.RETRO_ENVIRONMENT_GET_RUMBLE_INTERFACE,                        GetRumbleInterface             },
                { retro_environment.RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES,               GetInputDeviceCapabilities     },
                { retro_environment.RETRO_ENVIRONMENT_GET_SENSOR_INTERFACE,                        (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_GET_CAMERA_INTERFACE,                        (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_GET_LOG_INTERFACE,                           GetLogInterface                },
                { retro_environment.RETRO_ENVIRONMENT_GET_PERF_INTERFACE,                          GetPerfInterface               },
                { retro_environment.RETRO_ENVIRONMENT_GET_LOCATION_INTERFACE,                      (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_GET_CORE_ASSETS_DIRECTORY,                   GetCoreAssetsDirectory         },
                { retro_environment.RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY,                          GetSaveDirectory               },
                { retro_environment.RETRO_ENVIRONMENT_GET_USERNAME,                                GetUsername                    },
                { retro_environment.RETRO_ENVIRONMENT_GET_LANGUAGE,                                GetLanguage                    },
                { retro_environment.RETRO_ENVIRONMENT_GET_CURRENT_SOFTWARE_FRAMEBUFFER,            (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND, false) },
                { retro_environment.RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE,                     (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_GET_VFS_INTERFACE,                           GetVfsInterface                },
                { retro_environment.RETRO_ENVIRONMENT_GET_LED_INTERFACE,                           GetLedInterface                },
                { retro_environment.RETRO_ENVIRONMENT_GET_AUDIO_VIDEO_ENABLE,                      GetAudioVideoEnable            },
                { retro_environment.RETRO_ENVIRONMENT_GET_MIDI_INTERFACE,                          (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_GET_FASTFORWARDING,                          GetFastForwarding              },
                { retro_environment.RETRO_ENVIRONMENT_GET_TARGET_REFRESH_RATE,                     (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_GET_INPUT_BITMASKS,                          GetInputBitmasks               },
                { retro_environment.RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION,                    GetCoreOptionsVersion          },
                { retro_environment.RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER,                     GetPreferredHwRender           },
                { retro_environment.RETRO_ENVIRONMENT_GET_DISK_CONTROL_INTERFACE_VERSION,          GetDiskControlInterfaceVersion },
                { retro_environment.RETRO_ENVIRONMENT_GET_MESSAGE_INTERFACE_VERSION,               GetMessageInterfaceVersion     },
                { retro_environment.RETRO_ENVIRONMENT_GET_INPUT_MAX_USERS,                         GetInputMaxUsers               },
                { retro_environment.RETRO_ENVIRONMENT_GET_GAME_INFO_EXT,                           GetGameInfoExt                 },

                /************************************************************************************************
                 * Data passed from the core to the frontend
                 */
                { retro_environment.RETRO_ENVIRONMENT_SET_ROTATION,                                SetRotation                    },
                { retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE,                                 SetMessage                     },
                { retro_environment.RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL,                       SetPerformanceLevel            },
                { retro_environment.RETRO_ENVIRONMENT_SET_PIXEL_FORMAT,                            SetPixelFormat                 },
                { retro_environment.RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS,                       SetInputDescriptors            },
                { retro_environment.RETRO_ENVIRONMENT_SET_KEYBOARD_CALLBACK,                       SetKeyboardCallback            },
                { retro_environment.RETRO_ENVIRONMENT_SET_DISK_CONTROL_INTERFACE,                  SetDiskControlInterface        },
                { retro_environment.RETRO_ENVIRONMENT_SET_HW_RENDER,                               SetHwRender                    },
                { retro_environment.RETRO_ENVIRONMENT_SET_VARIABLES,                               SetVariables                   },
                { retro_environment.RETRO_ENVIRONMENT_SET_SUPPORT_NO_GAME,                         SetSupportNoGame               },
                { retro_environment.RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK,                     SetFrameTimeCallback           },
                { retro_environment.RETRO_ENVIRONMENT_SET_AUDIO_CALLBACK,                          SetAudioCallback               },
                { retro_environment.RETRO_ENVIRONMENT_SET_SYSTEM_AV_INFO,                          SetSystemAvInfo                },
                { retro_environment.RETRO_ENVIRONMENT_SET_PROC_ADDRESS_CALLBACK,                   (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_SET_SUBSYSTEM_INFO,                          SetSubsystemInfo               },
                { retro_environment.RETRO_ENVIRONMENT_SET_CONTROLLER_INFO,                         SetControllerInfo              },
                { retro_environment.RETRO_ENVIRONMENT_SET_MEMORY_MAPS,                             SetMemoryMaps                  },
                { retro_environment.RETRO_ENVIRONMENT_SET_GEOMETRY,                                SetGeometry                    },
                { retro_environment.RETRO_ENVIRONMENT_SET_SUPPORT_ACHIEVEMENTS,                    SetSupportAchievements         },
                { retro_environment.RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE, (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_SET_SERIALIZATION_QUIRKS,                    SetSerializationQuirks         },
                { retro_environment.RETRO_ENVIRONMENT_SET_HW_SHARED_CONTEXT,                       (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS,                            SetCoreOptions                 },
                { retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS_INTL,                       SetCoreOptionsIntl             },
                { retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS_DISPLAY,                    SetCoreOptionsDisplay          },
                { retro_environment.RETRO_ENVIRONMENT_SET_DISK_CONTROL_EXT_INTERFACE,              SetDiskControlExtInterface     },
                { retro_environment.RETRO_ENVIRONMENT_SHUTDOWN,                                    Shutdown                       },
                { retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT,                             SetMessageExt                  },
                { retro_environment.RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK,            (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK) },
                { retro_environment.RETRO_ENVIRONMENT_SET_MINIMUM_AUDIO_LATENCY,                   (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_MINIMUM_AUDIO_LATENCY) },
                { retro_environment.RETRO_ENVIRONMENT_SET_FASTFORWARDING_OVERRIDE,                 (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_FASTFORWARDING_OVERRIDE) },
                { retro_environment.RETRO_ENVIRONMENT_SET_CONTENT_INFO_OVERRIDE,                   SetContentInfoOverride },

                /************************************************************************************************
                 * RetroArch Extensions
                 */
                { retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND,                (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_SAVE_STATE_IN_BACKGROUND) },
                { retro_environment.RETRO_ENVIRONMENT_GET_CLEAR_ALL_THREAD_WAITS_CB,               (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_GET_CLEAR_ALL_THREAD_WAITS_CB) },
                { retro_environment.RETRO_ENVIRONMENT_POLL_TYPE_OVERRIDE,                          (data) => ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_POLL_TYPE_OVERRIDE) }
            };
        }

        public bool Callback(retro_environment cmd, void* data)
        {
            if (!_callbacks.TryGetValue(cmd, out EnvironmentCallDelegate callback))
            {
                Logger.Instance.LogError($"Environment unknown: {cmd}", "LibretroEnvironment.Callback");
                return false;
            }
            return callback(data);
        }

        /************************************************************************************************
        * Data passed from the frontend to the core
        */
        #region FrontendToCore
        private bool GetOverscan(void* data)
        {
            if (data != null)
                *(bool*)data = _wrapper.OptionCropOverscan;
            return true;
        }

        private bool GetCanDupe(void* data)
        {
            if (data != null)
                *(bool*)data = true;
            return true;
        }

        private bool GetSystemDirectory(void* data)
        {
            string path = $"{Wrapper.SystemDirectory}/{_wrapper.Core.Name}";
            if (!Directory.Exists(path))
                _ = Directory.CreateDirectory(path);
            if (data != null)
                *(char**)data = _wrapper.GetUnsafeString(path);
            return true;
        }

        private bool GetVariable(void* data)
        {
            if (data == null)
            {
                Logger.Instance.LogWarning($"data is null.", nameof(retro_environment.RETRO_ENVIRONMENT_GET_VARIABLE));
                return false;
            }

            retro_variable* outVariable = (retro_variable*)data;
            if (outVariable->key == IntPtr.Zero)
                return false;

            string key = Marshal.PtrToStringAnsi(outVariable->key);

            if (_wrapper.Core.GameOptions == null)
            {
                Logger.Instance.LogWarning($"Core didn't set its options. Requested key: {key}", nameof(retro_environment.RETRO_ENVIRONMENT_GET_VARIABLE));
                return false;
            }

            CoreOption coreOption = _wrapper.Core.GameOptions[key];
            if (coreOption == null)
            {
                Logger.Instance.LogWarning($"Core option '{key}' not found.", nameof(retro_environment.RETRO_ENVIRONMENT_GET_VARIABLE));
                return false;
            }

            outVariable->value = (IntPtr)_wrapper.GetUnsafeString(coreOption.CurrentValue);
            return true;
        }

        private bool GetVariableUpdate(void* data)
        {
            if (data != null)
                *(bool*)data = _wrapper.UpdateVariables;
            _wrapper.UpdateVariables = false;
            return true;
        }

        private bool GetLibretroPath(void* data)
        {
            if (data != null)
            {
                string path = $"{Wrapper.CoresDirectory}/{_wrapper.Core.Name}";
                if (!Directory.Exists(path))
                    _ = Directory.CreateDirectory(path);
                *(char**)data = _wrapper.GetUnsafeString(path);
            }
            return true;
        }

        private bool GetRumbleInterface(void* data)
        {
            Marshal.StructureToPtr(_wrapper.Input.RumbleInterface, (IntPtr)data, true);
            return true;
        }

        private bool GetInputDeviceCapabilities(void* data)
        {
            if (data != null)
                *(ulong*)data = (1 << (int)RETRO_DEVICE_JOYPAD)
                              | (1 << (int)RETRO_DEVICE_MOUSE)
                              | (1 << (int)RETRO_DEVICE_KEYBOARD)
                              | (1 << (int)RETRO_DEVICE_LIGHTGUN)
                              | (1 << (int)RETRO_DEVICE_ANALOG)
                              | (1 << (int)RETRO_DEVICE_POINTER);
            return true;
        }

        private bool GetLogInterface(void* data)
        {
            if (data != null)
                ((retro_log_callback*)data)->log = Marshal.GetFunctionPointerForDelegate<retro_log_printf_t>(LogInterface.RetroLogPrintf);
            return true;
        }

        private bool GetPerfInterface(void* data)
        {
            _wrapper.Perf = new PerfInterface((IntPtr)data);
            return true;
        }

        private bool GetCoreAssetsDirectory(void* data)
        {
            string path = $"{Wrapper.CoreAssetsDirectory}/{_wrapper.Core.Name}";
            if (!Directory.Exists(path))
                _ = Directory.CreateDirectory(path);
            if (data != null)
                *(char**)data = _wrapper.GetUnsafeString(path);
            return true;
        }

        private bool GetSaveDirectory(void* data)
        {
            string path = $"{Wrapper.SavesDirectory}/{_wrapper.Core.Name}";
            if (!Directory.Exists(path))
                _ = Directory.CreateDirectory(path);
            if (data != null)
                *(char**)data = _wrapper.GetUnsafeString(path);
            return true;
        }

        private bool GetUsername(void* data)
        {
            if (data != null)
                *(char**)data = _wrapper.GetUnsafeString(_wrapper.OptionUserName);
            return true;
        }

        private bool GetLanguage(void* data)
        {
            if (data != null)
                *(char**)data = _wrapper.GetUnsafeString(_wrapper.OptionLanguage.ToString());
            return true;
        }

        private bool GetVfsInterface(void* data)
        {
            if (data != null)
            {
                retro_vfs_interface_info* interfaceInfo = (retro_vfs_interface_info*)data;
                interfaceInfo->iface = IntPtr.Zero;
                Logger.Instance.LogWarning($"VFS not implemented (Core asked for VFS API v{interfaceInfo->required_interface_version}).");
            }
            return false;
        }

        private bool GetLedInterface(void* data)
        {
            _wrapper.Led = new LedInterface((IntPtr)data);
            return true;
        }

        private bool GetAudioVideoEnable(void* data)
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

        private bool GetFastForwarding(void* data)
        {
            if (data != null)
                *(bool*)data = false;
            return false;
        }

        private bool GetInputBitmasks(void* data)
        {
            if (data != null)
                *(bool*)data = true;
            return true;
        }

        // TODO: Set to >= 1 (RETRO_API_VERSION) once RETRO_ENVIRONMENT_SET_CORE_OPTIONS_INTL works
        private bool GetCoreOptionsVersion(void* data)
        {
            if (data != null)
            {
                // TEMP_HACK
                string filePath = $"{Wrapper.MainDirectory}/cores_using_options_intl.json";
                CoresUsingOptionsIntlList coresUsingOptionsIntl = FileSystem.DeserializeFromJson<CoresUsingOptionsIntlList>(filePath);
                *(uint*)data = coresUsingOptionsIntl == null || coresUsingOptionsIntl.Cores == null || !coresUsingOptionsIntl.Cores.Contains(_wrapper.Core.Name)
                             ? RETRO_API_VERSION
                             : 0;
            }
            return true;
        }

        private bool GetPreferredHwRender(void* data)
        {
            if (data != null)
            {
                //*(uint*)data = (uint)retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL;
                *(uint*)data = (uint)retro_hw_context_type.RETRO_HW_CONTEXT_NONE;
            }
            return true;
        }

        private bool GetMessageInterfaceVersion(void* data)
        {
            if (data != null)
                *(uint*)data = MessageInterface.VERSION;
            return true;
        }

        private bool GetInputMaxUsers(void* data)
        {
            if (data != null)
                *(uint*)data = Input.MAX_USERS_SUPPORTED;
            return true;
        }

        private bool GetGameInfoExt(void* data)
        {
            if (data == null)
                return false;

            retro_game_info_ext** infoExt = (retro_game_info_ext**)data;
            fixed (retro_game_info_ext* ptr = &_wrapper.Game.GameInfoExt)
                *infoExt = ptr;

            return true;
        }

        private bool GetDiskControlInterfaceVersion(void* data)
        {
            if (data != null)
                *(uint*)data = DiskInterface.VERSION;
            return true;
        }
        #endregion

        /************************************************************************************************
        / Data passed from the core to the frontend
        /***********************************************************************************************/
        #region CoreToFrontend
        private bool SetRotation(void* data)
        {
            // Values: 0,  1,   2,   3
            // Result: 0, 90, 180, 270 degrees
            if (data != null)
                _wrapper.Core.Rotation = (int)*(uint*)data * 90;
            return _wrapper.Graphics.UseCoreRotation;
        }

        private bool SetMessage(void* data)
        {
            if (data != null)
                Logger.Instance.LogInfo($"<- Message: {Marshal.PtrToStringAnsi(((retro_message*)data)->msg)}", nameof(retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE));
            return true;
        }

        private bool SetPerformanceLevel(void* data)
        {
            if (data != null)
                _wrapper.Core.PerformanceLevel = *(int*)data;
            return true;
        }

        private bool SetPixelFormat(void* data)
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
                    _wrapper.Graphics.PixelFormat = *inPixelFormat;
                    return true;
                }
            }

            return false;
        }

        private bool SetInputDescriptors(void* data)
        {
            if (data == null)
                return true;

            retro_input_descriptor* inInputDescriptors = (retro_input_descriptor*)data;
            uint id;
            for (IntPtr inDesc = inInputDescriptors->desc; inInputDescriptors->desc != IntPtr.Zero; ++inInputDescriptors)
            {
                uint port = inInputDescriptors->port;
                if (port >= Input.MAX_USERS)
                    continue;

                uint device = inInputDescriptors->device;
                if (device != RETRO_DEVICE_JOYPAD && device != RETRO_DEVICE_ANALOG)
                    continue;

                id = inInputDescriptors->id;
                if (id >= Input.FIRST_CUSTOM_BIND)
                    continue;

                string descText = Marshal.PtrToStringAnsi(inDesc);
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
                                    _wrapper.Input.ButtonDescriptions[port, (int)Input.CustomBinds.ANALOG_LEFT_X_PLUS] = descText;
                                    _wrapper.Input.ButtonDescriptions[port, (int)Input.CustomBinds.ANALOG_LEFT_X_MINUS] = descText;
                                }
                                break;
                                case RETRO_DEVICE_INDEX_ANALOG_RIGHT:
                                {
                                    _wrapper.Input.ButtonDescriptions[port, (int)Input.CustomBinds.ANALOG_RIGHT_X_PLUS] = descText;
                                    _wrapper.Input.ButtonDescriptions[port, (int)Input.CustomBinds.ANALOG_RIGHT_X_MINUS] = descText;
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
                                    _wrapper.Input.ButtonDescriptions[port, (int)Input.CustomBinds.ANALOG_LEFT_Y_PLUS] = descText;
                                    _wrapper.Input.ButtonDescriptions[port, (int)Input.CustomBinds.ANALOG_LEFT_Y_MINUS] = descText;
                                }
                                break;
                                case RETRO_DEVICE_INDEX_ANALOG_RIGHT:
                                {
                                    _wrapper.Input.ButtonDescriptions[port, (int)Input.CustomBinds.ANALOG_RIGHT_Y_PLUS] = descText;
                                    _wrapper.Input.ButtonDescriptions[port, (int)Input.CustomBinds.ANALOG_RIGHT_Y_MINUS] = descText;
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
                else
                    _wrapper.Input.ButtonDescriptions[port, id] = descText;
            }

            _wrapper.Input.HasInputDescriptors = true;

            return true;
        }

        private bool SetKeyboardCallback(void* data)
        {
            if (data == null)
                return false;

            _wrapper.Input.KeyboardCallback = Marshal.PtrToStructure<retro_keyboard_callback>((IntPtr)data);
            return true;
        }

        private bool SetDiskControlInterface(void* data)
        {
            if (data == null)
                return false;

            retro_disk_control_callback inCallback = Marshal.PtrToStructure<retro_disk_control_callback>((IntPtr)data);
            _wrapper.Disk = new DiskInterface(_wrapper, inCallback);
            return true;
        }

        private bool SetHwRender(void* data)
        {
            if (data == null || _wrapper.Core.HwAccelerated)
                return false;

            retro_hw_render_callback* inCallback = (retro_hw_render_callback*)data;

            if (inCallback->context_type != retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL && inCallback->context_type != retro_hw_context_type.RETRO_HW_CONTEXT_OPENGL_CORE)
                return false;

            _wrapper.OpenGL = new OpenGL();
            if (!_wrapper.OpenGL.Init())
                return false;

            inCallback->get_current_framebuffer = Marshal.GetFunctionPointerForDelegate(_wrapper.OpenGL.GetCurrentFrameBufferCallback);
            inCallback->get_proc_address        = Marshal.GetFunctionPointerForDelegate(_wrapper.OpenGL.GetProcAddressCallback);

            _wrapper.HwRenderInterface = Marshal.PtrToStructure<retro_hw_render_callback>((IntPtr)data);

            _wrapper.Core.HwAccelerated = true;

            return true;
        }

        private bool SetVariables(void* data)
        {
            if (data == null)
                return true;

            try
            {
                _wrapper.Core.DeserializeOptions();

                retro_variable* inVariable = (retro_variable*)data;
                for (IntPtr inKey = inVariable->key; inVariable->key != IntPtr.Zero && inVariable->value != IntPtr.Zero; ++inVariable)
                {
                    string key     = Marshal.PtrToStringAnsi(inVariable->key);
                    string inValue = Marshal.PtrToStringAnsi(inVariable->value);
                    string[] lineSplit = inValue.Split(';');
                    if (_wrapper.Core.CoreOptions[key] == null)
                        _wrapper.Core.CoreOptions[key] = lineSplit.Length > 3 ? new CoreOption(lineSplit) : new CoreOption(key, lineSplit);
                    else
                    {
                        if (lineSplit.Length > 3)
                            _wrapper.Core.CoreOptions[key].Update(lineSplit);
                        else
                            _wrapper.Core.CoreOptions[key].Update(key, lineSplit);
                    }
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

        private bool SetSupportNoGame(void* data)
        {
            if (data != null)
                _wrapper.Core.SupportNoGame = *(bool*)data;
            return true;
        }

        private bool SetFrameTimeCallback(void* data)
        {
            Logger.Instance.LogInfo("Using FrameTime Callback");
            if (data != null)
                _wrapper.FrameTimeInterface = *(retro_frame_time_callback*)data;
            return true;
        }

        private bool SetAudioCallback(void* data)
        {
            if (data != null)
                _wrapper.Audio.AudioCallback = Marshal.PtrToStructure<retro_audio_callback>((IntPtr)data);
            return true;
        }

        private bool SetSystemAvInfo(void* data)
        {
            if (data != null)
                _wrapper.Game.SystemAVInfo = *(retro_system_av_info*)data;
            return true;
        }

        private bool SetSubsystemInfo(void* data)
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

        private bool SetControllerInfo(void* data)
        {
            if (data == null)
                return true;

            retro_controller_info* inControllerInfo = (retro_controller_info*)data;

            int portIndex = 0;
            for (; inControllerInfo[portIndex].types != null; ++portIndex)
            {
                retro_controller_info controllerInfo = inControllerInfo[portIndex];
                uint numDevices = controllerInfo.num_types;
                for (int deviceIndex = 0; deviceIndex < numDevices; ++deviceIndex)
                {
                    retro_controller_description controllerDescription = controllerInfo.types[deviceIndex];
                    string description = Marshal.PtrToStringAnsi(controllerDescription.desc);
                    Controller device = new Controller
                    {
                        Description = !string.IsNullOrEmpty(description) ? description : "none",
                        Id          = controllerDescription.id
                    };
                    _wrapper.Input.DeviceMap.Add(portIndex, device);
                }
            }

            return true;
        }

        private bool SetMemoryMaps(void* data)
        {
            if (data != null)
                _wrapper.Memory = new MemoryMap(*(retro_memory_map*)data);
            return true;
        }

        private bool SetGeometry(void* data)
        {
            if (data != null)
            {
                retro_game_geometry* inGeometry = (retro_game_geometry*)data;

                if (_wrapper.Game.SystemAVInfo.geometry.base_width != inGeometry->base_width
                    || _wrapper.Game.SystemAVInfo.geometry.base_height != inGeometry->base_height
                    || _wrapper.Game.SystemAVInfo.geometry.aspect_ratio != inGeometry->aspect_ratio)
                {
                    _wrapper.Game.SystemAVInfo.geometry = *inGeometry;
                    // TODO: Set video aspect ratio if needed
                }
            }
            return true;
        }

        private bool SetSupportAchievements(void* data)
        {
            if (data != null)
                _supportsAchievements = *(bool*)data;
            return false;
        }

        private bool SetSerializationQuirks(void* data)
        {
            if (data != null)
            {
                ulong* quirks = (ulong*)data;
                //*quirks |= RETRO_SERIALIZATION_QUIRK_FRONT_VARIABLE_SIZE;
                _wrapper.Serialization.SetQuirks(*quirks);
            }
            return true;
        }

        private bool SetCoreOptions(void* data)
        {
            if (data == null)
                return false;

            try
            {
                _wrapper.Core.DeserializeOptions();

                retro_core_option_definition* defs = (retro_core_option_definition*)data;
                for (; defs->key != IntPtr.Zero; ++defs)
                {
                    string key = Marshal.PtrToStringAnsi(defs->key);
                    string description = defs->desc != IntPtr.Zero ? Marshal.PtrToStringAnsi(defs->desc) : "";
                    string info = defs->info != IntPtr.Zero ? Marshal.PtrToStringAnsi(defs->info) : "";
                    string defaultValue = defs->default_value != IntPtr.Zero ? Marshal.PtrToStringAnsi(defs->default_value) : "";

                    List<string> possibleValues = new List<string>();
                    for (int i = 0; i < defs->values.Length; ++i)
                    {
                        if (defs->values[i].value == IntPtr.Zero)
                            break;
                        possibleValues.Add(Marshal.PtrToStringAnsi(defs->values[i].value));
                    }

                    string value = "";
                    if (!string.IsNullOrEmpty(defaultValue))
                        value = defaultValue;
                    else if (possibleValues.Count > 0)
                        value = possibleValues[0];

                    if (_wrapper.Core.CoreOptions[key] == null)
                        _wrapper.Core.CoreOptions[key] = new CoreOption(key, description, info, value, possibleValues.ToArray());
                    else
                        _wrapper.Core.CoreOptions[key].Update(key, description, info, possibleValues.ToArray());
                }

                _wrapper.Core.SerializeOptions();
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogError($"Failed to retrieve and write the core options, they must be entered manually in the configuration file for now... (Exception message: {e.Message})");
                return false;
            }
        }

        /*
         * FIXME: Figure out why data isn't providing proper addresses...
         * Trying to read intl->us gives invalid/unreadable memory
         * Definitions for retro_core_option_definition or retro_core_option_value don't really matter here I think.
         * Since intl->local and intl->us are of IntPtr type, shouldn't this give a valid memory address either way?
         */
        private bool SetCoreOptionsIntl(void* data)
        {
            if (data == null)
                return false;

            retro_core_options_intl* intl = (retro_core_options_intl*)data;
            retro_core_option_definition* us = (retro_core_option_definition*)intl->us;

            return ENVIRONMENT_NOT_IMPLEMENTED(retro_environment.RETRO_ENVIRONMENT_SET_CORE_OPTIONS_INTL);
        }

        private bool SetCoreOptionsDisplay(void* data)
        {
            if (data == null)
                return false;

            retro_core_option_display* inDisplay = (retro_core_option_display*)data;
            if (inDisplay->key == IntPtr.Zero)
                return false;

            string key = Marshal.PtrToStringAnsi(inDisplay->key);
            _wrapper.Core.CoreOptions[key]?.SetVisibility(inDisplay->visible);
            return true;
        }

        private bool SetDiskControlExtInterface(void* data)
        {
            if (data == null)
                return false;

            retro_disk_control_ext_callback inCallback = Marshal.PtrToStructure<retro_disk_control_ext_callback>((IntPtr)data);
            _wrapper.Disk = new DiskInterface(_wrapper, inCallback);
            return true;
        }

        private bool Shutdown(void* data) => false;

        private bool SetMessageExt(void* data)
        {
            if (data == null)
                return false;

            retro_message_ext* messageExt = (retro_message_ext*)data;
            Logger.Instance.LogInfo($"{Marshal.PtrToStringAnsi(messageExt->msg)}", nameof(retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt->duration}", nameof(retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt->priority}", nameof(retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt->level}", nameof(retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt->target}", nameof(retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt->type}", nameof(retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT));
            Logger.Instance.LogInfo($"{messageExt->progress}", nameof(retro_environment.RETRO_ENVIRONMENT_SET_MESSAGE_EXT));
            return true;
        }

        private bool SetContentInfoOverride(void* data)
        {
            if (data == null)
                return true;

            retro_system_content_info_override* infoOverrides = (retro_system_content_info_override*)data;

            for (retro_system_content_info_override* infoOverride = infoOverrides; infoOverrides->extensions != IntPtr.Zero; ++infoOverrides)
            {
                string extensionsString = Marshal.PtrToStringAnsi(infoOverride->extensions);
                if (string.IsNullOrEmpty(extensionsString))
                    continue;

                string[] extensions = extensionsString.Split('|');
                foreach (string extension in extensions)
                    _wrapper.Game.ContentOverrides.Add(extension, infoOverride->need_fullpath, infoOverride->persistent_data);
            }

            return true;
        }
        #endregion

        private static bool ENVIRONMENT_NOT_IMPLEMENTED(retro_environment cmd, bool log = true, bool defaultReturns = false)
        {
            if (!log)
                return defaultReturns;

            if (defaultReturns)
                Logger.Instance.LogWarning("Environment not implemented!", cmd.ToString());
            else
                Logger.Instance.LogError("Environment not implemented!", cmd.ToString());
            return defaultReturns;
        }
    }
}
