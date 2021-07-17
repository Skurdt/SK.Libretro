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

using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: InternalsVisibleTo("SK.Libretro.UnityEditor")]

namespace SK.Libretro.Unity
{
    [HelpURL("https://github.com/Skurdt/SK.Libretro/wiki")]
    [DisallowMultipleComponent]
    public sealed class LibretroInstance : MonoBehaviour
    {
        [Tooltip("The renderer to show libretro's content on.")]
        [SerializeField] private Renderer _renderer;

        [Tooltip("Assign this to enable distance based audio control.")]
        [SerializeField] private Transform _viewer;

        [SerializeField] private LibretroSettings _settings;
        [SerializeField] private string _coreName;
        [SerializeField] private string _gameDirectory;
        [SerializeField] private string[] _gameNames;
        [SerializeField] private bool _allowGLCoresInEditor;

        public bool FastForwarding { get; set; }

        private struct SaveStateStatus
        {
            public bool InProgress;
            public bool TakeScreenshot;
        }

        private static bool _firstInstance = true;

        private IInputProcessor _inputProcessor;
        private IAudioProcessor _audioProcessor;

        private Thread _thread;
        private bool _running;
        private bool _paused;
        private int _currentSaveStateSlot;
        private SaveStateStatus _saveStateStatus;
        private bool _loadSaveStateStatus;
        private bool _saveSRAMStatus;
        private bool _loadSRAMStatus;
        private bool _setRewindStatus;

        private bool _performRewind;

        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        private double _targetFrameTime                          = 1.0 / 60.0;
        private double _startTime                                = 0.0;
        private double _accumulator                              = 0.0;

        private int _shaderTextureId;
        private Texture2D _texture;
        private Coroutine _screenshotCoroutine;

        private void Awake()
        {
            if (_firstInstance)
            {
                MainThreadDispatcher mainThreadDispatcher = FindObjectOfType<MainThreadDispatcher>();
                if (mainThreadDispatcher == null)
                    _ = new GameObject("MainThreadDispatcher", typeof(MainThreadDispatcher));

                Utilities.Logger.Instance.AddDebughandler(Debug.Log, true);
                Utilities.Logger.Instance.AddInfoHandler(Debug.Log, true);
                Utilities.Logger.Instance.AddWarningHandler(Debug.LogWarning, true);
                Utilities.Logger.Instance.AddErrorhandler(Debug.LogError, true);
                Utilities.Logger.Instance.AddExceptionHandler(Debug.LogException);

                _firstInstance = false;
            }

            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            _shaderTextureId = Shader.PropertyToID(_settings.ShaderTextureName);
        }

        private void OnDisable() => StopContent();

        public void StartContent()
        {
            if (_running)
                return;

            if (string.IsNullOrEmpty(_coreName))
            {
                Debug.LogError("Core is not set");
                return;
            }

            PlayerInputManager playerInputManager = FindObjectOfType<PlayerInputManager>();
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

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            AudioProcessor unityAudio = _renderer.GetComponentInChildren<AudioProcessor>();
            _audioProcessor = unityAudio != null ? unityAudio : (IAudioProcessor)new NAudio.AudioProcessor();
#else
            AudioProcessor unityAudio = _renderer.GetComponentInChildren<AudioProcessor>(true);
            if (unityAudio != null)
            {
                unityAudio.gameObject.SetActive(true);
                _audioProcessor = unityAudio;
            }
            else
            {
                GameObject audioProcessorGameObject = new GameObject("LibretroAudioProcessor");
                audioProcessorGameObject.transform.SetParent(_renderer.transform);
                _audioProcessor = audioProcessorGameObject.AddComponent<AudioProcessor>();
            }
#endif
            _thread = new Thread(LibretroThread)
            {
                Name = $"LibretroThread_{_coreName}_{(_gameNames.Length > 0 ? _gameNames[0] : "nogame")}"
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public void PauseContent()
        {
            if (!_running || _paused)
                return;

            _paused = true;
        }

        public void ResumeContent()
        {
            if (!_running || !_paused)
                return;

            _paused = false;
            _thread.Interrupt();
        }

        public void StopContent()
        {
            if (!_running)
                return;

            if (_paused)
                ResumeContent();

            _running = false;
        }

        public void TakeScreenshot()
        {
            string screenshotPath = Path.Combine(_settings.MainDirectory, "screenshots", _coreName, $"{(_gameNames.Length > 0 ? _gameNames[0] : _coreName)}.png");
            TakeScreenshot(screenshotPath);
        }

        public void TakeScreenshot(string screenshotPath) => MainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (_screenshotCoroutine == null && _texture != null && _running)
                _screenshotCoroutine = StartCoroutine(CoSaveScreenshot(screenshotPath));
        });

