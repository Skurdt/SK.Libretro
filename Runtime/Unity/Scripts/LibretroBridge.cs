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

using Cysharp.Threading.Tasks;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    public sealed class LibretroBridge
    {
        public sealed class Settings
        {
            public string MainDirectory                 = Path.Combine(Application.streamingAssetsPath, "libretro~");
            public string ShaderTextureName             = "_MainTex";
            public bool AudioVolumeControlledByDistance = true;
            public float AudioMaxVolume                 = 1f;
            public float AudioMinDistance               = 2f;
            public float AudioMaxDistance               = 10f;
            public bool AnalogDirectionsToDigital       = false;
        }

        private struct SaveStateStatus
        {
            public bool InProgress;
            public int Index;
            public bool TakeScreenshot;
        }

        private struct LoadStateStatus
        {
            public bool InProgress;
            public int Index;
        }

        private static bool _firstInstance = true;

        //private readonly Transform _screenTransform;
        private readonly Renderer _screenRenderer;
        //private readonly Transform _viewer;
        private readonly Settings _settings;
        private readonly int _shaderTextureId;

        private readonly IInputProcessor _inputProcessor;
        private readonly IAudioProcessor _audioProcessor;

        private string _coreName;
        private string _gameDirectory;
        private string _gameName;

        private Thread _thread;
        private bool _running;
        private bool _paused;

        private Texture2D _texture;
        private SaveStateStatus _saveStateStatus;
        private LoadStateStatus _loadStateStatus;
        private bool _savingScreenshot;

        public LibretroBridge(LibretroScreenNode screen, Transform viewer, Settings settings = null)
        {
            if (_firstInstance)
            {
                Utilities.Logger.Instance.AddDebughandler(Debug.Log, true);
                Utilities.Logger.Instance.AddInfoHandler(Debug.Log, true);
                Utilities.Logger.Instance.AddWarningHandler(Debug.LogWarning, true);
                Utilities.Logger.Instance.AddErrorhandler(Debug.LogError, true);
                Utilities.Logger.Instance.AddExceptionHandler(Debug.LogException);

                _firstInstance = false;
            }

            //_screenTransform = screen.transform;
            _screenRenderer  = screen.GetComponent<Renderer>();
            //_viewer          = viewer;
            _settings        = settings ?? new Settings();
            _shaderTextureId = Shader.PropertyToID(_settings.ShaderTextureName);

            PlayerInputManager playerInputManager = Object.FindObjectOfType<PlayerInputManager>();
            if (playerInputManager == null)
            {
                GameObject processorGameObject = new GameObject("LibretroInputProcessor");
                playerInputManager = processorGameObject.AddComponent<PlayerInputManager>();
                playerInputManager.EnableJoining();
                playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
                playerInputManager.playerPrefab = Resources.Load<GameObject>("pfLibretroUserInput");
            }

            _inputProcessor = playerInputManager.GetComponent<IInputProcessor>();
            if (_inputProcessor is null)
                _inputProcessor = playerInputManager.gameObject.AddComponent<InputProcessor>();

            _inputProcessor.AnalogDirectionsToDigital = _settings.AnalogDirectionsToDigital;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            AudioProcessor unityAudio = _screenRenderer.GetComponentInChildren<AudioProcessor>();
            if (unityAudio != null)
                _audioProcessor = unityAudio;
            else
                _audioProcessor = new NAudio.AudioProcessor();
#else
            AudioProcessor unityAudio = _screenRenderer.GetComponentInChildren<AudioProcessor>(true);
            if (unityAudio != null)
            {
                unityAudio.gameObject.SetActive(true);
                _audioProcessor = unityAudio;
            }
            else
            {
                GameObject audioProcessorGameObject = new GameObject("LibretroAudioProcessor");
                audioProcessorGameObject.transform.SetParent(_screenTransform);
                _audioProcessor = audioProcessorGameObject.AddComponent<AudioProcessor>();
            }
#endif
        }

        public void Start(string coreName, string gameDirectory, string gameName)
        {
            if (_running)
                return;

            _coreName      = coreName;
            _gameDirectory = gameDirectory;
            _gameName      = gameName;

            _thread = new Thread(LibretroThread)
            {
                Name = $"LibretroThread_{_coreName}_{_gameName}"
            };
            _thread.Start();
        }

        public void Pause()
        {
            if (!_running || _paused)
                return;

            _paused = true;
        }

        public void Resume()
        {
            if (!_running || !_paused)
                return;

            _paused = false;
            _thread.Interrupt();
        }

        public void Stop()
        {
            if (!_running)
                return;

            if (_paused)
                Resume();

            _running = false;
        }

        public void TakeScreenshot()
        {
            string screenshotPath = Path.Combine(_settings.MainDirectory, "screenshots", _coreName, $"{_gameName}.png");
            TakeScreenshot(screenshotPath);
        }

        public void TakeScreenshot(string screenshotPath) => MainThreadDispatcher.Instance.Enqueue(() => SaveScreenshotAsync(screenshotPath).Forget());

        public void SaveStateWithScreenshot() => SaveState(0, true);

        public void SaveStateWithoutScreenshot() => SaveState(0, false);

        public void SaveStateWithScreenshot(int index) => SaveState(index, true);

        public void SaveStateWithoutScreenshot(int index) => SaveState(index, false);

        public void SaveState(int index, bool takeScreenshot)
        {
            _saveStateStatus.InProgress     = true;
            _saveStateStatus.Index          = index;
            _saveStateStatus.TakeScreenshot = takeScreenshot;
        }

        public void LoadState() => LoadState(0);

        public void LoadState(int index)
        {
            _loadStateStatus.InProgress = true;
            _loadStateStatus.Index      = index;
        }

        private void LibretroThread()
        {
            LibretroWrapper wrapper = new LibretroWrapper((LibretroTargetPlatform)Application.platform, _settings.MainDirectory);
            if (!wrapper.StartGame(_coreName, _gameDirectory, _gameName))
            {
                wrapper.StopGame();
                throw new System.Exception("Failed to start core/game combination");
            }

            if (wrapper.Core.HwAccelerated)
            {
                // Running gl cores only works in builds, or if a debugger is attached to the unity instance. (Visual Studio > Attach > Unity.exe)
                if (Application.isEditor)
                {
                    wrapper.StopGame();
                    throw new System.Exception("Starting hardware accelerated cores is not supported in the editor");
                }

                wrapper.InitHardwareContext();
            }

            IGraphicsProcessor graphicsProcessor = new GraphicsProcessor(wrapper.Game.VideoWidth, wrapper.Game.VideoHeight, SetTexture);
            wrapper.ActivateGraphics(graphicsProcessor);
            wrapper.ActivateAudio(_audioProcessor);
            wrapper.ActivateInput(_inputProcessor);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            double targetFrameTime = 1.0 / wrapper.Game.VideoFps;
            double startTime       = 0.0;
            double accumulator     = 0.0;

            _running = true;
            while (_running)
            {
                if (_paused)
                {
                    try
                    {
                        Thread.Sleep(Timeout.Infinite);
                    }
                    catch (ThreadInterruptedException)
                    {
                        Debug.Log($"[Paused] Thread '{Thread.CurrentThread.Name}' awoken.");
                        if (!_running)
                            break;
                    }
                }

                if (_saveStateStatus.InProgress)
                {
                    if (wrapper.SaveState(_saveStateStatus.Index, out string screenshotPath))
                        TakeScreenshot(screenshotPath);
                    _saveStateStatus.InProgress = false;
                }

                if (_loadStateStatus.InProgress)
                {
                    _ = wrapper.LoadState(_loadStateStatus.Index);
                    _loadStateStatus.InProgress = false;
                }

                //wrapper.FrameTimeUpdate();

                double currentTime = stopwatch.Elapsed.TotalSeconds;
                double dt          = currentTime - startTime;
                startTime          = currentTime;
                if ((accumulator += dt) >= targetFrameTime)
                {
                    wrapper.Update();
                    accumulator = 0.0;
                }
            }

            wrapper.StopGame();
            Thread.Sleep(200);
        }

        //  public void Rewind(bool rewind) => _wrapper.DoRewind = rewind;

        //  public void SetAnalogToDigitalInput(bool value)
        //  {
        //      DeactivateInput();
        //      _settings.AnalogDirectionsToDigital = value;
        //      ActivateInput();
        //  }

        //  public void SetRewindEnabled(bool value)
        //  {
        //      if (_wrapper is null)
        //          return;

        //      _wrapper.RewindEnabled = value;
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
        //      if (_wrapper is null || _viewer == null || _screenTransform == null || !_settings.AudioVolumeControlledByDistance || !(_wrapper.Audio.Processor is NAudio.AudioProcessor audioProcessor))
        //          return;
        //      float distance = Vector3.Distance(_screenTransform.position, _viewer.transform.position);
        //      if (distance > 0f)
        //      {
        //          float volume = math.clamp(math.pow((distance - _settings.AudioMaxDistance) / (_settings.AudioMinDistance - _settings.AudioMaxDistance), 2f), 0f, _settings.AudioMaxVolume);
        //          audioProcessor.SetVolume(volume);
        //      }
        //#endif
        //  }

        private void SetTexture(Texture texture)
        {
            if (_screenRenderer == null || texture == null)
                return;

            _texture = texture as Texture2D;
            _screenRenderer.material.SetTexture(_shaderTextureId, texture);
        }

        private async UniTaskVoid SaveScreenshotAsync(string screenshotPath)
        {
            if (_texture == null || !_running || _savingScreenshot)
                return;

            _savingScreenshot = true;
            await UniTask.WaitForEndOfFrame();

            Texture2D tex = new Texture2D(_texture.width, _texture.height, TextureFormat.RGB24, false);
            tex.SetPixels32(_texture.GetPixels32());
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            Object.Destroy(tex);

            screenshotPath = screenshotPath.Replace(".state", ".png");
            File.WriteAllBytes(screenshotPath, bytes);

            _savingScreenshot = false;
        }
    }
}
