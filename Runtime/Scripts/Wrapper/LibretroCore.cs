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
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroCore
    {
        public retro_init_t retro_init;
        public retro_deinit_t retro_deinit;
        public retro_api_version_t retro_api_version;
        public retro_get_system_info_t retro_get_system_info;
        public retro_get_system_av_info_t retro_get_system_av_info;
        public retro_set_controller_port_device_t retro_set_controller_port_device;
        public retro_reset_t retro_reset;
        public retro_run_t retro_run;
        public retro_serialize_size_t retro_serialize_size;
        public retro_serialize_t retro_serialize;
        public retro_unserialize_t retro_unserialize;
        public retro_cheat_reset_t retro_cheat_reset;
        public retro_cheat_set_t retro_cheat_set;
        public retro_load_game_t retro_load_game;
        public retro_load_game_special_t retro_load_game_special;
        public retro_unload_game_t retro_unload_game;
        public retro_get_region_t retro_get_region;
        public retro_get_memory_data_t retro_get_memory_data;
        public retro_get_memory_size_t retro_get_memory_size;

        public bool Initialized { get; private set; } = false;

        public uint ApiVersion { get; private set; }

        public string Name { get; private set; }
        public string LibraryName { get; private set; }
        public string LibraryVersion { get; private set; }
        public string[] ValidExtensions { get; private set; }
        public bool NeedFullpath { get; private set; }
        public bool BlockExtract { get; private set; }

        public LibretroCoreOptions CoreOptions;

        public int PerformanceLevel;
        public bool SupportNoGame;
        public bool HwAccelerated;
        public int Rotation;

        public retro_controller_info[] ControllerPorts;

        private readonly LibretroWrapper _wrapper;

        private DynamicLibrary _dll;

        public LibretroCore(LibretroWrapper wrapper) => _wrapper = wrapper;

        public bool Start(string coreName)
        {
            switch (_wrapper.TargetPlatform)
            {
                case LibretroTargetPlatform.WindowsEditor:
                case LibretroTargetPlatform.WindowsPlayer:
                    _dll = new DynamicLibraryWindows();
                    break;
                case LibretroTargetPlatform.OSXEditor:
                case LibretroTargetPlatform.OSXPlayer:
                    _dll = new DynamicLibraryOSX();
                    break;
                case LibretroTargetPlatform.LinuxEditor:
                case LibretroTargetPlatform.LinuxPlayer:
                    _dll = new DynamicLibraryLinux();
                    break;
                default:
                    Logger.Instance.LogError($"Target platform '{_wrapper.TargetPlatform}' not supported.");
                    return false;
            }

            Name = coreName;

            if (!LoadLibrary())
                return false;

            if (!GetCoreFunctions())
                return false;

            ApiVersion = retro_api_version();
            GetSystemInfo();

            if (!SetCallbacks())
                return false;

            retro_init();

            Initialized = true;
            return true;
        }

        public void Stop()
        {
            try
            {
                //FIXME(Tom): This sometimes crash (mostly on cores using libco)
                if (Initialized && !HwAccelerated)
                    retro_deinit();

                _dll?.Free(true);
                _dll = null;

                Initialized = false;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
            }
        }

        private bool LoadLibrary()
        {
            try
            {
                string corePath = FileSystem.GetAbsolutePath($"{LibretroWrapper.CoresDirectory}/{Name}_libretro.{_dll.Extension}");
                if (!FileSystem.FileExists(corePath))
                {
                    Logger.Instance.LogError($"Core '{Name}' at path '{corePath}' not found.");
                    return false;
                }

                string tempDirectory = FileSystem.GetAbsolutePath(LibretroWrapper.TempDirectory);
                if (!Directory.Exists(tempDirectory))
                    _ = Directory.CreateDirectory(tempDirectory);

                string instancePath = Path.Combine(tempDirectory, $"{Name}_{Guid.NewGuid()}.{_dll.Extension}");
                File.Copy(corePath, instancePath);

                _dll.Load(instancePath);
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                Stop();
                return false;
            }
        }

        private bool GetCoreFunctions()
        {
            try
            {
                retro_init                       = _dll.GetFunction<retro_init_t>("retro_init");
                retro_deinit                     = _dll.GetFunction<retro_deinit_t>("retro_deinit");
                retro_api_version                = _dll.GetFunction<retro_api_version_t>("retro_api_version");
                retro_get_system_info            = _dll.GetFunction<retro_get_system_info_t>("retro_get_system_info");
                retro_get_system_av_info         = _dll.GetFunction<retro_get_system_av_info_t>("retro_get_system_av_info");
                retro_set_controller_port_device = _dll.GetFunction<retro_set_controller_port_device_t>("retro_set_controller_port_device");
                retro_reset                      = _dll.GetFunction<retro_reset_t>("retro_reset");
                retro_run                        = _dll.GetFunction<retro_run_t>("retro_run");
                retro_serialize_size             = _dll.GetFunction<retro_serialize_size_t>("retro_serialize_size");
                retro_serialize                  = _dll.GetFunction<retro_serialize_t>("retro_serialize");
                retro_unserialize                = _dll.GetFunction<retro_unserialize_t>("retro_unserialize");
                retro_cheat_reset                = _dll.GetFunction<retro_cheat_reset_t>("retro_cheat_reset");
                retro_cheat_set                  = _dll.GetFunction<retro_cheat_set_t>("retro_cheat_set");
                retro_load_game                  = _dll.GetFunction<retro_load_game_t>("retro_load_game");
                retro_load_game_special          = _dll.GetFunction<retro_load_game_special_t>("retro_load_game_special");
                retro_unload_game                = _dll.GetFunction<retro_unload_game_t>("retro_unload_game");
                retro_get_region                 = _dll.GetFunction<retro_get_region_t>("retro_get_region");
                retro_get_memory_data            = _dll.GetFunction<retro_get_memory_data_t>("retro_get_memory_data");
                retro_get_memory_size            = _dll.GetFunction<retro_get_memory_size_t>("retro_get_memory_size");
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                Stop();
                return false;
            }
        }

        private unsafe void GetSystemInfo()
        {
            retro_get_system_info(out retro_system_info systemInfo);

            LibraryName    = UnsafeStringUtils.CharsToString(systemInfo.library_name);
            LibraryVersion = UnsafeStringUtils.CharsToString(systemInfo.library_version);
            if (systemInfo.valid_extensions != null)
                ValidExtensions = UnsafeStringUtils.CharsToString(systemInfo.valid_extensions).Split('|');
            NeedFullpath = systemInfo.need_fullpath;
            BlockExtract = systemInfo.block_extract;
        }

        private bool SetCallbacks()
        {
            try
            {
                _dll.GetFunction<retro_set_environment_t>("retro_set_environment")(_wrapper.EnvironmentCallback);
                _dll.GetFunction<retro_set_video_refresh_t>("retro_set_video_refresh")(_wrapper.VideoRefreshCallback);
                _dll.GetFunction<retro_set_audio_sample_t>("retro_set_audio_sample")(_wrapper.AudioSampleCallback);
                _dll.GetFunction<retro_set_audio_sample_batch_t>("retro_set_audio_sample_batch")(_wrapper.AudioSampleBatchCallback);
                _dll.GetFunction<retro_set_input_poll_t>("retro_set_input_poll")(_wrapper.InputPollCallback);
                _dll.GetFunction<retro_set_input_state_t>("retro_set_input_state")(_wrapper.InputStateCallback);
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                Stop();
                return false;
            }
        }
    }
}