        public void SetSaveStateSlot(string slot)
        {
            if (int.TryParse(slot, out int slotInt))
                _currentSaveStateSlot = slotInt;
        }

        public void SetSaveStateSlot(int slot) => _currentSaveStateSlot = slot;

        public void SaveStateWithScreenshot() => SaveState(true);

        public void SaveStateWithoutScreenshot() => SaveState(false);

        public void SaveState(bool takeScreenshot)
        {
            _saveStateStatus.InProgress     = true;
            _saveStateStatus.TakeScreenshot = takeScreenshot;
        }

        public void LoadState() => _loadSaveStateStatus = true;

        public void SaveSRAM() => _saveSRAMStatus = true;

        public void LoadSRAM() => _loadSRAMStatus = true;

        public void SetRewindEnabled(bool enabled)
        {
            _settings.RewindEnabled = enabled;
            _setRewindStatus        = true;
        }

        public void PerformRewind(bool rewind) => _performRewind = rewind;

        private void LibretroThread()
        {
            LibretroWrapper wrapper = new LibretroWrapper((LibretroTargetPlatform)Application.platform, _settings.MainDirectory);
            if (!wrapper.StartContent(_coreName, _gameDirectory, _gameNames?[0]))
            {
                Debug.LogError("Failed to start core/game combination");
                return;
            }

            if (wrapper.Core.HwAccelerated)
            {
                // Running gl cores only works in builds, or if a debugger is attached to Unity instance. Set "_allowGLCoresInEditor" to true to bypass this.
                if (Application.isEditor && !_allowGLCoresInEditor)
                {
                    wrapper.StopContent();
                    Debug.LogError("Starting hardware accelerated cores is not supported in the editor");
                    return;
                }

                wrapper.InitHardwareContext();
            }

            IGraphicsProcessor graphicsProcessor = new GraphicsProcessor(wrapper.Game.VideoWidth, wrapper.Game.VideoHeight, SetTexture);
            wrapper.Graphics.Enable(graphicsProcessor);
            wrapper.Audio.Enable(_audioProcessor);
            wrapper.Input.Enable(_inputProcessor);

            wrapper.RewindEnabled = _settings.RewindEnabled;

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
                        if (!_running)
                            break;
                    }
                }

                if (_saveStateStatus.InProgress)
                {
                    if (wrapper.Serialization.SaveState(_currentSaveStateSlot, out string screenshotPath))
                        if (_saveStateStatus.TakeScreenshot)
                            TakeScreenshot(screenshotPath);
                    _saveStateStatus.InProgress = false;
                }

                if (!_saveStateStatus.InProgress && _loadSaveStateStatus)
                {
                    _ = wrapper.Serialization.LoadState(_currentSaveStateSlot);
                    _loadSaveStateStatus = false;
                }

                if (_saveSRAMStatus)
                {
                    _ = wrapper.Serialization.SaveSRAM();
                    _saveSRAMStatus = false;
                }

                if (!_saveSRAMStatus && _loadSRAMStatus)
                {
                    _ = wrapper.Serialization.LoadSRAM();
                    _loadSRAMStatus = false;
                }

                if (_setRewindStatus)
                {
                    wrapper.RewindEnabled = _settings.RewindEnabled;
                    _setRewindStatus = false;
                }


                if (_settings.RewindEnabled && !_setRewindStatus)
                    wrapper.PerformRewind = _performRewind;

                double currentTime = _stopwatch.Elapsed.TotalSeconds;
                double dt = currentTime - _startTime;
                _startTime = currentTime;

                double targetFrameTime = FastForwarding ? 1.0 / wrapper.Game.VideoFps / 4.0 : 1.0 / wrapper.Game.VideoFps;
                if ((_accumulator += dt) >= targetFrameTime)
                {
                    wrapper.RunFrame();
                    _accumulator = 0.0;
                }
            }

            wrapper.StopContent();
        }

        private void SetTexture(Texture texture)
        {
            if (_renderer == null || texture == null)
                return;

            _texture = texture as Texture2D;
            _renderer.material.SetTexture(_shaderTextureId, texture);
        }

        private IEnumerator CoSaveScreenshot(string screenshotPath)
        {
            yield return new WaitForEndOfFrame();

            Texture2D tex = new Texture2D(_texture.width, _texture.height, TextureFormat.RGB24, false);
            tex.SetPixels32(_texture.GetPixels32());
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            Destroy(tex);

            screenshotPath = screenshotPath.Replace(".state", ".png");
            File.WriteAllBytes(screenshotPath, bytes);

            _screenshotCoroutine = null;
        }
    }
}
