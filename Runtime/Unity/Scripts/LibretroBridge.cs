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
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace SK.Libretro.Unity
{
    public sealed class LibretroBridge
    {
        public sealed class Settings
        {
            public string MainDirectory                 = Path.Combine(Application.streamingAssetsPath, "libretro~");
            public string ShaderTextureName             = "_BaseMap";
            public float TimeScale                      = 1.0f;
            public bool AudioVolumeControlledByDistance = true;
            public float AudioMaxVolume                 = 1f;
            public float AudioMinDistance               = 2f;
            public float AudioMaxDistance               = 10f;
            public bool AnalogDirectionsToDigital       = false;
        }

        public bool Running { get; private set; } = false;

        public bool GraphicsEnabled
        {
            get => _graphicsEnabled;
            set
            {
                if (value)
                    ActivateGraphics();
                else
                    DeactivateGraphics();
            }
        }

        public bool AudioEnabled
        {
            get => _audioEnabled;
            set
            {
                if (value)
                    ActivateAudio();
                else
                    DeactivateAudio();
            }
        }

        public bool InputEnabled
        {
            get => _inputEnabled;
            set
            {
                if (value)
                    ActivateInput();
                else
                    DeactivateInput();
            }
        }

        private static bool _firstInstance = true;

        private readonly Transform _screenTransform;
        private readonly Renderer _screenRenderer;
        private readonly Transform _viewer;
        private readonly Settings _settings;

        private readonly int _shaderTextureId;
        private readonly Material _savedMaterial;

        private LibretroWrapper _wrapper              = null;
        private GraphicsProcessorHardware _hwRenderer = null;
        private bool _graphicsEnabled                 = false;
        private bool _audioEnabled                    = false;
        private bool _inputEnabled                    = false;

        private readonly System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
        private readonly int _maxSkipFrames                      = 10;
        private double _targetFrameTime                          = 0.0;
        private double _accumulatedTime                          = 0.0;
        private int _nLoops                                      = 0;

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

            _screenTransform = screen.transform;
            _screenRenderer  = screen.GetComponent<Renderer>();
            _viewer          = viewer;
            _settings        = settings ?? new Settings();
            _shaderTextureId = Shader.PropertyToID(_settings.ShaderTextureName);
            _savedMaterial   = new Material(_screenRenderer.material);
        }

        public bool Start(string coreName, string gameDirectory, string gameName)
        {
            _wrapper = new LibretroWrapper((LibretroTargetPlatform)Application.platform, _settings.MainDirectory);
            if (!_wrapper.StartGame(coreName, gameDirectory, gameName))
            {
                Stop();
                return false;
            }

            ActivateGraphics();
            ActivateAudio();

            _accumulatedTime = 0;
            _stopwatch.Restart();

            Running = true;

            if (_wrapper.Core.HwAccelerated)
            {
                if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore || _hwRenderer is null || !_hwRenderer.Initialized)
                {
                    Stop();
                    return false;
                }
                UpdateHardware().Forget();
                return true;
            }

            UpdateSoftware().Forget();
            return true;
        }

        public void Pause()
        {
            if (!Running)
                return;

            Running = false;
        }

        public void Resume()
        {
            if (Running)
                return;

            _wrapper.FrameTimeRestart();
            _accumulatedTime = 0;
            _stopwatch.Restart();
            Running = true;
        }

        public void Stop()
        {
            Running = false;

            _wrapper?.StopGame();
            _wrapper = null;

            if (_screenRenderer != null && _savedMaterial != null)
                _screenRenderer.material = _savedMaterial;
        }

        public void Rewind(bool rewind) => _wrapper.DoRewind = rewind;

        public bool SaveState(int index, bool saveScreenshot)
        {
            if (_wrapper is null)
                return false;

            Pause();

            if (!_wrapper.SaveState(index, out string savePath))
            {
                Resume();
                return false;
            }

            if (saveScreenshot)
            {
                string saveFileDirectory = Path.GetDirectoryName(savePath);
                string saveFileName      = Path.GetFileNameWithoutExtension(savePath);
                string screenshotPath    = Path.Combine(saveFileDirectory, $"{saveFileName}.tga");
                SaveScreenshot(screenshotPath);
            }

            Resume();
            return true;
        }

        public bool LoadState(int index)
        {
            if (_wrapper is null)
                return false;

            _accumulatedTime = 0;
            _stopwatch.Restart();

            return _wrapper.LoadState(index);
        }

        public void SaveScreenshot(string screenshotPath)
        {
            if (_savingScreenshot)
                return;
            SaveScreenshotAsync(screenshotPath).Forget();
        }

        public void SetAnalogToDigitalInput(bool value)
        {
            DeactivateInput();
            _settings.AnalogDirectionsToDigital = value;
            ActivateInput();
        }

        public void SetRewindEnabled(bool value)
        {
            if (_wrapper is null)
                return;

            _wrapper.RewindEnabled = value;
        }

        private void UpdateTimings()
        {
            _wrapper.FrameTimeUpdate();

            _targetFrameTime = 1.0 / _wrapper.Game.VideoFps / _settings.TimeScale;
            _accumulatedTime += _stopwatch.Elapsed.TotalSeconds;
            _nLoops = 0;
            _stopwatch.Restart();
        }

        private void ActivateGraphics()
        {
            if (_wrapper is null)
                return;

            IGraphicsProcessor graphicsProcessor;
            if (_wrapper.Core.HwAccelerated)
            {
                // FIXME(Tom): Prevents hardware rendering while in the editor.
                // Hardware rendering only works in builds so far, at least on my windows setup.
                //if (Application.isEditor)
                //{
                //    Debug.LogError("Hardware rendering is disabled in the editor.");
                //    return;
                //}

                _hwRenderer = new GraphicsProcessorHardware(_wrapper.Game.VideoWidth,
                                                            _wrapper.Game.VideoHeight,
                                                            _wrapper.HwRenderHasDepth,
                                                            _wrapper.HwRenderHasStencil)
                {
                    OnTextureRecreated = SetTexture
                };

                graphicsProcessor = _hwRenderer;
                SetTexture(_hwRenderer.Texture);
            }
            else
                graphicsProcessor = new GraphicsProcessorSoftware(_wrapper.Game.VideoWidth, _wrapper.Game.VideoHeight, SetTexture);

            _wrapper.ActivateGraphics(graphicsProcessor);
            _graphicsEnabled = true;
        }

        private void DeactivateGraphics()
        {
            if (_wrapper is null)
                return;

            _wrapper.DeactivateGraphics();
            _hwRenderer      = null;
            _graphicsEnabled = false;
        }

        private void ActivateAudio()
        {
            if (_wrapper is null)
                return;

            IAudioProcessor audioProcessor;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            IAudioProcessor unityAudio = _screenRenderer.GetComponentInChildren<AudioProcessor>();
            audioProcessor = unityAudio ?? new NAudio.AudioProcessor();
#else
            AudioProcessor unityAudio = _screenRenderer.GetComponentInChildren<AudioProcessor>(true);
            if (unityAudio != null)
            {
                unityAudio.gameObject.SetActive(true);
                audioProcessor = unityAudio;
            }
            else
            {
                GameObject audioProcessorGameObject = new GameObject("AudioProcessor");
                audioProcessorGameObject.transform.SetParent(_screenTransform);
                audioProcessor = audioProcessorGameObject.AddComponent<AudioProcessor>();
            }
#endif
            _wrapper.ActivateAudio(audioProcessor);
            _audioEnabled = true;
        }

        private void DeactivateAudio()
        {
            if (_wrapper is null)
                return;

            _wrapper.DeactivateAudio();
            _audioEnabled = false;
        }

        private void ActivateInput()
        {
            if (_wrapper is null)
                return;

            PlayerInputManager playerInputManager = Object.FindObjectOfType<PlayerInputManager>();
            if (playerInputManager == null)
            {
                GameObject processorGameObject = new GameObject("InputProcessor");
                playerInputManager = processorGameObject.AddComponent<PlayerInputManager>();
                playerInputManager.EnableJoining();
                playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
                playerInputManager.playerPrefab = Resources.Load<GameObject>("pfLibretroUserInput");
            }

            IInputProcessor processor = playerInputManager.GetComponent<IInputProcessor>();
            if (processor is null)
                processor = playerInputManager.gameObject.AddComponent<InputProcessor>();

            processor.AnalogDirectionsToDigital = _settings.AnalogDirectionsToDigital;
            _wrapper.ActivateInput(processor);
            _inputEnabled = true;
        }

        private void DeactivateInput()
        {
            if (_wrapper is null)
                return;

            _wrapper.DeactivateInput();
            _inputEnabled = false;
        }

        private async UniTaskVoid UpdateSoftware()
        {
            while (!(_wrapper is null))
            {
                UpdateDistanceBasedAudio();
                UpdateTimings();
                while (_accumulatedTime >= _targetFrameTime && _nLoops < _maxSkipFrames)
                {
                    if (Running)
                        _wrapper.Update();
                    _accumulatedTime -= _targetFrameTime;
                    ++_nLoops;
                }
                await UniTask.Yield();
            }
        }

        private async UniTaskVoid UpdateHardware()
        {
            while (!(_wrapper is null))
            {
                _hwRenderer.ClearRetroRunCommandQueue();

                UpdateDistanceBasedAudio();
                UpdateTimings();
                while (_accumulatedTime >= _targetFrameTime && _nLoops < _maxSkipFrames)
                {
                    if (Running)
                        _hwRenderer.UpdateRetroRunCommandQueue();
                    _accumulatedTime -= _targetFrameTime;
                    ++_nLoops;
                    await UniTask.Yield();
                }
                await UniTask.WaitForEndOfFrame();
                _hwRenderer.FlushRetroRunCommandQueue();
            }
        }

        private void UpdateDistanceBasedAudio()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (_wrapper is null || _viewer == null || _screenTransform == null || !_settings.AudioVolumeControlledByDistance || !(_wrapper.Audio.Processor is NAudio.AudioProcessor audioProcessor))
                return;
            float distance = Vector3.Distance(_screenTransform.position, _viewer.transform.position);
            if (distance > 0f)
            {
                float volume = math.clamp(math.pow((distance - _settings.AudioMaxDistance) / (_settings.AudioMinDistance - _settings.AudioMaxDistance), 2f), 0f, _settings.AudioMaxVolume);
                audioProcessor.SetVolume(volume);
            }
#endif
        }

        private void SetTexture(Texture texture)
        {
            if (_screenRenderer == null || texture == null)
                return;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetTexture(_shaderTextureId, texture);
            _screenRenderer.SetPropertyBlock(block);
        }

        private async UniTaskVoid SaveScreenshotAsync(string screenshotPath)
        {
            _savingScreenshot = true;
            await UniTask.WaitForEndOfFrame();

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _screenRenderer.GetPropertyBlock(block);
            Texture2D tex = block.GetTexture(_shaderTextureId) as Texture2D;
    	    File.WriteAllBytes(screenshotPath, tex.EncodeToTGA());
            _savingScreenshot = false;
        }
    }
}
