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
using System.Linq;
using System.Runtime.InteropServices;
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
        public bool DoRewind { get; set; } = false;
        public bool HwRenderHasDepth => HwRenderInterface.depth;
        public bool HwRenderHasStencil => HwRenderInterface.stencil;

        public static readonly retro_log_level LogLevel = retro_log_level.RETRO_LOG_WARN;

        public static LibretroCoreOptionsList CoreOptionsList = null;

        public readonly LibretroGraphics Graphics;
        public readonly LibretroAudio Audio;
        public readonly LibretroInput Input;

        public readonly LibretroCore Core;
        public readonly LibretroGame Game;

        public readonly LibretroTargetPlatform TargetPlatform;
        public readonly LibretroEnvironment Environment;

        public readonly retro_environment_t EnvironmentCallback;
        public readonly retro_video_refresh_t VideoRefreshCallback;
        public readonly retro_audio_sample_t AudioSampleCallback;
        public readonly retro_audio_sample_batch_t AudioSampleBatchCallback;
        public readonly retro_input_poll_t InputPollCallback;
        public readonly retro_input_state_t InputStateCallback;
        public readonly retro_log_printf_t LogPrintfCallback;

        public retro_frame_time_callback FrameTimeInterface           = default;
        public retro_frame_time_callback_t FrameTimeInterfaceCallback = null;
        public retro_hw_render_callback HwRenderInterface             = default;

        private Language _optionLanguage = Language.English;
        private string _optionUserName   = "LibretroUnityFE's Awesome User";
        private bool _optionCropOverscan = true;

        private sealed class SaveStateData
        {
            public readonly byte[] Data;
            public readonly ulong Size;

            public SaveStateData(byte[] data, ulong size)
            {
                Data = data;
                Size = size;
            }
        }

        private const int MAX_STATES_COUNT = 2048;

        private readonly List<IntPtr> _unsafePointers          = new List<IntPtr>();
        private readonly List<SaveStateData> _rewindSaveStates = new List<SaveStateData>(MAX_STATES_COUNT);

        private LibretroPlugin.InteropInterface _interopInterface = default;
        private uint _totalFrameCount                             = 0;
        private long _frameTimeLast                               = 0;

        public unsafe LibretroWrapper(LibretroTargetPlatform targetPlatform, string baseDirectory = null)
        {
            TargetPlatform = targetPlatform;

            if (MainDirectory == null)
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

            Environment = new LibretroEnvironment(this);
            Graphics    = new LibretroGraphics(this);
            Audio       = new LibretroAudio(this);
            Input       = new LibretroInput();

            EnvironmentCallback      = Environment.Callback;
            VideoRefreshCallback     = Graphics.Callback;
            AudioSampleCallback      = Audio.SampleCallback;
            AudioSampleBatchCallback = Audio.SampleBatchCallback;
            InputPollCallback        = Input.PollCallback;
            InputStateCallback       = Input.StateCallback;

            LogPrintfCallback = LibretroLog.RetroLogPrintf;
        }

        public bool StartGame(string coreName, string gameDirectory, string gameName)
        {
            if (string.IsNullOrEmpty(coreName))
                return false;

            LoadCoreOptionsFile();

            if (!Core.Start(coreName))
                return false;

            if (FrameTimeInterface.callback != IntPtr.Zero)
                FrameTimeInterfaceCallback = Marshal.GetDelegateForFunctionPointer<retro_frame_time_callback_t>(FrameTimeInterface.callback);

            if (!Game.Start(gameDirectory, gameName))
                return false;

            if (Core.HwAccelerated)
            {
                _interopInterface = new LibretroPlugin.InteropInterface
                {
                    context_reset   = HwRenderInterface.context_reset,
                    context_destroy = HwRenderInterface.context_destroy,
                    retro_run       = Marshal.GetFunctionPointerForDelegate(Core.retro_run)
                };
                LibretroPlugin.SetupInteropInterface(ref _interopInterface);
            }

            FrameTimeRestart();

            return true;
        }

        public void StopGame()
        {
            DeactivateInput();
            DeactivateAudio();
            DeactivateGraphics();

            Game.Stop();
            Core.Stop();

            FreeUnsafePointers();
        }

        public void FrameTimeRestart() => _frameTimeLast = System.Diagnostics.Stopwatch.GetTimestamp();

        public void FrameTimeUpdate()
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

        public void Update()
        {
            if (!Game.Running || !Core.Initialized)
                return;

            _totalFrameCount++;

            if (DoRewind && _rewindSaveStates.Count > 0)
            {
                RewindLoadState(_rewindSaveStates[_rewindSaveStates.Count - 1]);
                _rewindSaveStates.RemoveAt(_rewindSaveStates.Count - 1);
            }
            else if (_totalFrameCount % 2 == 0)
            {
                if (_rewindSaveStates.Count >= MAX_STATES_COUNT)
                    _rewindSaveStates.RemoveAt(0);
                _rewindSaveStates.Add(RewindSaveState());
            }

            if (!Core.HwAccelerated)
                Core.retro_run();
        }

        public void ActivateGraphics(IGraphicsProcessor graphicsProcessor) => Graphics.Processor = graphicsProcessor;

        public void DeactivateGraphics()
        {
            Graphics.Processor?.DeInit();
            Graphics.Processor = null;
        }

        public void ActivateAudio(IAudioProcessor audioProcessor)
        {
            Audio.Processor = audioProcessor;
            Audio.Init();
        }

        public void DeactivateAudio()
        {
            Audio.DeInit();
            Audio.Processor = null;
        }

        public void ActivateInput(IInputProcessor inputProcessor) => Input.Processor = inputProcessor;

        public void DeactivateInput() => Input.Processor = null;

        public unsafe bool SaveState(int index, out string outPath)
        {
            outPath = null;

            ulong size = Core.retro_serialize_size();
            if (size <= 0)
                return false;

            byte[] data = new byte[size];
            fixed (byte* p = data)
            {
                if (!Core.retro_serialize(p, size))
                    return false;
            }

            string coreDirectory = Path.Combine(SavesDirectory, Core.Name);
            if (!Directory.Exists(coreDirectory))
                _ = Directory.CreateDirectory(coreDirectory);

            string gameDirectory = Game.Name != null ? Path.Combine(coreDirectory, Path.GetFileNameWithoutExtension(Game.Name)) : null;
            if (gameDirectory != null && !Directory.Exists(gameDirectory))
                _ = Directory.CreateDirectory(gameDirectory);

            outPath = Path.GetFullPath(gameDirectory != null ? Path.Combine(gameDirectory, $"save_{index}.state") : Path.Combine(coreDirectory, $"save_{index}.state"));
            File.WriteAllBytes(outPath, data);

            return true;
        }

        public unsafe bool LoadState(int index)
        {
            string coreDirectory = Path.Combine(SavesDirectory, Core.Name);
            if (!Directory.Exists(coreDirectory))
                return false;

            string gameDirectory = Game.Name != null ? Path.Combine(coreDirectory, Path.GetFileNameWithoutExtension(Game.Name)) : null;
            if (gameDirectory != null && !Directory.Exists(gameDirectory))
                return false;

            string savePath = gameDirectory != null ? Path.Combine(gameDirectory, $"save_{index}.state") : Path.Combine(coreDirectory, $"save_{index}.state");

            if (!File.Exists(savePath))
                return false;

            ulong size = Core.retro_serialize_size();
            if (size <= 0)
                return false;

            byte[] data = File.ReadAllBytes(savePath);
            fixed (byte* p = data)
                _ = Core.retro_unserialize(p, size); // FIXME: This returns false, not sure why

            return true;
        }

        public static void LoadCoreOptionsFile()
        {
            CoreOptionsList = FileSystem.DeserializeFromJson<LibretroCoreOptionsList>(CoreOptionsFile);
            if (CoreOptionsList == null)
                CoreOptionsList = new LibretroCoreOptionsList();
        }

        public static void SaveCoreOptionsFile()
        {
            if (CoreOptionsList == null || CoreOptionsList.Cores.Count == 0)
                return;

            CoreOptionsList.Cores = CoreOptionsList.Cores.OrderBy(x => x.CoreName).ToList();
            for (int i = 0; i < CoreOptionsList.Cores.Count; ++i)
                CoreOptionsList.Cores[i].Options.Sort();
            _ = FileSystem.SerializeToJson(CoreOptionsList, CoreOptionsFile);
        }

        public unsafe char* GetUnsafeString(string source)
        {
            char* result = UnsafeStringUtils.StringToChars(source, out IntPtr ptr);
            _unsafePointers.Add(ptr);
            return result;
        }

        private unsafe SaveStateData RewindSaveState()
        {
            ulong size = Core.retro_serialize_size();
            if (size <= 0)
                return null;

            byte[] data = new byte[size];
            fixed (byte* p = data)
                return Core.retro_serialize(p, size) ? new SaveStateData(data, size) : null;
        }

        private unsafe void RewindLoadState(SaveStateData saveStateData)
        {
            if (saveStateData == null)
                return;

            fixed (byte* p = saveStateData.Data)
                _ = Core.retro_unserialize(p, saveStateData.Size);
        }

        private void FreeUnsafePointers()
        {
            for (int i = 0; i < _unsafePointers.Count; ++i)
                Marshal.FreeHGlobal(_unsafePointers[i]);
        }
    }
}
