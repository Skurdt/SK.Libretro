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
            public float TimeScale                      = 1.0f;
            public bool AudioVolumeControlledByDistance = true;
            public float AudioMaxVolume                 = 1f;
            public float AudioMinDistance               = 2f;
            public float AudioMaxDistance               = 10f;
            public bool AnalogDirectionsToDigital       = false;
        }

        public bool Running { get; private set; } = false;

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

        private readonly ScreenNode _screenNode;
        private readonly Transform _screenTransform;
        private readonly Renderer _screenRenderer;
        private readonly Transform _viewer;
        private readonly Settings _settings;

        private readonly Material _savedMaterial = null;

        private LibretroWrapper _wrapper              = null;
        private System.Func<IEnumerator> _updateFunc  = null;
        private GraphicsProcessorHardware _hwRenderer = null;
        private bool _inputEnabled                    = false;

        private Coroutine _updateCoroutine     = null;
        private Coroutine _screenshotCoroutine = null;

        private readonly System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
        private readonly int _maxSkipFrames                      = 10;
        private double _targetFrameTime                          = 0.0;
        private double _accumulatedTime                          = 0.0;
        private int _nLoops                                      = 0;

        public LibretroBridge(ScreenNode screen, Transform viewer, Settings settings = null)
        {
            if (_firstInstance)
            {
                Utilities.Logger.SetLoggers(Debug.Log, Debug.LogWarning, Debug.LogError);
                Utilities.Logger.ColorSupport = true;
                _firstInstance = false;
            }

            _screenNode      = screen;
            _screenTransform = screen.transform;
            _screenRenderer  = screen.GetComponent<Renderer>();
            _viewer          = viewer;
            _settings        = settings ?? new Settings();
            _savedMaterial   = new Material(_screenRenderer.material);
        }

        public bool Start(string coreName, string gameDirectory, string gameName)
        {
            _wrapper = new LibretroWrapper((LibretroTargetPlatform)Application.platform, Path.Combine(Application.streamingAssetsPath, "libretro~"));
            if (!_wrapper.StartGame(coreName, gameDirectory, gameName))
            {
                Stop();
                return false;
            }

            if (_wrapper.Core.HwAccelerated)
            {
                if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore)
                {
                    Stop();
                    return false;
                }
                _updateFunc = CoUpdateHardware;
            }
            else
                _updateFunc = CoUpdateSoftware;

            ActivateGraphics();
            ActivateAudio();

            _accumulatedTime = 0;
            _stopwatch.Restart();

            Running = true;
            return true;
        }

        public void Update()
        {
            if (_wrapper == null || !Running)
                return;

            if (_updateCoroutine == null)
                _updateCoroutine = _screenNode.StartCoroutine(_updateFunc());

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (_settings.AudioVolumeControlledByDistance && _wrapper.Audio.Processor is NAudio.AudioProcessor audioProcessor)
            {
                float distance = Vector3.Distance(_screenTransform.position, _viewer.transform.position);
                if (distance > 0f)
                {
                    float volume = math.clamp(math.pow((distance - _settings.AudioMaxDistance) / (_settings.AudioMinDistance - _settings.AudioMaxDistance), 2f), 0f, _settings.AudioMaxVolume);
                    audioProcessor.SetVolume(volume);
                }
            }
#endif
        }

        public void Pause()
        {
            if (!Running)
                return;

            if (_updateCoroutine != null)
            {
                _screenNode.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }

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
            if (_updateCoroutine != null)
            {
                _screenNode.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }

            DeactivateGraphics();
            DeactivateAudio();
            DeactivateInput();

            if (_screenRenderer != null && _savedMaterial != null)
                _screenRenderer.material = _savedMaterial;

            _wrapper?.StopGame();
            _wrapper = null;

            _updateFunc = null;
            Running    = false;
        }

        public void Rewind(bool rewind) => _wrapper.DoRewind = rewind;

        public bool SaveState(int index, bool saveScreenshot)
        {
            if (_wrapper == null)
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
            if (_wrapper == null)
                return false;

            _accumulatedTime = 0;
            _stopwatch.Restart();

            return _wrapper.LoadState(index);
        }

        public void SaveScreenshot(string screenshotPath)
        {
            if (_screenshotCoroutine == null)
                _screenshotCoroutine = _screenNode.StartCoroutine(CoSaveScreenshot(screenshotPath));
        }

        public void ToggleAnalogToDigitalInput(bool value)
        {
            DeactivateInput();
            _settings.AnalogDirectionsToDigital = value;
            ActivateInput();
        }

        private IEnumerator CoUpdateSoftware()
        {
            if (_wrapper == null || !Running)
            {
                Stop();
                yield break;
            }

            UpdateTimings();

            while (_accumulatedTime >= _targetFrameTime && _nLoops < _maxSkipFrames)
            {
                _wrapper.Update();
                _accumulatedTime -= _targetFrameTime;
                ++_nLoops;
                yield return null;
            }

            _updateCoroutine = null;
        }

        private IEnumerator CoUpdateHardware()
        {
            if (_wrapper == null || !Running || _hwRenderer == null || !_hwRenderer.Initialized)
            {
                Stop();
                yield break;
            }

            UpdateTimings();

            _hwRenderer.ClearRetroRunCommandQueue();

            while (_accumulatedTime >= _targetFrameTime && _nLoops < _maxSkipFrames)
            {
                _hwRenderer.UpdateRetroRunCommandQueue();
                _accumulatedTime -= _targetFrameTime;
                ++_nLoops;
                yield return null;
            }

            yield return new WaitForEndOfFrame();
            _hwRenderer.FlushRetroRunCommandQueue();

            _updateCoroutine = null;
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
            if (_wrapper == null)
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
        }

        private void DeactivateGraphics()
        {
            _wrapper?.DeactivateGraphics();
            _hwRenderer = null;
        }

        private void ActivateAudio()
        {
            if (_wrapper == null)
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
        }

        private void DeactivateAudio() => _wrapper?.DeactivateAudio();

        private void ActivateInput()
        {
            if (_wrapper == null)
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
            if (processor == null)
                processor = playerInputManager.gameObject.AddComponent<InputProcessor>();

            processor.AnalogDirectionsToDigital = _settings.AnalogDirectionsToDigital;
            _wrapper.ActivateInput(processor);
            _inputEnabled = true;
        }

        private void DeactivateInput()
        {
            _wrapper?.DeactivateInput();
            _inputEnabled = false;
        }

        private void SetTexture(Texture texture)
        {
            if (_screenRenderer == null || texture == null)
                return;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _screenRenderer.GetPropertyBlock(block);
            block.SetTexture("_Texture", texture);
            block.SetFloat("_Intensity", 1.2f);
            _screenRenderer.SetPropertyBlock(block);
        }

        private IEnumerator CoSaveScreenshot(string screenshotPath)
        {
            yield return new WaitForEndOfFrame();

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _screenRenderer.GetPropertyBlock(block);
            Texture2D tex = block.GetTexture("_Texture") as Texture2D;
    	    File.WriteAllBytes(screenshotPath, tex.EncodeToTGA());
            _screenshotCoroutine = null;
        }
    }
}
