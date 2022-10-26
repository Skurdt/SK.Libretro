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

        public ControllersMap ControllersMap { get; private set; }

        private const int DEFAULT_FASTFORWARD_FACTOR = 8;

        private readonly LibretroInstance _instanceComponent;
        private readonly Material _originalMaterial;
        private readonly int _shaderTextureId;
        private readonly ManualResetEventSlim _manualResetEvent;
        private readonly ConcurrentQueue<IBridgeCommand> _bridgeCommands;
        private readonly object _lock;

        private string _coreName;
        private string _gamesDirectory;
        private string[] _gameNames;

        private Wrapper _wrapper;

        private ILogProcessor _logProcessor;
        private IGraphicsProcessor _graphicsProcessor;
        private IAudioProcessor _audioProcessor;
        private IInputProcessor _inputProcessor;

        private bool _running;
        private bool _paused;
        private int _fastForwardFactor;
        private bool _fastForward;
        private bool _rewind;
        private bool _inputEnabled;

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
        }

        public void Dispose() => _manualResetEvent.Dispose();

        public async UniTask StartContent(string coreName,
                                          string gamesDirectory,
                                          string[] gameNames,
                                          Action instanceStartedCallback,
                                          Action instanceStoppedCallback,
                                          CancellationToken cancellationToken)
        {
            if (Running)
                return;

            if (string.IsNullOrWhiteSpace(coreName))
            {
                Debug.LogError("Core is not set");
                return;
            }

            CoreName       = coreName;
            GamesDirectory = gamesDirectory;
            GameNames      = gameNames;

            _logProcessor      = GetLogProcessor();
            _graphicsProcessor = GetGraphicsProcessor(cancellationToken);

            await UniTask.SwitchToMainThread(cancellationToken);
            _inputProcessor = GetInputProcessor(_instanceComponent.Settings.AnalogToDigital);
            _audioProcessor = GetAudioProcessor(_instanceComponent.transform, cancellationToken);
            await UniTask.SwitchToThreadPool();

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

            WrapperSettings wrapperSettings = new(platform)
            {
                MainDirectory     = $"{Application.streamingAssetsPath}/libretro~",
                LogProcessor      = _logProcessor,
                GraphicsProcessor = _graphicsProcessor,
                AudioProcessor    = _audioProcessor,
                InputProcessor    = _inputProcessor
            };

            _wrapper = new(wrapperSettings);
            if (!_wrapper.StartContent(CoreName, GamesDirectory, GameNames))
            {
                Debug.LogError("Failed to start core/game combination");
                return;
            }

            _wrapper.InitGraphics();
            _wrapper.InitAudio();
            _wrapper.InputHandler.Enabled = true;
            ControllersMap = _wrapper.InputHandler.DeviceMap;

            //_wrapper.RewindEnabled = _settings.RewindEnabled;

            if (instanceStartedCallback is not null)
            {
                await UniTask.SwitchToMainThread(cancellationToken);
                instanceStartedCallback();
                await UniTask.SwitchToThreadPool();
            }

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            double gameFrameTime = 1.0 / _wrapper.Game.SystemAVInfo.Fps;
            double startTime     = 0.0;
            double accumulator   = 0.0;

            Running = true;
            while (Running && !cancellationToken.IsCancellationRequested)
            {
                while (_bridgeCommands.Count > 0)
                    if (_bridgeCommands.TryDequeue(out IBridgeCommand command))
                        await command.Execute(_wrapper, cancellationToken);

                if (Paused)
                    _manualResetEvent.Wait(cancellationToken);

                if (!Running || cancellationToken.IsCancellationRequested)
                    break;

                //_wrapper.RewindEnabled = _settings.RewindEnabled;

                //if (_settings.RewindEnabled)
                //    _wrapper.PerformRewind = Rewind;

                double currentTime = stopwatch.Elapsed.TotalSeconds;
                double dt          = currentTime - startTime;
                startTime          = currentTime;

                double targetFrameTime = /*FastForward && FastForwardFactor > 0 ? gameFrameTime / FastForwardFactor : */gameFrameTime;
                if ((accumulator += dt) >= targetFrameTime)
                {
                    _wrapper.RunFrame();
                    accumulator = 0.0;
                }
                else
                    Thread.Sleep(1);
            }

            if (instanceStoppedCallback is not null)
            {
                await UniTask.SwitchToMainThread(cancellationToken);
                instanceStoppedCallback();
                await UniTask.SwitchToThreadPool();
            }

            lock (_lock)
                _wrapper.StopContent();

            await UniTask.SwitchToMainThread(cancellationToken);
            RestoreMaterial();
            await UniTask.SwitchToThreadPool();
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
            if (!Running)
                return;

            if (Paused)
                ResumeContent();

            Running = false;

            lock (_lock)
                _wrapper?.ResetContent();
        }

        public void StopContent()
        {
            if (!Running)
                return;

            if (Paused)
                ResumeContent();

            Running = false;
        }

        public void SetControllerPortDevice(uint port, RETRO_DEVICE device) =>
            _bridgeCommands.Enqueue(new SetControllerPortDeviceBridgeCommand(port, device));

        public void SetDiskIndex(int index) =>
            _bridgeCommands.Enqueue(new SetDiskIndexBridgeCommand(GamesDirectory, GameNames, index));

        public void SetStateSlot(int slot) =>
            _bridgeCommands.Enqueue(new SetStateSlotBridgeCommand(slot));

        public void SaveStateWithScreenshot() =>
            _bridgeCommands.Enqueue(new SaveStateWithScreenshotBridgeCommand(TakeScreenshot));

        public void SaveStateWithoutScreenshot() =>
            _bridgeCommands.Enqueue(new SaveStateWithoutScreenshotBridgeCommand());

        public void LoadState() =>
            _bridgeCommands.Enqueue(new LoadStateBridgeCommand());

        public void SaveSRAM() =>
            _bridgeCommands.Enqueue(new SaveSRAMBridgeCommand());

        public void LoadSRAM() =>
            _bridgeCommands.Enqueue(new LoadSRAMBridgeCommand());

        private void SetTexture(Texture texture)
        {
            if (!texture || !_instanceComponent.Renderer)
                return;

            _texture = texture as Texture2D;
            _instanceComponent.Renderer.material.SetTexture(_shaderTextureId, _texture);
        }

        private void RestoreMaterial()
        {
            if (_instanceComponent.Renderer && _originalMaterial)
                _instanceComponent.Renderer.material = _originalMaterial;
        }

        private async UniTask TakeScreenshot(string screenshotPath, CancellationToken cancellationToken)
        {
            await UniTask.SwitchToMainThread(cancellationToken);

            if (!_texture || !Running)
                return;

            await UniTask.WaitForEndOfFrame(_instanceComponent, cancellationToken);

            Texture2D tex = new(_texture.width, _texture.height, TextureFormat.RGB24, false);
            tex.SetPixels32(_texture.GetPixels32());
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.Destroy(tex);

            screenshotPath = screenshotPath.Replace(".state", ".png");
            File.WriteAllBytes(screenshotPath, bytes);

            await UniTask.SwitchToThreadPool();
        }

        private static ILogProcessor GetLogProcessor() => new LogProcessor();

        private IGraphicsProcessor GetGraphicsProcessor(CancellationToken cancellationToken) => new GraphicsProcessor(SetTexture, FilterMode.Point, cancellationToken);

        private static IAudioProcessor GetAudioProcessor(Transform instanceTransform, CancellationToken cancellationToken)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            AudioProcessor unityAudio = instanceTransform.GetComponentInChildren<AudioProcessor>();
            if (unityAudio && unityAudio.enabled)
            {
                unityAudio.Construct(cancellationToken);
                return unityAudio;
            }
            return new NAudio.AudioProcessor();
#else
            AudioProcessor unityAudio = instanceTransform.GetComponentInChildren<AudioProcessor>(true);
            if (unityAudio)
                unityAudio.gameObject.SetActive(true);
            else
            {
                GameObject audioProcessorGameObject = new("LibretroAudioProcessor");
                audioProcessorGameObject.transform.SetParent(instanceTransform);
                unityAudio = audioProcessorGameObject.AddComponent<AudioProcessor>();
            }

            unityAudio.Construct(cancellationToken);
            return unityAudio;
#endif
        }

        private static IInputProcessor GetInputProcessor(bool analogToDigital)
        {
            PlayerInputManager playerInputManager = UnityEngine.Object.FindObjectOfType<PlayerInputManager>();
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
    }
}
