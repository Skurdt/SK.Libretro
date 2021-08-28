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
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: InternalsVisibleTo("SK.Libretro.Unity.Editor")]

namespace SK.Libretro.Unity
{
    internal sealed class Bridge : IDisposable
    {
        private enum ThreadCommandType
        {
            SaveStateWithScreenshot,
            SaveStateWithoutScreenshot,
            LoadState,
            SaveSRAM,
            LoadSRAM,
            EnableInput,
            DisableInput,
            SetDiskIndex,
            SetControllerPortDevice
        }

        private struct ThreadCommand
        {
            public ThreadCommandType Type;
            public object Param0;
            public object Param1;
        }

        public event Action OnInstanceStarted;
        public event Action OnInstanceStopped;

        public bool AllowGLCoresInEditor
        {
            get
            {
                lock (_lock)
                    return _allowGLCoresInEditor;
            }
            set
            {
                lock (_lock)
                    _allowGLCoresInEditor = value;
            }
        }
        private volatile bool _allowGLCoresInEditor;

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
        private volatile bool _running;

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
        private volatile bool _paused;

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
        private int _fastForwardFactor = 8;

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
        private volatile bool _fastForward;

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
        private volatile bool _rewind;

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
                    _inputEnabled = value;

                if (InputEnabled)
                    _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.EnableInput });
                else
                    _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.DisableInput });
            }
        }
        private volatile bool _inputEnabled;

        public ControllersMap ControllersMap { get; private set; }

        private static bool _firstInstance = true;

        private readonly LibretroInstance _instanceComponent;
        private readonly Renderer _screen;
        private readonly Transform _viewer;
        private readonly BridgeSettings _settings;

        private readonly object _lock                                   = new object();
        private readonly ManualResetEventSlim _manualResetEvent         = new ManualResetEventSlim(false);
        private readonly ConcurrentQueue<ThreadCommand> _threadCommands = new ConcurrentQueue<ThreadCommand>();

        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();

        private readonly MaterialPropertyBlock _materialPropertyBlock;
        private readonly int _shaderTextureId;

        private string _coreName;
        private string _gamesDirectory;
        private string[] _gameNames;

        private Thread _thread;
        private Exception _threadException;
        private IInputProcessor _inputProcessor;
        private IAudioProcessor _audioProcessor;
        private Texture2D _texture;
        private double _startTime;
        private double _accumulator;
        private Coroutine _screenshotCoroutine;

        private volatile int _currentStateSlot;

        public Bridge(LibretroInstance instance)
        {
            if (_firstInstance)
            {
                MainThreadDispatcher mainThreadDispatcher = UnityEngine.Object.FindObjectOfType<MainThreadDispatcher>();
                if (mainThreadDispatcher == null)
                    _ = new GameObject("MainThreadDispatcher", typeof(MainThreadDispatcher));

                Logger.Instance.AddDebughandler(Debug.Log, true);
                Logger.Instance.AddInfoHandler(Debug.Log, true);
                Logger.Instance.AddWarningHandler(Debug.LogWarning, true);
                Logger.Instance.AddErrorhandler(Debug.LogError, true);
                Logger.Instance.AddExceptionHandler(Debug.LogException);

                _firstInstance = false;
            }

            _instanceComponent = instance;
            _screen            = instance.Renderer;
            _viewer            = instance.Viewer;
            _settings          = instance.Settings;

            _materialPropertyBlock = new MaterialPropertyBlock();

            _shaderTextureId = Shader.PropertyToID(_settings.ShaderTextureName);
        }

        public void Dispose()
        {
            StopContent();
            _ = _thread?.Join(1000);

            _thread = null;
            _manualResetEvent.Dispose();
        }

        public void SetContent(string coreName, string gamesDirectory, string[] gameNames)
        {
            if (Running)
                return;

            _coreName       = coreName;
            _gamesDirectory = gamesDirectory;
            _gameNames      = gameNames;
        }

        public void StartContent(Action instanceStartedCallback, Action instanceStoppedCallback)
        {
            if (Running)
                return;

            if (string.IsNullOrEmpty(_coreName))
            {
                Debug.LogError("Core is not set");
                return;
            }

            PlayerInputManager playerInputManager = UnityEngine.Object.FindObjectOfType<PlayerInputManager>();
            if (playerInputManager == null)
            {
                GameObject processorGameObject = new GameObject("LibretroInputProcessor");
                playerInputManager = processorGameObject.AddComponent<PlayerInputManager>();
                playerInputManager.EnableJoining();
                playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
                playerInputManager.playerPrefab = Resources.Load<GameObject>("pfLibretroUserInput");
            }

            _inputProcessor = playerInputManager.GetComponent<IInputProcessor>();
            if (_inputProcessor == null)
                _inputProcessor = playerInputManager.gameObject.AddComponent<InputProcessor>();

            _inputProcessor.AnalogToDigital = _settings.AnalogToDigital;

            GameObject go = _screen.gameObject;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            AudioProcessor unityAudio = go != null ? go.GetComponentInChildren<AudioProcessor>() : null;
            _audioProcessor = unityAudio != null ? unityAudio : (IAudioProcessor)new NAudio.AudioProcessor();
#else
            AudioProcessor unityAudio = go != null ? go.GetComponentInChildren<AudioProcessor>(true) : null;
            if (unityAudio != null)
            {
                unityAudio.gameObject.SetActive(true);
                _audioProcessor = unityAudio;
            }
            else
            {
                GameObject audioProcessorGameObject = new GameObject("LibretroAudioProcessor");
                audioProcessorGameObject.transform.SetParent(go.transform);
                _audioProcessor = audioProcessorGameObject.AddComponent<AudioProcessor>();
            }
#endif
            if (!Directory.Exists(_settings.MainDirectory))
                _settings.MainDirectory = BridgeSettings.DefaultMainDirectory;

            OnInstanceStarted += instanceStartedCallback;
            OnInstanceStopped += instanceStoppedCallback;

            _thread = new Thread(LibretroThread)
            {
                Name         = $"LibretroThread_{_coreName}_{(_gameNames.Length > 0 ? _gameNames[0] : "nogame")}",
                IsBackground = true,
                Priority     = System.Threading.ThreadPriority.Lowest
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
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

        public void StopContent()
        {
            if (!Running)
                return;

            if (Paused)
                ResumeContent();

            Running = false;

            OnInstanceStopped?.Invoke();
        }

        public void SetStateSlot(string slot)
        {
            lock (_lock)
                if (int.TryParse(slot, out int slotInt))
                    _currentStateSlot = slotInt;
        }

        public void SetStateSlot(int slot)
        {
            lock(_lock)
                _currentStateSlot = slot;
        }

        public void SaveStateWithScreenshot() => _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SaveStateWithScreenshot });

        public void SaveStateWithoutScreenshot() => _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SaveStateWithoutScreenshot });

        public void LoadState() => _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.LoadState });

        public void SaveSRAM() => _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SaveSRAM });

        public void LoadSRAM() => _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.LoadSRAM });

        public void SetDiskIndex(int index) => _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SetDiskIndex, Param0 = index });

        public void TakeScreenshot(string screenshotPath) => MainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (_screenshotCoroutine == null && _texture != null && Running)
                _screenshotCoroutine = _instanceComponent.StartCoroutine(CoSaveScreenshot(screenshotPath));
        });

        public void SetControllerPortDevice(uint port, uint id) => _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SetControllerPortDevice, Param0 = port, Param1 = id });

        private void SetTexture(Texture texture)
        {
            if (texture == null)
                return;

            _texture = texture as Texture2D;
            if (_screen != null)
            {
                _screen.GetPropertyBlock(_materialPropertyBlock, 0);
                _materialPropertyBlock.SetTexture(_shaderTextureId, _texture);
                _screen.SetPropertyBlock(_materialPropertyBlock, 0);
            }
        }

        private void LibretroThread()
        {
            try
            {
                Wrapper wrapper = new Wrapper((TargetPlatform)Application.platform, _settings.MainDirectory);
                if (!wrapper.StartContent(_coreName, _gamesDirectory, _gameNames?[0]))
                {
                    Debug.LogError("Failed to start core/game combination");
                    return;
                }

                if (_gameNames != null)
                    foreach (string gameName in _gameNames)
                        _ = wrapper.Disk?.AddImageIndex();

                if (wrapper.Core.HwAccelerated)
                {
                    // Running gl cores only works in builds, or if a debugger is attached to Unity instance. Set "_allowGLCoresInEditor" to true to bypass this.
                    if (Application.isEditor && !AllowGLCoresInEditor)
                    {
                        wrapper.StopContent();
                        Debug.LogError("Starting hardware accelerated cores is not supported in the editor");
                        return;
                    }

                    wrapper.InitHardwareContext();
                }

                IGraphicsProcessor graphicsProcessor = new GraphicsProcessor(wrapper.Game.VideoWidth, wrapper.Game.VideoHeight, SetTexture);
                wrapper.Graphics.Init(graphicsProcessor);
                wrapper.Graphics.Enabled = true;

                wrapper.Audio.Init(_audioProcessor);
                wrapper.Audio.Enabled = true;

                wrapper.Input.Init(_inputProcessor);
                wrapper.Input.Enabled = true;

                ControllersMap = wrapper.Input.DeviceMap;

                wrapper.RewindEnabled = _settings.RewindEnabled;

                double gameFrameTime = 1.0 / wrapper.Game.VideoFps;

                _manualResetEvent.Reset();
                MainThreadDispatcher mainThreadDispatcher = MainThreadDispatcher.Instance;
                if (mainThreadDispatcher != null)
                    mainThreadDispatcher.Enqueue(() =>
                    {
                        OnInstanceStarted?.Invoke();
                        _manualResetEvent.Set();
                    });
                _manualResetEvent.Wait();

                Running = true;
                while (Running)
                {
                    lock (_lock)
                    {
                        while (_threadCommands.Count > 0)
                        {
                            if (_threadCommands.TryDequeue(out ThreadCommand command))
                            {
                                lock (_lock)
                                {
                                    switch (command.Type)
                                    {
                                        case ThreadCommandType.SaveStateWithScreenshot:
                                            if (wrapper.Serialization.SaveState(_currentStateSlot, out string screenshotPath))
                                                TakeScreenshot(screenshotPath);
                                            break;
                                        case ThreadCommandType.SaveStateWithoutScreenshot:
                                            _ = wrapper.Serialization.SaveState(_currentStateSlot);
                                            break;
                                        case ThreadCommandType.LoadState:
                                            _ = wrapper.Serialization.LoadState(_currentStateSlot);
                                            break;
                                        case ThreadCommandType.SaveSRAM:
                                            _ = wrapper.Serialization.SaveSRAM();
                                            break;
                                        case ThreadCommandType.LoadSRAM:
                                            _ = wrapper.Serialization.LoadSRAM();
                                            break;
                                        case ThreadCommandType.EnableInput:
                                            wrapper.Input.Enabled = true;
                                            break;
                                        case ThreadCommandType.DisableInput:
                                            wrapper.Input.Enabled = false;
                                            break;
                                        case ThreadCommandType.SetDiskIndex:
                                            int index = (int)command.Param0;
                                            if (_gameNames.Length > index)
                                                _ = wrapper.Disk?.SetImageIndexAuto((uint)index, $"{_gamesDirectory}/{_gameNames[index]}");
                                            break;
                                        case ThreadCommandType.SetControllerPortDevice:
                                            wrapper.Core.retro_set_controller_port_device((uint)command.Param0, (uint)command.Param1);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    if (Paused)
                        _manualResetEvent.Wait();
                    else if (!Running)
                        break;

                    wrapper.RewindEnabled = _settings.RewindEnabled;

                    if (_settings.RewindEnabled)
                        wrapper.PerformRewind = Rewind;

                    double currentTime = _stopwatch.Elapsed.TotalSeconds;
                    double dt          = currentTime - _startTime;
                    _startTime         = currentTime;

                    double targetFrameTime = FastForward && FastForwardFactor > 0 ? gameFrameTime / FastForwardFactor : gameFrameTime;
                    if ((_accumulator += dt) >= targetFrameTime)
                    {
                        wrapper.RunFrame();
                        _accumulator = 0.0;
                    }
                }

                wrapper.StopContent();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _threadException = e;
            }
        }

        private IEnumerator CoSaveScreenshot(string screenshotPath)
        {
            yield return new WaitForEndOfFrame();

            Texture2D tex = new Texture2D(_texture.width, _texture.height, TextureFormat.RGB24, false);
            tex.SetPixels32(_texture.GetPixels32());
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.Destroy(tex);

            screenshotPath = screenshotPath.Replace(".state", ".png");
            File.WriteAllBytes(screenshotPath, bytes);

            _screenshotCoroutine = null;
        }

        //  public void SetAnalogToDigitalInput(bool value)
        //  {
        //      DeactivateInput();
        //      _settings.AnalogDirectionsToDigital = value;
        //      ActivateInput();
        //  }

        //  private void UpdateTimings()
        //  {
        //      _wrapper.FrameTimeUpdate();

        //      _targetFrameTime = 1.0 / _wrapper.Game.VideoFps / _settings.TimeScale;
        //      _accumulatedTime += _stopwatch.Elapsed.TotalSeconds;
        //      _nLoops = 0;
        //      _stopwatch.Restart();
        //  }

        //  private void UpdateDistanceBasedAudio()
        //  {
        //#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        //      if (_wrapper == null || _viewer == null || _screenTransform == null || !_settings.AudioVolumeControlledByDistance || !(_wrapper.Audio.Processor is NAudio.AudioProcessor audioProcessor))
        //          return;
        //      float distance = Vector3.Distance(_screenTransform.position, _viewer.transform.position);
        //      if (distance > 0f)
        //      {
        //          float volume = math.clamp(math.pow((distance - _settings.AudioMaxDistance) / (_settings.AudioMinDistance - _settings.AudioMaxDistance), 2f), 0f, _settings.AudioMaxVolume);
        //          audioProcessor.SetVolume(volume);
        //      }
        //#endif
        //  }
    }
}
