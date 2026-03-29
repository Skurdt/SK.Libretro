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
                    _bridgeCommands.Enqueue(new EnableInputBridgeCommand(_wrapper, value));
                }
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

        private readonly object _lock = new();

        private readonly string _mainDirectory;
        private readonly string _tempDirectory;
        private readonly ManualResetEventSlim _manualResetEvent;
        private readonly ConcurrentQueue<IBridgeCommand> _bridgeCommands;

        private LibretroInstance _instanceComponent;
        private int _shaderTextureId;
        private Material _originalMaterial;

        private string _coreName;
        private string _gamesDirectory;
        private string[] _gameNames;
        private Action _instanceStartedCallback;
        private Action _instanceStoppedCallback;

        private Wrapper _wrapper;
        private bool _running;
        private bool _paused;
        private int _fastForwardFactor;
        private bool _fastForward;
        private bool _rewind;
        private bool _inputEnabled;
        private (Options, Options) _options;
        private bool _diskHandlerEnabled;

        private Thread _thread;
        private Texture2D _texture;

        public Bridge()
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

        public void StartContent(LibretroInstance instanceComponent)
        {
            if (string.IsNullOrWhiteSpace(instanceComponent.CoreName))
            {
                Debug.LogError("Core is not set");
                return;
            }

            StopContent();

            _instanceComponent = instanceComponent;
            _shaderTextureId   = Shader.PropertyToID(_instanceComponent.Settings.ShaderTextureName);
            _originalMaterial  = instanceComponent.Renderer ? new(instanceComponent.Renderer.material) : null;

            _coreName                = instanceComponent.CoreName;
            _gamesDirectory          = instanceComponent.GamesDirectory;
            _gameNames               = instanceComponent.GameNames;
            _instanceStartedCallback = instanceComponent.OnInstanceStarted;
            _instanceStoppedCallback = instanceComponent.OnInstanceStopped;

            _thread = new(LibretroThread)
            {
                Name         = $"LibretroThread_{_coreName}_{(_gameNames.Length > 0 ? _gameNames[0] : "nogame")}",
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
                var platform = Application.platform switch
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

                (var logProcessor, var graphicsProcessor, var audioProcessor, var inputProcessor, var ledProcessor) = await GetProcessors();

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

                _wrapper = new(wrapperSettings, _coreName, _gamesDirectory, _gameNames);

                if (!_wrapper.StartContent())
                {
                    Debug.LogError("Failed to start core/game combination");
                    return;
                }

                _wrapper.InitGraphics();
                _wrapper.InitAudio();
                _wrapper.InputHandler.Enabled = true;
                DiskHandlerEnabled = _wrapper.DiskHandler.Enabled;
                ControllersMap     = _wrapper.InputHandler.DeviceMap;

                //__wrapper.RewindEnabled = _settings.RewindEnabled;

                Options = (_wrapper.OptionsHandler.CoreOptions, _wrapper.OptionsHandler.GameOptions);
                
                InvokeInstanceEvent(_instanceStartedCallback);

                var frameDuration = 1000.0 / _wrapper.Game.SystemAVInfo.Fps;
                var lastTime      = System.Diagnostics.Stopwatch.GetTimestamp();
                var accumulator   = 0.0;

                var frameStatsEnabled = true;
                var runTimed          = true;
                var frameTime         = 0.0;
                var lastRetroRunTime  = 0L;
                var fps = 0.0;

                Running = true;
                while (Running)
                {
                    if (Paused)
                        _manualResetEvent.Wait();

                    if (!Running)
                        break;

                    //__wrapper.RewindEnabled = _settings.RewindEnabled;

                    //if (_settings.RewindEnabled)
                    //    __wrapper.PerformRewind = Rewind;

                    lock (_lock)
                        while (_bridgeCommands.TryDequeue(out var command))
                            command.Execute();

                    var currentTime = System.Diagnostics.Stopwatch.GetTimestamp();
                    var elapsedTime = (currentTime - lastTime) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                    lastTime = currentTime;
                    accumulator += elapsedTime;

                    while (accumulator >= frameDuration)
                    {
                        if (frameStatsEnabled)
                        {
                            var runCurrentTime = System.Diagnostics.Stopwatch.GetTimestamp();
                            if (runTimed)
                            {
                                var dtMs = (runCurrentTime - lastRetroRunTime) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                                var kAlpha = 0.1;
                                var smoothed = (kAlpha * dtMs) + ((1.0 - kAlpha) * frameTime);

                                frameTime = smoothed;
                                fps = smoothed > 0.0 ? 1000.0 / smoothed : 0.0;
                            }
                            lastRetroRunTime = runCurrentTime;
                            runTimed = true;
                        }
                        else if (runTimed)
                        {
                            fps = 0.0;
                            frameTime = 0.0;
                            runTimed = false;
                        }

                        _wrapper.RunFrame();

                        accumulator -= frameDuration;

                        if (accumulator < frameDuration)
                        {
                            var sleepTimeMs = frameDuration - accumulator;
                            Thread.Sleep((int)(sleepTimeMs * 0.9));
                        }
                    }
                }

                InvokeInstanceEvent(_instanceStoppedCallback);

                lock (_lock)
                    _wrapper.StopContent();

                StopThread();

                _wrapper = null;
            }
            catch (Exception e) when (e is not ThreadAbortException)
            {
                Debug.LogException(e);
            }
        }

        public ReadOnlySpan<byte> GetSaveMemory()
        {
            lock (_lock)
                return _wrapper.MemoryHandler.GetSaveMemory();
        }

        public ReadOnlySpan<byte> GetRtcMemory()
        {
            lock (_lock)
                return _wrapper.MemoryHandler.GetRtcMemory();
        }

        public ReadOnlySpan<byte> GetSystemMemory()
        {
            lock (_lock)
                return _wrapper.MemoryHandler.GetSystemMemory();
        }

        public ReadOnlySpan<byte> GetVideoMemory()
        {
            lock (_lock)
                return _wrapper.MemoryHandler.GetVideoMemory();
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

        public void SetStateSlot(int slot)
            => _bridgeCommands.Enqueue(new SetStateSlotBridgeCommand(_wrapper, slot));

        public void SaveStateWithScreenshot()
            => _bridgeCommands.Enqueue(new SaveStateWithScreenshotBridgeCommand(_wrapper, TakeScreenshot));

        public void SaveStateWithoutScreenshot()
            => _bridgeCommands.Enqueue(new SaveStateWithoutScreenshotBridgeCommand(_wrapper));

        public void LoadState()
            => _bridgeCommands.Enqueue(new LoadStateBridgeCommand(_wrapper));

        public void SetDiskIndex(int index)
            => _bridgeCommands.Enqueue(new SetDiskIndexBridgeCommand(_wrapper, _gamesDirectory, _gameNames, index));

        public void SaveSRAM()
            => _bridgeCommands.Enqueue(new SaveSRAMBridgeCommand(_wrapper));

        public void LoadSRAM()
            => _bridgeCommands.Enqueue(new LoadSRAMBridgeCommand(_wrapper));

        public void SaveOptions(bool global)
            => _bridgeCommands.Enqueue(new SaveOptionsBridgeCommand(_wrapper, global));

        public void SetControllerPortDevice(uint port, uint device)
            => _bridgeCommands.Enqueue(new SetControllerPortDeviceBridgeCommand(_wrapper, port, device));

        public void SetPlayerPosition(float x, float y, float z, float distance, float forwardX, float forwardZ)
            => _bridgeCommands.Enqueue(new SetPlayerPositionBridgeCommand(_wrapper, x, y ,z, distance, forwardX, forwardZ));

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

        private async void InvokeInstanceEvent(Action action)
        {
            await Awaitable.MainThreadAsync();
            action?.Invoke();
        }

        private async Awaitable<(ILogProcessor, IGraphicsProcessor, IAudioProcessor, IInputProcessor, ILedProcessor)> GetProcessors()
        {
            await Awaitable.MainThreadAsync();

            var log           = GetLogProcessor();
            var graphics = GetGraphicsProcessor();
            var audio       = GetAudioProcessor(_instanceComponent.transform);
            var input       = GetInputProcessor(_instanceComponent.Settings.LeftStickBehaviour);
            var led           = GetLedProcessor();
            return (log, graphics, audio, input, led);
        }

        private static ILogProcessor GetLogProcessor() => new LogProcessor();

        private IGraphicsProcessor GetGraphicsProcessor() => new GraphicsProcessor(SetTexture, FilterMode.Point);

        private static IAudioProcessor GetAudioProcessor(Transform instanceTransform)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var unityAudio = instanceTransform.GetComponentInChildren<AudioProcessor>(false);
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
            var inputProcessor = Object.FindAnyObjectByType<InputProcessor>();
            if (!inputProcessor)
                inputProcessor = Object.Instantiate(Resources.Load<InputProcessor>("pfLibretroInputProcessor"));
            inputProcessor.LeftStickBehaviour = leftStickBehaviour;
            return inputProcessor;
        }

        private static ILedProcessor GetLedProcessor() => Object.FindAnyObjectByType<LedProcessorBase>(FindObjectsInactive.Exclude);

        private async void TakeScreenshot(string screenshotPath)
        {
            await Awaitable.MainThreadAsync();

            if (!_texture || !Running)
                return;

            await Awaitable.EndOfFrameAsync();

            Texture2D tex = new(_texture.width, _texture.height, TextureFormat.RGB24, false, false, true);
            tex.SetPixels32(_texture.GetPixels32());
            tex.Apply();
            var bytes = tex.EncodeToPNG();
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
            using var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            using var getFilesDir     = currentActivity.Call<AndroidJavaObject>("getFilesDir");
            return getFilesDir.Call<string>("getCanonicalPath");
        }
    }
}
