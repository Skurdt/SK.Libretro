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

using SK.Libretro.Header;
using System;
using System.Collections;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal class BridgeMainThread : IDisposable
    {
        public virtual bool Running
        {
            get => _running;
            protected set => _running = value;
        }

        public virtual bool Paused
        {
            get => _paused;
            protected set => _paused = value;
        }

        public virtual int FastForwardFactor
        {
            get => _fastForwardFactor;
            set => _fastForwardFactor = math.clamp(value, 2, 32);
        }

        public virtual bool FastForward
        {
            get => _fastForward;
            set => _fastForward = value;
        }

        public virtual bool Rewind
        {
            get => _rewind;
            set => _rewind = value;
        }

        public virtual bool InputEnabled
        {
            get => _inputEnabled;
            set => _inputEnabled = value;
        }

        public ControllersMap ControllersMap { get; protected set; }

        protected event Action OnInstanceStarted;
        protected event Action OnInstanceStopped;

        protected readonly BridgeSettings _settings;
        protected string CoreName { get; private set; }
        protected string GamesDirectory { get; private set; }
        protected string[] GameNames { get; private set; }

        protected readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();

        protected volatile bool _running;
        protected volatile bool _paused;
        protected volatile int _fastForwardFactor = 8;
        protected volatile bool _fastForward;
        protected volatile bool _rewind;
        protected volatile bool _inputEnabled;

        protected IInputProcessor _inputProcessor;
        protected IAudioProcessor _audioProcessor;
        protected int _currentStateSlot;
        protected double _startTime;
        protected double _accumulator;

        private static bool _firstInstance = true;

        private readonly LibretroInstance _instanceComponent;
        private readonly Renderer _screen;
        private readonly int _shaderTextureId;

        private Wrapper _wrapper;
        private Texture2D _texture;
        private Coroutine _screenshotCoroutine;
        private double _gameFrameTime;

        public BridgeMainThread(LibretroInstance instance)
        {
            if (_firstInstance)
            {
                Logger.Instance.AddDebughandler(Debug.Log, true);
                Logger.Instance.AddInfoHandler(Debug.Log, true);
                Logger.Instance.AddWarningHandler(Debug.LogWarning, true);
                Logger.Instance.AddErrorhandler(Debug.LogError, true);
                Logger.Instance.AddExceptionHandler(Debug.LogException);

                _firstInstance = false;
            }

            _instanceComponent = instance;
            _screen            = instance.Renderer;
            _settings          = instance.Settings;

            _shaderTextureId = Shader.PropertyToID(_settings.ShaderTextureName);
        }

        public virtual void Dispose() =>
            StopContent();

        public void SetContent(string coreName, string gamesDirectory, string[] gameNames)
        {
            if (Running)
                return;

            CoreName       = coreName;
            GamesDirectory = gamesDirectory;
            GameNames      = gameNames;
        }

        public void StartContent(Action instanceStartedCallback, Action instanceStoppedCallback)
        {
            if (Running)
                return;

            if (string.IsNullOrWhiteSpace(CoreName))
            {
                Debug.LogError("Core is not set");
                return;
            }

            PlayerInputManager playerInputManager = UnityEngine.Object.FindObjectOfType<PlayerInputManager>();
            if (!playerInputManager)
            {
                GameObject processorGameObject = new("LibretroInputProcessor");
                playerInputManager = processorGameObject.AddComponent<PlayerInputManager>();
                playerInputManager.EnableJoining();
                playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
                playerInputManager.playerPrefab = Resources.Load<GameObject>("pfLibretroUserInput");
            }

            if (!playerInputManager.TryGetComponent(out _inputProcessor))
                _inputProcessor = playerInputManager.gameObject.AddComponent<InputProcessor>();

            _inputProcessor.AnalogToDigital = _settings.AnalogToDigital;

            GameObject go = _screen.gameObject;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            AudioProcessor unityAudio = go ? go.GetComponentInChildren<AudioProcessor>() : null;
            _audioProcessor = unityAudio ? unityAudio : new NAudio.AudioProcessor();
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

            StartContent();
        }

        public void StopContent()
        {
            if (!Running)
                return;

            if (Paused)
                ResumeContent();

            Running = false;

            OnInstanceStopped?.Invoke();

            _wrapper?.StopContent();
        }

        public void TickMainThread()
        {
            if (!Running || Paused)
                return;

            _wrapper.RewindEnabled = _settings.RewindEnabled;

            if (_settings.RewindEnabled)
                _wrapper.PerformRewind = Rewind;

            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            double dt = currentTime - _startTime;
            _startTime = currentTime;

            double targetFrameTime = FastForward && FastForwardFactor > 0 ? _gameFrameTime / FastForwardFactor : _gameFrameTime;
            if ((_accumulator += dt) >= targetFrameTime)
            {
                _wrapper.RunFrame();
                _accumulator = 0.0;
            }
        }

        public virtual void PauseContent()
        {
            if (!Running || Paused)
                return;

            Paused = true;
        }

        public virtual void ResumeContent()
        {
            if (!Running || !Paused)
                return;

            Paused = false;
        }

        public virtual void SetStateSlot(string slot)
        {
            if (int.TryParse(slot, out int slotInt))
                _currentStateSlot = slotInt;
        }

        public virtual void SetStateSlot(int slot) =>
            _currentStateSlot = slot;

        public virtual void SaveStateWithScreenshot()
        {
            if (_wrapper.Serialization.SaveState(_currentStateSlot, out string screenshotPath))
                TakeScreenshot(screenshotPath);
        }

        public virtual void SaveStateWithoutScreenshot() =>
            _wrapper.Serialization.SaveState(_currentStateSlot);

        public virtual void LoadState() =>
            _wrapper.Serialization.LoadState(_currentStateSlot);

        public virtual void SaveSRAM() =>
            _wrapper.Serialization.SaveSRAM();

        public virtual void LoadSRAM() =>
            _wrapper.Serialization.LoadSRAM();

        public virtual void SetDiskIndex(int index)
        {
            if (GameNames.Length > index)
                _ = _wrapper.Disk?.SetImageIndexAuto((uint)index, $"{GamesDirectory}/{GameNames[index]}");
        }

        public virtual void SetControllerPortDevice(uint port, RETRO_DEVICE device) =>
            _wrapper.Core.retro_set_controller_port_device(port, device);

        public virtual void TakeScreenshot(string screenshotPath)
        {
            if (_screenshotCoroutine is null && _texture && Running)
                _screenshotCoroutine = _instanceComponent.StartCoroutine(CoSaveScreenshot(screenshotPath));
        }

        protected virtual void StartContent()
        {
            try
            {
                _wrapper = InitializeWrapper();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected virtual void InvokeOnStartedEvent() => OnInstanceStarted?.Invoke();

        protected virtual IGraphicsProcessor GetGraphicsProcessor(int videoWidth, int videoHeight) =>
            new GraphicsProcessor(videoWidth, videoHeight, SetTexture);

        protected Wrapper InitializeWrapper()
        {
            Wrapper wrapper = new((RuntimePlatform)Application.platform, _settings.MainDirectory);
            if (!wrapper.StartContent(CoreName, GamesDirectory, GameNames?[0]))
            {
                Debug.LogError("Failed to start core/game combination");
                return null;
            }

            if (GameNames != null)
                for (int i = 0; i < GameNames.Length; ++i)
                    _ = wrapper.Disk?.AddImageIndex();

            IGraphicsProcessor graphicsProcessor = GetGraphicsProcessor(wrapper.Game.VideoWidth, wrapper.Game.VideoHeight);
            GraphicsFrameHandlerBase graphicsFrameHandler;
            if (wrapper.Core.HwAccelerated)
            {
                if (Application.isEditor && !_settings.AllowGLCoresInEditor)
                {
                    wrapper.StopContent();
                    Debug.LogError("Starting hardware accelerated cores is not supported in the editor");
                    return null;
                }

                graphicsFrameHandler = new GraphicsFrameHandlerOpenGLXRGB8888VFlip(graphicsProcessor, wrapper.OpenGL);

                wrapper.InitHardwareContext();
            }
            else
                graphicsFrameHandler = wrapper.Graphics.PixelFormat switch
                {
                    retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555 => new GraphicsFrameHandlerSoftware0RGB1555(graphicsProcessor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888 => new GraphicsFrameHandlerSoftwareXRGB8888(graphicsProcessor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_RGB565   => new GraphicsFrameHandlerSoftwareRGB565(graphicsProcessor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN
                    or _ => new GraphicsFrameHandlerNull(graphicsProcessor)
                };

            wrapper.Graphics.Init(graphicsFrameHandler);
            wrapper.Graphics.Enabled = true;

            wrapper.Audio.Init(_audioProcessor);
            wrapper.Audio.Enabled = true;

            wrapper.Input.Init(_inputProcessor);
            wrapper.Input.Enabled = true;

            ControllersMap = wrapper.Input.DeviceMap;

            wrapper.RewindEnabled = _settings.RewindEnabled;

            _gameFrameTime = 1.0 / wrapper.Game.VideoFps;

            InvokeOnStartedEvent();

            Running = true;
            return wrapper;
        }

        protected void SetTexture(Texture texture)
        {
            if (!texture || !_screen)
                return;

            _texture = texture as Texture2D;
            _screen.material.SetTexture(_shaderTextureId, _texture);
        }

        private IEnumerator CoSaveScreenshot(string screenshotPath)
        {
            yield return new WaitForEndOfFrame();

            Texture2D tex = new(_texture.width, _texture.height, TextureFormat.RGB24, false);
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
