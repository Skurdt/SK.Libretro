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
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal abstract class Instance : IDisposable
    {
        public abstract bool Running { get; protected set; }
        public abstract bool Paused { get; protected set; }
        public abstract int FastForwardFactor { get; set; }
        public abstract bool FastForward { get; set; }
        public abstract bool Rewind { get; set; }
        public abstract bool InputEnabled { get; set; }

        protected abstract string CoreName { get; set; }
        protected abstract string GamesDirectory { get; set; }
        protected abstract string[] GameNames { get; set; }

        public ControllersMap ControllersMap { get; protected set; }

        protected event Action OnInstanceStarted;
        protected event Action OnInstanceStopped;

        protected readonly BridgeSettings _settings;
        protected readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        protected readonly Renderer _screen;
        protected readonly Material _originalMaterial;

        protected Wrapper _wrapper;
        protected IAudioProcessor _audioProcessor;
        protected IInputProcessor _inputProcessor;
        protected int _currentStateSlot;
        protected double _startTime;
        protected double _accumulator;

        private readonly LibretroInstance _instanceComponent;
        private readonly ILogProcessor _logProcessor;
        private readonly int _shaderTextureId;

        private Texture2D _texture;
        private Coroutine _screenshotCoroutine;
        private double _gameFrameTime;

        public Instance(LibretroInstance instance)
        {
            _instanceComponent = instance;
            _screen            = instance.Renderer;
            _settings          = instance.Settings;

            _originalMaterial = new(_screen.material);
            _shaderTextureId  = Shader.PropertyToID(_settings.ShaderTextureName);
            _logProcessor     = new LogProcessor();
        }

        public abstract void Dispose();

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

        public void ResetContent()
        {
            if (!Running)
                return;

            if (Paused)
                ResumeContent();

            Running = false;

            _wrapper?.ResetContent();
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

        public abstract void PauseContent();
        public abstract void ResumeContent();
        public abstract void SetStateSlot(string slot);
        public abstract void SetStateSlot(int slot);
        public abstract void SaveStateWithScreenshot();
        public abstract void SaveStateWithoutScreenshot();
        public abstract void LoadState();
        public abstract void SaveSRAM();
        public abstract void LoadSRAM();

        public virtual void SetDiskIndex(int index)
        {
            if (GameNames.Length > index)
                _ = _wrapper.DiskHandler?.SetImageIndexAuto((uint)index, $"{GamesDirectory}/{GameNames[index]}");
        }

        public virtual void SetControllerPortDevice(uint port, RETRO_DEVICE device) =>
            _wrapper.Core.SetControllerPortDevice(port, device);

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
                MainDirectory  = _settings.MainDirectory,
                LogProcessor   = _logProcessor,
                AudioProcessor = _audioProcessor,
                InputProcessor = _inputProcessor
            };

            Wrapper wrapper = new(wrapperSettings);
            if (!wrapper.StartContent(CoreName, GamesDirectory, GameNames?[0]))
            {
                Debug.LogError("Failed to start core/game combination");
                return null;
            }

            if (wrapper.DiskHandler.Enabled && GameNames is not null)
                for (int i = 0; i < GameNames.Length; ++i)
                    _ = wrapper.DiskHandler?.AddImageIndex();

            IGraphicsProcessor graphicsProcessor = GetGraphicsProcessor(wrapper.Game.SystemAVInfo.MaxWidth, wrapper.Game.SystemAVInfo.MaxHeight);
            GraphicsFrameHandlerBase graphicsFrameHandler;
            if (wrapper.Core.HwAccelerated)
            {
                if (Application.isEditor && !_settings.AllowGLCoresInEditor)
                {
                    wrapper.StopContent();
                    Debug.LogError("Starting hardware accelerated cores is not supported in the editor");
                    return null;
                }

                graphicsFrameHandler = new GraphicsFrameHandlerOpenGLXRGB8888VFlip(graphicsProcessor, wrapper.GraphicsHandler.OpenGLHelperWindow);
                wrapper.GraphicsHandler.InitHardwareContext();
            }
            else
                graphicsFrameHandler = wrapper.GraphicsHandler.PixelFormat switch
                {
                    retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555 => new GraphicsFrameHandlerSoftware0RGB1555(graphicsProcessor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888 => new GraphicsFrameHandlerSoftwareXRGB8888(graphicsProcessor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_RGB565 => new GraphicsFrameHandlerSoftwareRGB565(graphicsProcessor),
                    retro_pixel_format.RETRO_PIXEL_FORMAT_UNKNOWN
                    or _ => new NullGraphicsFrameHandler(graphicsProcessor)
                };

            wrapper.InitGraphics(graphicsFrameHandler, true);
            wrapper.InitAudio(true);
            wrapper.InputHandler.Enabled = true;
            ControllersMap = wrapper.InputHandler.DeviceMap;

            wrapper.RewindEnabled = _settings.RewindEnabled;

            _gameFrameTime = 1.0 / wrapper.Game.SystemAVInfo.Fps;

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
