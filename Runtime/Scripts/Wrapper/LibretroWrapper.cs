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

using Nito.Collections;
using SK.Libretro.Utilities;
using SK.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroWrapper
    {
        public enum Language
        {
            English,
            Japanese,
            French,
            Spanish,
            German,
            Italian,
            Dutch,
            Portuguese_brazil,
            Portuguese_portugal,
            Russian,
            Korean,
            Chinese_traditional,
            Chinese_simplified,
            Esperanto,
            Polish,
            Vietnamese,
            Arabic,
            Greek,
            Turkish,
            Slovak,
            Persian,
            Hebrew,
            Asturian
        }

        public static string MainDirectory       { get; private set; } = null;
        public static string CoresDirectory      { get; private set; } = null;
        public static string SystemDirectory     { get; private set; } = null;
        public static string CoreAssetsDirectory { get; private set; } = null;
        public static string SavesDirectory      { get; private set; } = null;
        public static string TempDirectory       { get; private set; } = null;
        public static string CoreOptionsFile     { get; private set; } = null;

        public bool OptionCropOverscan
        {
            get => _optionCropOverscan;
            set
            {
                if (_optionCropOverscan != value)
                {
                    _optionCropOverscan         = value;
                    Environment.UpdateVariables = true;
                }
            }
        }
        public string OptionUserName
        {
            get => _optionUserName;
            set
            {
                if (!_optionUserName.Equals(value, StringComparison.Ordinal))
                {
                    _optionUserName             = value;
                    Environment.UpdateVariables = true;
                }
            }
        }
        public Language OptionLanguage
        {
            get => _optionLanguage;
            set
            {
                if (_optionLanguage != value)
                {
                    _optionLanguage             = value;
                    Environment.UpdateVariables = true;
                }
            }
        }
        public bool RewindEnabled { get; set; } = false;
        public bool PerformRewind { get; set; } = false;

        public static readonly retro_log_level LogLevel = retro_log_level.RETRO_LOG_WARN;

        public readonly LibretroTargetPlatform TargetPlatform;

        public readonly LibretroCore Core;
        public readonly LibretroGame Game;

        public readonly LibretroEnvironment Environment;
        public readonly LibretroGraphics Graphics;
        public readonly LibretroAudio Audio;
        public readonly LibretroInput Input;
        public readonly LibretroSerialization Serialization;
        public LibretroDiskInterface Disk { get; internal set; }
        public LibretroMessageInterface Message { get; internal set; }
        public LibretroPerfInterface Perf { get; internal set; }
        public LibretroLedInterface Led { get; internal set; }
        public LibretroMemory Memory { get; internal set; }

        public readonly retro_environment_t EnvironmentCallback;
        public readonly retro_video_refresh_t VideoRefreshCallback;
        public readonly retro_audio_sample_t AudioSampleCallback;
        public readonly retro_audio_sample_batch_t AudioSampleBatchCallback;
        public readonly retro_input_poll_t InputPollCallback;
        public readonly retro_input_state_t InputStateCallback;
        public readonly retro_log_printf_t LogPrintfCallback;

        public LibretroOpenGL OpenGL;
        public retro_hw_render_callback HwRenderInterface;

        public retro_frame_time_callback FrameTimeInterface;
        public retro_frame_time_callback_t FrameTimeInterfaceCallback;

        private Language _optionLanguage = Language.English;
        private string _optionUserName   = "LibretroUnityFE's Awesome User";
        private bool _optionCropOverscan = true;

        private const int REWIND_NUM_MAX_STATES          = 4096;
        private const int REWIND_FRAMES_INTERVAL         = 10;
        private readonly Deque<byte[]> _rewindSaveStates = new Deque<byte[]>(REWIND_NUM_MAX_STATES);
        private ulong _rewindSaveStateSize               = ulong.MaxValue;

        private readonly List<IntPtr> _unsafePointers = new List<IntPtr>();

        private long _frameTimeLast   = 0;
        private uint _totalFrameCount = 0;

        public unsafe LibretroWrapper(LibretroTargetPlatform targetPlatform, string baseDirectory = null)
        {
            TargetPlatform = targetPlatform;

            if (MainDirectory is null)
            {
                MainDirectory       = !string.IsNullOrEmpty(baseDirectory) ? baseDirectory : "libretro";
                CoresDirectory      = $"{MainDirectory}/cores";
                SystemDirectory     = $"{MainDirectory}/system";
                CoreAssetsDirectory = $"{MainDirectory}/core_assets";
                SavesDirectory      = $"{MainDirectory}/saves";
                TempDirectory       = $"{MainDirectory}/temp";
                CoreOptionsFile     = $"{MainDirectory}/core_options.json";

                string dir = FileSystem.GetAbsolutePath(MainDirectory);
                if (!Directory.Exists(dir))
                    _ = Directory.CreateDirectory(dir);

                dir = FileSystem.GetAbsolutePath(CoresDirectory);
                if (!Directory.Exists(dir))
                    _ = Directory.CreateDirectory(dir);

                dir = Path.GetFullPath(TempDirectory);
                if (Directory.Exists(dir))
                {
                    string[] fileNames = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
                    foreach (string fileName in fileNames)
                        _ = FileSystem.DeleteFile(fileName);
                }
            }

            Core = new LibretroCore(this);
            Game = new LibretroGame(this);

            Environment   = new LibretroEnvironment(this);
            Graphics      = new LibretroGraphics(this);
            Audio         = new LibretroAudio(this);
            Input         = new LibretroInput();
            Serialization = new LibretroSerialization(this);

            EnvironmentCallback      = Environment.Callback;
            VideoRefreshCallback     = Graphics.Callback;
            AudioSampleCallback      = Audio.SampleCallback;
            AudioSampleBatchCallback = Audio.SampleBatchCallback;
            InputPollCallback        = Input.PollCallback;
            InputStateCallback       = Input.StateCallback;

            LogPrintfCallback = LibretroLog.RetroLogPrintf;
        }

        public bool StartContent(string coreName, string gameDirectory, string gameName)
        {
            if (string.IsNullOrEmpty(coreName))
                return false;

            LibretroCoreOptions.LoadCoreOptionsFile();

            if (!Core.Start(coreName))
            {
                StopContent();
                return false;
            }

            if (FrameTimeInterface.callback != IntPtr.Zero)
                FrameTimeInterfaceCallback = Marshal.GetDelegateForFunctionPointer<retro_frame_time_callback_t>(FrameTimeInterface.callback);

            if (!Game.Start(gameDirectory, gameName))
            {
                StopContent();
                return false;
            }

            if (Core.HwAccelerated && OpenGL is null)
            {
                StopContent();
                return false;
            }

            Core.retro_set_controller_port_device(0, RETRO_DEVICE_JOYPAD);

            FrameTimeRestart();

            return true;
        }

        public void StopContent()
        {
            Input.Disable();
            Audio.Disable();
            Graphics.Disable();

            Game.Stop();
            Core.Stop();

            FreeUnsafePointers();
            Thread.Sleep(200);
        }

        public void InitHardwareContext() => Marshal.GetDelegateForFunctionPointer<retro_hw_context_reset_t>(HwRenderInterface.context_reset).Invoke();

        public void RunFrame()
        {
            if (!Game.Running || !Core.Initialized)
                return;

            if (Core.HwAccelerated)
                OpenGL.PollEvents();

            _totalFrameCount++;

            FrameTimeUpdate();

            if (RewindEnabled)
            {
                if (PerformRewind)
                    RewindLoadState();
                else if (_totalFrameCount % REWIND_FRAMES_INTERVAL == 0)
                    RewindSaveState();
            }

            Core.retro_run();
        }

        internal unsafe char* GetUnsafeString(string source)
        {
            char* result = UnsafeStringUtils.StringToChars(source, out IntPtr ptr);
            _unsafePointers.Add(ptr);
            return result;
        }

        private void FrameTimeRestart() => _frameTimeLast = System.Diagnostics.Stopwatch.GetTimestamp();

        private void FrameTimeUpdate()
        {
            if (FrameTimeInterfaceCallback is null)
                return;

            long current = System.Diagnostics.Stopwatch.GetTimestamp();
            long delta   = current - _frameTimeLast;

            if (_frameTimeLast <= 0)
                delta = FrameTimeInterface.reference;
            _frameTimeLast = current;
            FrameTimeInterfaceCallback(delta * 1000);
        }

        private unsafe void RewindSaveState()
        {
            if (_rewindSaveStateSize == ulong.MaxValue)
                _rewindSaveStateSize = Core.retro_serialize_size();

            if (_rewindSaveStateSize == 0)
                return;

            byte[] rewindSaveStatedata = new byte[_rewindSaveStateSize];
            fixed (byte* p = rewindSaveStatedata)
            {
                if (Core.retro_serialize(p, _rewindSaveStateSize))
                {
                    if (_rewindSaveStates.Count == REWIND_NUM_MAX_STATES)
                        _ = _rewindSaveStates.RemoveFromFront();
                    _rewindSaveStates.AddToBack(rewindSaveStatedata);
                }
            }
        }

        private unsafe void RewindLoadState()
        {
            if (_rewindSaveStateSize == 0 || _rewindSaveStates.Count == 0)
                return;

            byte[] data = _rewindSaveStates.RemoveFromBack();
            fixed (byte* p = data)
            {
                _ = Core.retro_unserialize(p, _rewindSaveStateSize);
            }
        }

        private void FreeUnsafePointers()
        {
            for (int i = 0; i < _unsafePointers.Count; ++i)
            {
                if (_unsafePointers[i] != IntPtr.Zero)
                    Marshal.FreeHGlobal(_unsafePointers[i]);
            }
        }
    }
}
