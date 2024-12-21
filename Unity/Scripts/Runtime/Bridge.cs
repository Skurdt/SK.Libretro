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
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SK.Libretro.Unity
{
    internal sealed class Bridge
    {
        public static Bridge Instance
        {
            get
            {
                lock (_lock)
                {
                    _instance ??= new();
                    return _instance;
                }
            }
        }

        public bool Running
        {
            get
            {
                lock (_lock)
                    return _running;
            }
            private set
            {
                lock (_lock)
                    _running = value;
            }
        }

        public bool Paused
        {
            get
            {
                lock (_lock)
                    return _paused;
            }
            private set
            {
                lock (_lock)
                    _paused = value;
            }
        }

        public int FastForwardFactor
        {
            get
            {
                lock (_lock)
                    return _fastForwardFactor;
            }
            set
            {
                lock (_lock)
                    _fastForwardFactor = math.clamp(value, 2, 32);
            }
        }

        public bool FastForward
        {
            get
            {
                lock (_lock)
                    return _fastForward;
            }
            set
            {
                lock (_lock)
                    _fastForward = value;
            }
        }

        public bool Rewind
        {
            get
            {
                lock (_lock)
                    return _rewind;
            }
            set
            {
                lock (_lock)
                    _rewind = value;
            }
        }

        public bool InputEnabled
        {
            get
            {
                lock (_lock)
                    return _inputEnabled;
            }
            set
            {
                lock (_lock)
                {
                    _inputEnabled = value;
                    _bridgeCommands.Enqueue(new EnableInputBridgeCommand(value));
                }
            }
        }

        private string CoreName
        {
            get
            {
                lock (_lock)
                    return _coreName;
            }

            set
            {
                lock (_lock)
                    _coreName = value;
            }
        }

        private string GamesDirectory
        {
            get
            {
                lock (_lock)
                    return _gamesDirectory;
            }

            set
            {
                lock (_lock)
                    _gamesDirectory = value;
            }
        }

        private string[] GameNames
        {
            get
            {
                lock (_lock)
                    return _gameNames;
            }

            set
            {
                lock (_lock)
                    _gameNames = value;
            }
        }

        public bool ReadSaveMemory
        {
            get
            {
                lock (_lock)
                    return _readSaveMemory;
            }

            set
            {
                lock (_lock)
                    _readSaveMemory = value;
            }
        }

        public bool ReadRtcMemory
        {
            get
            {
                lock (_lock)
                    return _readRtcMemory;
            }

            set
            {
                lock (_lock)
                    _readRtcMemory = value;
            }
        }

        public bool ReadSystemMemory
        {
            get
            {
                lock (_lock)
                    return _readSystemMemory;
            }

            set
            {
                lock (_lock)
                    _readSystemMemory = value;
            }
        }

        public bool ReadVideoMemory
        {
            get
            {
                lock (_lock)
                    return _readVideoMemory;
            }

            set
            {
                lock (_lock)
                    _readVideoMemory = value;
            }
        }

        public byte[] SaveMemory
        {
            get
            {
                lock (_lock)
                    return _saveMemory;
            }

            private set
            {
                lock (_lock)
                    _saveMemory = value;
            }
        }

        public byte[] RtcMemory
        {
            get
            {
                lock (_lock)
                    return _rtcMemory;
            }

            private set
            {
                lock (_lock)
                    _rtcMemory = value;
            }
        }

        public byte[] SystemMemory
        {
            get
            {
                lock (_lock)
                    return _systemMemory;
            }

            private set
            {
                lock (_lock)
                    _systemMemory = value;
            }
        }

        public byte[] VideoMemory
        {
            get
            {
                lock (_lock)
                    return _videoMemory;
            }

            private set
            {
                lock (_lock)
                    _videoMemory = value;
            }
        }

        public (Options, Options) Options
        {
            get
            {
                lock (_lock)
                    return _options;
            }

            private set
            {
                lock (_lock)
                    _options = value;
            }
        }

        public bool DiskHandlerEnabled
        {
            get
            {
                lock (_lock)
                    return _diskHandlerEnabled;
            }

            private set
            {
                lock (_lock)
                    _diskHandlerEnabled = value;
            }
        }

        public ControllersMap ControllersMap { get; private set; }

        private const int DEFAULT_FASTFORWARD_FACTOR = 8;

        private LibretroInstance InstanceComponent { get; set; }
        private int ShaderTextureId                { get; set; }
        private Material OriginalMaterial          { get; set; }

        private static readonly object _lock = new();

        private readonly string _mainDirectory;
        private readonly string _tempDirectory;
        private readonly ManualResetEventSlim _manualResetEvent;
        private readonly ConcurrentQueue<IBridgeCommand> _bridgeCommands;

        private string _coreName;
        private string _gamesDirectory;
        private string[] _gameNames;
        private Action _instanceStartedCallback;
        private Action _instanceStoppedCallback;

        private bool _running;
        private bool _paused;
        private int _fastForwardFactor;
        private bool _fastForward;
        private bool _rewind;
        private bool _inputEnabled;
        private bool _readSaveMemory;
        private bool _readRtcMemory;
        private bool _readSystemMemory;
        private bool _readVideoMemory;
        private byte[] _saveMemory;
        private byte[] _rtcMemory;
        private byte[] _systemMemory;
        private byte[] _videoMemory;
        private (Options, Options) _options;
        private bool _diskHandlerEnabled;

        private Thread _thread;
        private Texture2D _texture;
        private static Bridge _instance;

        private Bridge()
        {
            _mainDirectory     = $"{Application.persistentDataPath}/Libretro";
            _tempDirectory     = Application.platform switch
            {
                UnityEngine.RuntimePlatform.Android => $"{GetAndroidPrivateAppDataPath()}/temp",
                _                                   => $"{_mainDirectory}/temp"
            };
            _manualResetEvent  = new(false);
            _bridgeCommands    = new();
            _fastForwardFactor = DEFAULT_FASTFORWARD_FACTOR;
        }

        public void StartContent(LibretroInstance instanceComponent,
                                 string coreName,
                                 string gamesDirectory,
                                 string[] gameNames,
                                 Action instanceStartedCallback,
                                 Action instanceStoppedCallback)
        {
            if (string.IsNullOrWhiteSpace(coreName))
            {
                Debug.LogError("Core is not set");
                return;
            }

            StopContent();

            InstanceComponent = instanceComponent;
            ShaderTextureId   = Shader.PropertyToID(InstanceComponent.Settings.ShaderTextureName);
            OriginalMaterial  = instanceComponent.Renderer ? new(instanceComponent.Renderer.material) : null;
            ReadSaveMemory    = instanceComponent.Settings.ReadSaveMemory;
            ReadRtcMemory     = instanceComponent.Settings.ReadRtcMemory;
            ReadSystemMemory  = instanceComponent.Settings.ReadSystemMemory;
            ReadVideoMemory   = instanceComponent.Settings.ReadVideoMemory;

            CoreName                 = coreName;
            GamesDirectory           = gamesDirectory;
            GameNames                = gameNames;
            _instanceStartedCallback = instanceStartedCallback;
            _instanceStoppedCallback = instanceStoppedCallback;

            _thread = new Thread(LibretroThread)
            {
                Name         = $"LibretroThread_{CoreName}_{(GameNames.Length > 0 ? GameNames[0] : "nogame")}",
                IsBackground = true,
                Priority     = System.Threading.ThreadPriority.Lowest
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        private async void LibretroThread()
        {
            try
            {
                Platform platform = Application.platform switch
                {
                    UnityEngine.RuntimePlatform.OSXEditor
                    or UnityEngine.RuntimePlatform.OSXPlayer     => Platform.OSX,
                    UnityEngine.RuntimePlatform.WindowsPlayer
                    or UnityEngine.RuntimePlatform.WindowsEditor => Platform.Win,
                    UnityEngine.RuntimePlatform.IPhonePlayer     => Platform.IOS,
                    UnityEngine.RuntimePlatform.Android          => Platform.Android,
                    UnityEngine.RuntimePlatform.LinuxPlayer
                    or UnityEngine.RuntimePlatform.LinuxEditor   => Platform.Linux,
                    _                                            => Platform.None
                };

                (ILogProcessor logProcessor, IGraphicsProcessor graphicsProcessor, IAudioProcessor audioProcessor, IInputProcessor inputProcessor, ILedProcessor ledProcessor) = await GetProcessors();

                WrapperSettings wrapperSettings = new(platform)
                {
                    LogLevel          = LogLevel.Warning,
                    MainDirectory     = _mainDirectory,
                    TempDirectory     = _tempDirectory,
                    LogProcessor      = logProcessor,
                    GraphicsProcessor = graphicsProcessor,
                    AudioProcessor    = audioProcessor,
                    InputProcessor    = inputProcessor,
                    LedProcessor      = ledProcessor
                };

                if (!Wrapper.Instance.StartContent(wrapperSettings, CoreName, GamesDirectory, GameNames))
                {
                    Debug.LogError("Failed to start core/game combination");
                    return;
                }

                Wrapper.Instance.InitGraphics();
                Wrapper.Instance.InitAudio();
                Wrapper.Instance.InputHandler.Enabled = true;
                DiskHandlerEnabled = Wrapper.Instance.DiskHandler.Enabled;
                ControllersMap     = Wrapper.Instance.InputHandler.DeviceMap;

                //_Wrapper.Instance.RewindEnabled = _settings.RewindEnabled;

                Options = (Wrapper.Instance.OptionsHandler.CoreOptions, Wrapper.Instance.OptionsHandler.GameOptions);

                InvokeInstanceEvent(_instanceStartedCallback);

                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                double gameFrameTime = 1.0 / Wrapper.Instance.Game.SystemAVInfo.Fps;
                double startTime     = 0.0;
                double accumulator   = 0.0;

                Running = true;
                while (Running)
                {
                    lock (_lock)
                        while (_bridgeCommands.Count > 0)
                            if (_bridgeCommands.TryDequeue(out IBridgeCommand command))
                                command.Execute();

                    if (Paused)
                        _manualResetEvent.Wait();

                    if (!Running)
                        break;

                    //_Wrapper.Instance.RewindEnabled = _settings.RewindEnabled;

                    //if (_settings.RewindEnabled)
                    //    _Wrapper.Instance.PerformRewind = Rewind;

                    double currentTime = stopwatch.Elapsed.TotalSeconds;
                    double dt = currentTime - startTime;
                    startTime = currentTime;

                    double targetFrameTime = /*FastForward && FastForwardFactor > 0 ? gameFrameTime / FastForwardFactor : */gameFrameTime;
                    if ((accumulator += dt) >= targetFrameTime)
                    {
                        Wrapper.Instance.RunFrame();
                        if (ReadSaveMemory)
                            SaveMemory = Wrapper.Instance.MemoryHandler.GetSaveMemory().ToArray();
                        if (ReadRtcMemory)
                            RtcMemory = Wrapper.Instance.MemoryHandler.GetRtcMemory().ToArray();
                        if (ReadSystemMemory)
                            SystemMemory = Wrapper.Instance.MemoryHandler.GetSystemMemory().ToArray();
                        if (ReadVideoMemory)
                            VideoMemory = Wrapper.Instance.MemoryHandler.GetVideoMemory().ToArray();
                        accumulator = 0.0;
                    }
                }

                InvokeInstanceEvent(_instanceStoppedCallback);

                lock (_lock)
                    Wrapper.Instance.StopContent();

                StopThread();

                _instance = null;
            }
            catch (Exception e) when (e is not ThreadAbortException)
            {
                Debug.LogException(e);
            }
        }

        public void PauseContent()
        {
            if (!Running || Paused)
                return;

            Paused = true;

            _manualResetEvent.Reset();
        }

        public void ResumeContent()
        {
            if (!Running || !Paused)
                return;

            Paused = false;

            _manualResetEvent.Set();
        }

        public void ResetContent()
        {
            //if (!Running)
            //    return;

            //if (Paused)
            //    ResumeContent();

            //Running = false;

            //lock (_lock)
            //    _wrapper?.ResetContent();
        }

        public void StopContent()
        {
            if (!Running)
                return;

            if (Paused)
                ResumeContent();

            Running = false;
        }

        public void SetStateSlot(int slot) =>
            _bridgeCommands.Enqueue(new SetStateSlotBridgeCommand(slot));

        public void SaveStateWithScreenshot() =>
            _bridgeCommands.Enqueue(new SaveStateWithScreenshotBridgeCommand(TakeScreenshot));

        public void SaveStateWithoutScreenshot() =>
            _bridgeCommands.Enqueue(new SaveStateWithoutScreenshotBridgeCommand());

        public void LoadState() =>
            _bridgeCommands.Enqueue(new LoadStateBridgeCommand());

        public void SetDiskIndex(int index) =>
            _bridgeCommands.Enqueue(new SetDiskIndexBridgeCommand(GamesDirectory, GameNames, index));

        public void SaveSRAM() =>
            _bridgeCommands.Enqueue(new SaveSRAMBridgeCommand());

        public void LoadSRAM() =>
            _bridgeCommands.Enqueue(new LoadSRAMBridgeCommand());

        public void SaveOptions(bool global) =>
            _bridgeCommands.Enqueue(new SaveOptionsBridgeCommand(global));

        public void SetControllerPortDevice(uint port, RETRO_DEVICE device) =>
            _bridgeCommands.Enqueue(new SetControllerPortDeviceBridgeCommand(port, device));

        private void SetTexture(Texture texture)
        {
            if (!Application.isPlaying || !texture || !InstanceComponent.Renderer)
                return;

            _texture = texture as Texture2D;
            InstanceComponent.Renderer.material.SetTexture(ShaderTextureId, _texture);
        }

        private void RestoreMaterial()
        {
            if (InstanceComponent.Renderer && OriginalMaterial)
                InstanceComponent.Renderer.material = OriginalMaterial;
        }

        private async void InvokeInstanceEvent(Action action)
        {
            await Awaitable.MainThreadAsync();
            action?.Invoke();
        }

        private async Awaitable<(ILogProcessor, IGraphicsProcessor, IAudioProcessor, IInputProcessor, ILedProcessor)> GetProcessors()
        {
            await Awaitable.MainThreadAsync();

            ILogProcessor log           = GetLogProcessor();
            IGraphicsProcessor graphics = GetGraphicsProcessor();
            IAudioProcessor audio       = GetAudioProcessor(InstanceComponent.transform);
            IInputProcessor input       = GetInputProcessor(InstanceComponent.Settings.LeftStickBehaviour);
            ILedProcessor led           = GetLedProcessor();
            return (log, graphics, audio, input, led);
        }

        private static ILogProcessor GetLogProcessor() => new LogProcessor();

        private IGraphicsProcessor GetGraphicsProcessor() => new GraphicsProcessor(SetTexture, FilterMode.Point);

        private static IAudioProcessor GetAudioProcessor(Transform instanceTransform)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            AudioProcessor unityAudio = instanceTransform.GetComponentInChildren<AudioProcessor>(false);
            return unityAudio && unityAudio.enabled ? unityAudio : new AudioProcessorSDL();
#else
            AudioProcessor unityAudio = instanceTransform.GetComponentInChildren<AudioProcessor>(true);
            if (unityAudio)
            {
                unityAudio.gameObject.SetActive(true);
                unityAudio.enabled = true;
            }
            else
            {
                GameObject audioProcessorGameObject = new("LibretroAudioProcessor");
                audioProcessorGameObject.transform.SetParent(instanceTransform);
                unityAudio = audioProcessorGameObject.AddComponent<AudioProcessor>();
            }
            return unityAudio;
#endif
        }

        private static IInputProcessor GetInputProcessor(LeftStickBehaviour leftStickBehaviour)
        {
            InputProcessor inputProcessor = Object.FindFirstObjectByType<InputProcessor>();
            if (!inputProcessor)
                inputProcessor = Object.Instantiate(Resources.Load<InputProcessor>("pfLibretroInputProcessor"));
            inputProcessor.LeftStickBehaviour = leftStickBehaviour;
            return inputProcessor;
        }

        private static ILedProcessor GetLedProcessor() => Object.FindFirstObjectByType<LedProcessorBase>(FindObjectsInactive.Exclude);

        private async void TakeScreenshot(string screenshotPath)
        {
            await Awaitable.MainThreadAsync();

            if (!_texture || !Running)
                return;

            await Awaitable.EndOfFrameAsync();

            Texture2D tex = new(_texture.width, _texture.height, TextureFormat.RGB24, false, false, true);
            tex.SetPixels32(_texture.GetPixels32());
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            Object.Destroy(tex);

            screenshotPath = screenshotPath.Replace(".state", ".png");
            File.WriteAllBytes(screenshotPath, bytes);
        }

        private async void StopThread()
        {
            await Awaitable.MainThreadAsync();

            if (_thread is not null)
                if (!_thread.Join(2000))
                {
                    //System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            _thread = null;
            RestoreMaterial();
        }

        private static string GetAndroidPrivateAppDataPath()
        {
            using AndroidJavaClass unityPlayerClass = new("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject getFilesDir     = currentActivity.Call<AndroidJavaObject>("getFilesDir");
            return getFilesDir.Call<string>("getCanonicalPath");
        }
    }
}
