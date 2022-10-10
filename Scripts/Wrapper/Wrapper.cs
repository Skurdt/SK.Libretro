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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SK.Libretro.Header;

[assembly: InternalsVisibleTo("SK.Libretro.NAudio")]
#if UNITY_EDITOR || UNITY_STANDALONE
[assembly: InternalsVisibleTo("SK.Libretro.Unity")]
#endif

namespace SK.Libretro
{
    internal sealed class Wrapper
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

        public static readonly retro_log_level LogLevel = retro_log_level.RETRO_LOG_WARN;

        public static string MainDirectory        { get; private set; } = null;
        public static string CoresDirectory       { get; private set; } = null;
        public static string CoreOptionsDirectory { get; private set; } = null;
        public static string SystemDirectory      { get; private set; } = null;
        public static string CoreAssetsDirectory  { get; private set; } = null;
        public static string SavesDirectory       { get; private set; } = null;
        public static string StatesDirectory      { get; private set; } = null;
        public static string TempDirectory        { get; private set; } = null;

        public bool OptionCropOverscan
        {
            get => _optionCropOverscan;
            set
            {
                if (_optionCropOverscan != value)
                {
                    _optionCropOverscan = value;
                    UpdateVariables = true;
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
                    _optionUserName = value;
                    UpdateVariables = true;
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
                    _optionLanguage = value;
                    UpdateVariables = true;
                }
            }
        }

        public readonly TargetPlatform TargetPlatform;
        public readonly Core Core;
        public readonly Game Game;
        public readonly Environment Environment;
        public readonly Graphics Graphics;
        public readonly Audio Audio;
        public readonly Input Input;
        public readonly Serialization Serialization;
        public readonly retro_environment_t EnvironmentCallback;
        public readonly retro_video_refresh_t VideoRefreshCallback;
        public readonly retro_audio_sample_t AudioSampleCallback;
        public readonly retro_audio_sample_batch_t AudioSampleBatchCallback;
        public readonly retro_input_poll_t InputPollCallback;
        public readonly retro_input_state_t InputStateCallback;
        public readonly retro_log_printf_t LogPrintfCallback;

        public bool RewindEnabled = false;
        public bool PerformRewind = false;

        public OpenGL OpenGL;
        public retro_hw_render_callback HwRenderInterface;
        public retro_frame_time_callback FrameTimeInterface;
        public retro_frame_time_callback_t FrameTimeInterfaceCallback;
        public DiskInterface Disk;
        public PerfInterface Perf;
        public LedInterface Led;
        public MemoryMap Memory;

        public bool UpdateVariables = false;

        private const int REWIND_FRAMES_INTERVAL = 10;

        private readonly List<IntPtr> _unsafeStrings = new();

        private Language _optionLanguage = Language.English;
        private string _optionUserName   = "LibretroUnityFE's Awesome User";
        private bool _optionCropOverscan = true;
        private long _frameTimeLast      = 0;
        private uint _totalFrameCount    = 0;

        public unsafe Wrapper(TargetPlatform targetPlatform, string baseDirectory = null)
        {
            TargetPlatform = targetPlatform;

            if (MainDirectory is null)
            {
                MainDirectory        = !string.IsNullOrWhiteSpace(baseDirectory) ? baseDirectory : "libretro";
                CoresDirectory       = $"{MainDirectory}/cores";
                CoreOptionsDirectory = $"{MainDirectory}/core_options";
                SystemDirectory      = $"{MainDirectory}/system";
                CoreAssetsDirectory  = $"{MainDirectory}/core_assets";
                SavesDirectory       = $"{MainDirectory}/saves";
                StatesDirectory      = $"{MainDirectory}/states";
                TempDirectory        = $"{MainDirectory}/temp";

                if (!Directory.Exists(MainDirectory))
                    _ = Directory.CreateDirectory(MainDirectory);

                if (!Directory.Exists(CoresDirectory))
                    _ = Directory.CreateDirectory(CoresDirectory);

                if (!Directory.Exists(CoreOptionsDirectory))
                    _ = Directory.CreateDirectory(CoreOptionsDirectory);

                if (!Directory.Exists(SystemDirectory))
                    _ = Directory.CreateDirectory(SystemDirectory);
            }

            Core = new Core(this);
            Game = new Game(this);

            Environment   = new Environment(this);
            Graphics      = new Graphics(this, false);
            Audio         = new Audio(this);
            Input         = new Input();
            Serialization = new Serialization(this);

            EnvironmentCallback      = Environment.Callback;
            VideoRefreshCallback     = Graphics.Callback;
            AudioSampleCallback      = Audio.SampleCallback;
            AudioSampleBatchCallback = Audio.SampleBatchCallback;
            InputPollCallback        = Input.PollCallback;
            InputStateCallback       = Input.StateCallback;

            LogPrintfCallback = LogInterface.RetroLogPrintf;
        }

        public bool StartContent(string coreName, string gameDirectory, string gameName)
        {
            if (string.IsNullOrWhiteSpace(coreName))
                return false;

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

            if (Core.HwAccelerated && OpenGL == null)
            {
                StopContent();
                return false;
            }

            FrameTimeRestart();

            ulong size = Core.retro_serialize_size();
            if (size > 0)
                Serialization.SetStateSize(size);

            return true;
        }

        public void StopContent()
        {
            Input.DeInit();
            Audio.DeInit();
            Graphics.DeInit();

            Game.Stop();
            Core.Stop();

            PointerUtilities.Free(_unsafeStrings);
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

            //if (RewindEnabled)
            //{
            //    if (PerformRewind)
            //        Serialization.RewindLoadState();
            //    else if (_totalFrameCount % REWIND_FRAMES_INTERVAL == 0)
            //        Serialization.RewindSaveState();
            //}

            Core.retro_run();
        }

        public ControllersMap DeviceMap => Input.DeviceMap;

        internal IntPtr GetUnsafeString(string source)
        {
            IntPtr ptr = Marshal.StringToHGlobalAnsi(source);
            _unsafeStrings.Add(ptr);
            return ptr;
        }

        private void FrameTimeRestart() => _frameTimeLast = System.Diagnostics.Stopwatch.GetTimestamp();

        private void FrameTimeUpdate()
        {
            if (FrameTimeInterfaceCallback == null)
                return;

            long current = System.Diagnostics.Stopwatch.GetTimestamp();
            long delta   = current - _frameTimeLast;

            if (_frameTimeLast <= 0)
                delta = FrameTimeInterface.reference;
            _frameTimeLast = current;
            FrameTimeInterfaceCallback(delta * 1000);
        }
    }
}
