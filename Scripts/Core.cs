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
using System.IO;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class Core : IDisposable
    {
        public bool Initialized { get; private set; }
        public uint ApiVersion { get; private set; }
        public string Name { get; private set; }
        public string Path => System.IO.Path.GetDirectoryName(_dll.Path);
        public SystemInfo SystemInfo { get; private set; }
        public int PerformanceLevel { get; private set; }
        public bool SupportNoGame { get; private set; }
        public bool SupportsAchievements { get; private set; }
        public bool HwAccelerated { get; set; }

        public readonly List<retro_subsystem_info> SubsystemInfo = new();

        private readonly Wrapper _wrapper;

        private DynamicLibrary _dll;

        private retro_init_t _retro_init;
        private retro_deinit_t _retro_deinit;
        private retro_api_version_t _retro_api_version;
        private retro_get_system_info_t _retro_get_system_info;
        private retro_get_system_av_info_t _retro_get_system_av_info;
        private retro_set_controller_port_device_t _retro_set_controller_port_device;
        private retro_reset_t _retro_reset;
        private retro_run_t _retro_run;
        private retro_serialize_size_t _retro_serialize_size;
        private retro_serialize_t _retro_serialize;
        private retro_unserialize_t _retro_unserialize;
        private retro_cheat_reset_t _retro_cheat_reset;
        private retro_cheat_set_t _retro_cheat_set;
        private retro_load_game_t _retro_load_game;
        private retro_load_game_special_t _retro_load_game_special;
        private retro_unload_game_t _retro_unload_game;
        private retro_get_region_t _retro_get_region;
        private retro_get_memory_data_t _retro_get_memory_data;
        private retro_get_memory_size_t _retro_get_memory_size;

        private retro_set_environment_t _retro_set_environment;
        private retro_set_video_refresh_t _retro_set_video_refresh;
        private retro_set_audio_sample_t _retro_set_audio_sample;
        private retro_set_audio_sample_batch_t _retro_set_audio_sample_batch;
        private retro_set_input_poll_t _retro_set_input_poll;
        private retro_set_input_state_t _retro_set_input_state;

        public Core(Wrapper wrapper) => _wrapper = wrapper;

        public void Dispose()
        {
            if (Initialized)
            {
                Initialized = false;
                try
                {
                    //FIXME(Tom): This sometimes crashes
                    _retro_deinit();
                }
                catch
                {
                    throw;
                }
            }

            try
            {
                _dll?.Dispose();
            }
            catch
            {
                throw;
            }
            finally
            {
                _dll = null;
            }
        }

        public bool Start(string coreName)
        {
            switch (_wrapper.Settings.Platform)
            {
                case Platform.Win:
                    _dll = new DynamicLibraryWindows(true);
                    break;
                case Platform.OSX:
                    _dll = new DynamicLibraryOSX(true);
                    break;
                case Platform.Android:
                case Platform.Linux:
                    _dll = new DynamicLibraryLinux(true);
                    break;
                default:
                    _wrapper.LogHandler.LogError($"Runtime platform '{_wrapper.Settings.Platform}' not supported.", "SK.Libretro.Core.Start");
                    return false;
            }

            Name = coreName;

            if (!LoadLibrary())
                return false;

            if (!GetCoreFunctions())
                return false;

            ApiVersion = _retro_api_version();
            GetSystemInfo();

            SetCallbacks();

            _retro_init();

            Initialized = true;
            return true;
        }

        public bool LoadGame(ref retro_game_info game) => _retro_load_game(ref game);

        public bool LoadGameSpecial(uint game_type, ref retro_game_info info, nuint num_info) => _retro_load_game_special(game_type, ref info, num_info);

        public void GetSystemAVInfo(out retro_system_av_info info) => _retro_get_system_av_info(out info);

        public void SetControllerPortDevice(uint port, RETRO_DEVICE device) => _retro_set_controller_port_device(port, device);

        public void Reset() => _retro_reset();

        public void Run() => _retro_run();

        public nuint SerializeSize() => _retro_serialize_size();

        public bool Serialize(IntPtr data, nuint size) => _retro_serialize(data, size);

        public bool Unserialize(IntPtr data, nuint size) => _retro_unserialize(data, size);

        public void CheatReset() => _retro_cheat_reset();

        public void CheatSet(uint index, bool enabled, IntPtr code) => _retro_cheat_set(index, enabled, code);

        public void UnloadGame() => _retro_unload_game(); /* FIXME(Tom): This sometimes crashes */

        public uint GetRegion() => _retro_get_region();

        public IntPtr GetMemoryData(RETRO_MEMORY id) => _retro_get_memory_data(id);

        public nuint GetMemorySize(RETRO_MEMORY id) => _retro_get_memory_size(id);

        public bool SetPerformanceLevel(IntPtr data)
        {
            if (data.IsNull())
                return false;

            PerformanceLevel = data.ReadInt32();
            return true;
        }

        public bool SetSupportNoGame(IntPtr data)
        {
            if (data.IsNull())
                return false;

            SupportNoGame = data.IsTrue();
            return true;
        }

        public bool SetSubsystemInfo(IntPtr data)
        {
            if (data.IsNull())
                return false;

            SubsystemInfo.Clear();

            retro_subsystem_info subsystemInfo = data.ToStructure<retro_subsystem_info>();
            while (!subsystemInfo.desc.IsNull())
            {
                SubsystemInfo.Add(subsystemInfo);

                data += Marshal.SizeOf(subsystemInfo);
                data.ToStructure(subsystemInfo);
            }

            return true;
        }

        public bool SetSupportAchievements(IntPtr data)
        {
            if (data.IsNull())
                return false;

            SupportsAchievements = data.IsTrue();
            return true;
        }

        private bool LoadLibrary()
        {
            try
            {
                string corePath;
                switch (_wrapper.Settings.Platform)
                {
                    case Platform.Android:
                        corePath = $"{Wrapper.CoresDirectory}/{Name}_libretro_android.{_dll.Extension}";
                        break;
                    case Platform.Win:
                    case Platform.OSX:
                    case Platform.Linux:
                        corePath = $"{Wrapper.CoresDirectory}/{Name}_libretro.{_dll.Extension}";
                        break;
                    default:
                        _wrapper.LogHandler.LogError($"Runtime platform '{_wrapper.Settings.Platform}' not supported.", "SK.Libretro.Core.Start");
                        return false;
                }
                if (!FileSystem.FileExists(corePath))
                {
                    _wrapper.LogHandler.LogError($"Core '{Name}' at path '{corePath}' not found.", "SK.Libretro.Core.LoadLibrary");
                    return false;
                }

                string instancePath = System.IO.Path.Combine($"{Wrapper.TempDirectory}", $"{Name}_{Guid.NewGuid()}.{_dll.Extension}");
                File.Copy(corePath, instancePath);

                _dll.Load(instancePath);
                return true;
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
                Dispose();
                return false;
            }
        }

        private bool GetCoreFunctions()
        {
            try
            {
                _retro_init                       = _dll.GetFunction<retro_init_t>("retro_init");
                _retro_deinit                     = _dll.GetFunction<retro_deinit_t>("retro_deinit");
                _retro_api_version                = _dll.GetFunction<retro_api_version_t>("retro_api_version");
                _retro_get_system_info            = _dll.GetFunction<retro_get_system_info_t>("retro_get_system_info");
                _retro_get_system_av_info         = _dll.GetFunction<retro_get_system_av_info_t>("retro_get_system_av_info");
                _retro_set_controller_port_device = _dll.GetFunction<retro_set_controller_port_device_t>("retro_set_controller_port_device");
                _retro_reset                      = _dll.GetFunction<retro_reset_t>("retro_reset");
                _retro_run                        = _dll.GetFunction<retro_run_t>("retro_run");
                _retro_serialize_size             = _dll.GetFunction<retro_serialize_size_t>("retro_serialize_size");
                _retro_serialize                  = _dll.GetFunction<retro_serialize_t>("retro_serialize");
                _retro_unserialize                = _dll.GetFunction<retro_unserialize_t>("retro_unserialize");
                _retro_cheat_reset                = _dll.GetFunction<retro_cheat_reset_t>("retro_cheat_reset");
                _retro_cheat_set                  = _dll.GetFunction<retro_cheat_set_t>("retro_cheat_set");
                _retro_load_game                  = _dll.GetFunction<retro_load_game_t>("retro_load_game");
                _retro_load_game_special          = _dll.GetFunction<retro_load_game_special_t>("retro_load_game_special");
                _retro_unload_game                = _dll.GetFunction<retro_unload_game_t>("retro_unload_game");
                _retro_get_region                 = _dll.GetFunction<retro_get_region_t>("retro_get_region");
                _retro_get_memory_data            = _dll.GetFunction<retro_get_memory_data_t>("retro_get_memory_data");
                _retro_get_memory_size            = _dll.GetFunction<retro_get_memory_size_t>("retro_get_memory_size");
                
                _retro_set_environment           = _dll.GetFunction<retro_set_environment_t>("retro_set_environment");
                _retro_set_video_refresh         = _dll.GetFunction<retro_set_video_refresh_t>("retro_set_video_refresh");
                _retro_set_audio_sample          = _dll.GetFunction<retro_set_audio_sample_t>("retro_set_audio_sample");
                _retro_set_audio_sample_batch    = _dll.GetFunction<retro_set_audio_sample_batch_t>("retro_set_audio_sample_batch");
                _retro_set_input_poll            = _dll.GetFunction<retro_set_input_poll_t>("retro_set_input_poll");
                _retro_set_input_state           = _dll.GetFunction<retro_set_input_state_t>("retro_set_input_state");
                
                return true;
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
                Dispose();
                return false;
            }
        }

        private void GetSystemInfo()
        {
            _retro_get_system_info(out retro_system_info systemInfo);
            SystemInfo = new(systemInfo);
        }

        private void SetCallbacks()
        {
            _wrapper.EnvironmentHandler.SetCoreCallback(_retro_set_environment);
            _wrapper.GraphicsHandler.SetCoreCallback(_retro_set_video_refresh);
            _wrapper.AudioHandler.SetCoreCallbacks(_retro_set_audio_sample, _retro_set_audio_sample_batch);
            _wrapper.InputHandler.SetCoreCallbacks(_retro_set_input_poll, _retro_set_input_state);
        }
    }
}
