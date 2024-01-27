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

using Cysharp.Threading.Tasks;
using SK.Libretro.Header;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace SK.Libretro.Unity
{
    internal sealed class Bridge
    {
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

        private string LibretroDirectory
        {
            get
            {
                lock (_lock)
                    return _libretroDirectory;
            }

            set
            {
                lock (_lock)
                    _libretroDirectory = value;
            }
        }

        private string TempDirectory
        {
            get
            {
                lock (_lock)
                    return _tempDirectory;
            }

            set
            {
                lock (_lock)
                    _tempDirectory = value;
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

        private readonly LibretroInstance _instanceComponent;
        private readonly Material _originalMaterial;
        private readonly int _shaderTextureId;
        private readonly ManualResetEventSlim _manualResetEvent;
        private readonly ConcurrentQueue<IBridgeCommand> _bridgeCommands;
        private readonly object _lock;

        private string _libretroDirectory;
        private string _tempDirectory;
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

        public Bridge(LibretroInstance instance)
        {
            _instanceComponent = instance;
            _originalMaterial  = instance.Renderer ? new(instance.Renderer.material) : null;
            _shaderTextureId   = Shader.PropertyToID(_instanceComponent.Settings.ShaderTextureName);
            _manualResetEvent  = new(false);
            _bridgeCommands    = new();
            _lock              = new();
            _fastForwardFactor = DEFAULT_FASTFORWARD_FACTOR;

            ReadSaveMemory   = instance.Settings.ReadSaveMemory;
            ReadRtcMemory    = instance.Settings.ReadRtcMemory;
            ReadSystemMemory = instance.Settings.ReadSystemMemory;
            ReadVideoMemory  = instance.Settings.ReadVideoMemory;

            MainThreadDispatcher.Construct();
        }

        public void StartContent(string libretroDirectory,
                                 string tempDirectory,
                                 string coreName,
                                 string gamesDirectory,
                                 string[] gameNames,
                                 Action instanceStartedCallback,
                                 Action instanceStoppedCallback)
        {
            if (Running)
                return;

            if (string.IsNullOrWhiteSpace(libretroDirectory))
            {
                Debug.LogError("Libretro Directory is not set");
                return;
            }

            if (string.IsNullOrWhiteSpace(tempDirectory))
            {
                Debug.LogError("Core Directory is not set");
                return;
            }

            if (string.IsNullOrWhiteSpace(coreName))
            {
                Debug.LogError("Core is not set");
                return;
            }

            TempDirectory           = tempDirectory;
            CoreName                 = coreName;
            GamesDirectory           = gamesDirectory;
            GameNames                = gameNames;
            LibretroDirectory        = libretroDirectory;
            _instanceStartedCallback = instanceStartedCallback;
            _instanceStoppedCallback = instanceStoppedCallback;

            _thread = new Thread(LibretroThread)
            {
                Name = $"LibretroThread_{CoreName}_{(GameNames.Length > 0 ? GameNames[0] : "nogame")}",
                IsBackground = true,
                Priority = System.Threading.ThreadPriority.Lowest
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        private void LibretroThread()
        {
            try
            {
                Platform platform = Application.platform switch
                {
                    UnityEngine.RuntimePlatform.OSXEditor
                    or UnityEngine.RuntimePlatform.OSXPlayer => Platform.OSX,
                    UnityEngine.RuntimePlatform.WindowsPlayer
                    or UnityEngine.RuntimePlatform.WindowsEditor => Platform.Win,
                    UnityEngine.RuntimePlatform.IPhonePlayer => Platform.IOS,
                    UnityEngine.RuntimePlatform.Android => Platform.Android,
                    UnityEngine.RuntimePlatform.LinuxPlayer
                    or UnityEngine.RuntimePlatform.LinuxEditor => Platform.Linux,
                    _ => Platform.None
                };

                (ILogProcessor logProcessor, IGraphicsProcessor graphicsProcessor, IAudioProcessor audioProcessor, IInputProcessor inputProcessor, ILedProcessor ledProcessor) = GetProcessors();

                WrapperSettings wrapperSettings = new(platform)
                {
                    LogLevel          = LogLevel.Warning,
                    MainDirectory     = LibretroDirectory,
                    TempDirectory    = TempDirectory,
                    LogProcessor      = logProcessor,
                    GraphicsProcessor = graphicsProcessor,
                    AudioProcessor    = audioProcessor,
                    InputProcessor    = inputProcessor,
                    LedProcessor      = ledProcessor
                };

                Wrapper wrapper = new(wrapperSettings);
                if (!wrapper.StartContent(CoreName, GamesDirectory, GameNames))
                {
                    Debug.LogError("Failed to start core/game combination");
                    return;
                }

                wrapper.InitGraphics();
                wrapper.InitAudio();
                wrapper.InputHandler.Enabled = InputEnabled;
                DiskHandlerEnabled = wrapper.DiskHandler.Enabled;
                ControllersMap     = wrapper.InputHandler.DeviceMap;

                //_wrapper.RewindEnabled = _settings.RewindEnabled;

                Options = (wrapper.OptionsHandler.CoreOptions, wrapper.OptionsHandler.GameOptions);

                InvokeInstanceEvent(_instanceStartedCallback);

                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                double gameFrameTime = 1.0 / wrapper.Game.SystemAVInfo.Fps;
                double startTime     = 0.0;
                double accumulator   = 0.0;

                Running = true;
                while (Running)
                {
                    lock (_lock)
                        while (_bridgeCommands.Count > 0)
                            if (_bridgeCommands.TryDequeue(out IBridgeCommand command))
                                command.Execute(wrapper);

                    if (Paused)
                        _manualResetEvent.Wait();

                    if (!Running)
                        break;

                    //_wrapper.RewindEnabled = _settings.RewindEnabled;

                    //if (_settings.RewindEnabled)
                    //    _wrapper.PerformRewind = Rewind;

                    double currentTime = stopwatch.Elapsed.TotalSeconds;
                    double dt = currentTime - startTime;
                    startTime = currentTime;

                    double targetFrameTime = /*FastForward && FastForwardFactor > 0 ? gameFrameTime / FastForwardFactor : */gameFrameTime;
                    if ((accumulator += dt) >= targetFrameTime)
                    {
                        wrapper.RunFrame();
                        if (ReadSaveMemory)
                            SaveMemory = wrapper.MemoryHandler.GetSaveMemory().ToArray();
                        if (ReadRtcMemory)
                            RtcMemory = wrapper.MemoryHandler.GetRtcMemory().ToArray();
                        if (ReadSystemMemory)
                            SystemMemory = wrapper.MemoryHandler.GetSystemMemory().ToArray();
                        if (ReadVideoMemory)
                            VideoMemory = wrapper.MemoryHandler.GetVideoMemory().ToArray();
                        accumulator = 0.0;
                    }
                }

                InvokeInstanceEvent(_instanceStoppedCallback);

                lock (_lock)
                    wrapper.StopContent();

                StopThread();
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
            if (!Application.isPlaying || !texture || !_instanceComponent.Renderer)
                return;

            _texture = texture as Texture2D;
            _instanceComponent.Renderer.material.SetTexture(_shaderTextureId, _texture);
        }

        private void RestoreMaterial()
        {
            if (_instanceComponent.Renderer && _originalMaterial)
                _instanceComponent.Renderer.material = _originalMaterial;
        }

        private void InvokeInstanceEvent(Action action)
        {
            if (action is null)
                return;

            _manualResetEvent.Reset();
            using CancellationTokenSource tokenSource = new();
            MainThreadDispatcher.Enqueue(() =>
            {
                action();
                _manualResetEvent.Set();
                tokenSource.Cancel();
            });
            _manualResetEvent.Wait(tokenSource.Token);
        }

        private (ILogProcessor, IGraphicsProcessor, IAudioProcessor, IInputProcessor, ILedProcessor) GetProcessors()
        {
            ILogProcessor log           = GetLogProcessor();
            IGraphicsProcessor graphics = GetGraphicsProcessor();
            IAudioProcessor audio       = default;
            IInputProcessor input       = default;
            ILedProcessor led           = default;

            using CancellationTokenSource getComponentsTokenSource = new();
            _manualResetEvent.Reset();
            MainThreadDispatcher.Enqueue(() =>
            {
                audio = GetAudioProcessor(_instanceComponent.transform);
                input = GetInputProcessor(_instanceComponent.Settings.AnalogToDigital);
                led   = GetLedProcessor();
                _manualResetEvent.Set();
                getComponentsTokenSource.Cancel();
            });
            _manualResetEvent.Wait(getComponentsTokenSource.Token);
            return (log, graphics, audio, input, led);
        }

        private static ILogProcessor GetLogProcessor() => new LogProcessor();

        private IGraphicsProcessor GetGraphicsProcessor() => new GraphicsProcessor(SetTexture, FilterMode.Point);

        private static IAudioProcessor GetAudioProcessor(Transform instanceTransform)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            AudioProcessor unityAudio = instanceTransform.GetComponentInChildren<AudioProcessor>();
            return unityAudio && unityAudio.enabled ? unityAudio : new NAudio.AudioProcessor();
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

        private static IInputProcessor GetInputProcessor(bool analogToDigital)
        {
            PlayerInputManager playerInputManager = Object.FindObjectOfType<PlayerInputManager>();
            if (!playerInputManager)
            {
                GameObject processorGameObject = new("LibretroInputProcessor");
                playerInputManager = processorGameObject.AddComponent<PlayerInputManager>();
                playerInputManager.EnableJoining();
                playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
                playerInputManager.playerPrefab = Resources.Load<GameObject>("pfLibretroUserInput");
            }

            if (!playerInputManager.TryGetComponent(out IInputProcessor inputProcessor))
                inputProcessor = playerInputManager.gameObject.AddComponent<InputProcessor>();

            inputProcessor.AnalogToDigital = analogToDigital;
            return inputProcessor;
        }

        private static ILedProcessor GetLedProcessor() => Object.FindObjectOfType<LedProcessorBase>(true);

        private void TakeScreenshot(string screenshotPath) => MainThreadDispatcher.Enqueue(async () =>
        {
            if (!_texture || !Running)
                return;

            await UniTask.WaitForEndOfFrame(_instanceComponent);

            Texture2D tex = new(_texture.width, _texture.height, TextureFormat.RGB24, false, false, true);
            tex.SetPixels32(_texture.GetPixels32());
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            Object.Destroy(tex);

            screenshotPath = screenshotPath.Replace(".state", ".png");
            File.WriteAllBytes(screenshotPath, bytes);
        });

        private void StopThread() => MainThreadDispatcher.Enqueue(() =>
        {
            if (_thread is not null)
                if (!_thread.Join(2000))
                {
                    //System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            _thread = null;
            _manualResetEvent.Dispose();
            RestoreMaterial();
        });
    }
}
