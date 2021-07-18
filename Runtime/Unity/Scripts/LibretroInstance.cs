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
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[assembly: InternalsVisibleTo("SK.Libretro.UnityEditor")]

namespace SK.Libretro.Unity
{
    [HelpURL("https://github.com/Skurdt/SK.Libretro/wiki")]
    [DisallowMultipleComponent]
    public sealed class LibretroInstance : MonoBehaviour
    {
        private enum ThreadCommand
        {
            SaveStateWithScreenshot,
            SaveStateWithoutScreenshot,
            LoadState,
            SaveSRAM,
            LoadSRAM,
            TakeScreenshot
        }

        public Renderer Renderer;
        public RawImage RawImage;
        public Transform Viewer;

        public LibretroSettings Settings;
        public string CoreName;
        public string GamesDirectory;
        public string[] GameNames;
        public bool AllowGLCoresInEditor;

        public double FastForwardFactor { get; set; } = 8.0;
        public bool FastForwarding { get; set; }
        public bool PerformRewind { get; set; }
        public bool Paused { get; private set; }

        private static bool _firstInstance = true;

        private IInputProcessor _inputProcessor;
        private IAudioProcessor _audioProcessor;

        private Thread _thread;
        private readonly ConcurrentQueue<ThreadCommand> _threadCommands = new ConcurrentQueue<ThreadCommand>();
        private bool _running;
        private int _currentStateSlot;

        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        private double _startTime = 0.0;
        private double _accumulator = 0.0;

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

                SK.Utilities.Logger.Instance.AddDebughandler(Debug.Log, true);
                SK.Utilities.Logger.Instance.AddInfoHandler(Debug.Log, true);
                SK.Utilities.Logger.Instance.AddWarningHandler(Debug.LogWarning, true);
                SK.Utilities.Logger.Instance.AddErrorhandler(Debug.LogError, true);
                SK.Utilities.Logger.Instance.AddExceptionHandler(Debug.LogException);

                _firstInstance = false;
            }

            if (Renderer == null)
                Renderer = GetComponent<Renderer>();

            if (Renderer == null)
                RawImage = GetComponent<RawImage>();

            if (RawImage == null)
                throw new Exception("No target display available!");

            _shaderTextureId = Shader.PropertyToID(Settings.ShaderTextureName);
        }

        private void OnDisable() => StopContent();

        public void SetContent(string coreName, string gamesDirectory, string[] gameNames)
        {
            CoreName       = coreName;
            GamesDirectory = gamesDirectory;
            GameNames      = gameNames;
        }

        public void StartContent()
        {
            if (_running)
                return;

            if (string.IsNullOrEmpty(CoreName))
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

            _inputProcessor.AnalogToDigital = Settings.AnalogToDigital;

            GameObject go = null;
            if (Renderer != null)
                go = Renderer.gameObject;
            else if (RawImage != null)
                go = RawImage.gameObject;
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
            _thread = new Thread(LibretroThread)
            {
                Name = $"LibretroThread_{CoreName}_{(GameNames.Length > 0 ? GameNames[0] : "nogame")}"
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public void PauseContent()
        {
            if (!_running || Paused)
                return;

            Paused = true;
        }

        public void ResumeContent()
        {
            if (!_running || !Paused)
                return;

            Paused = false;
            _thread.Interrupt();
        }

        public void StopContent()
        {
            if (!_running)
                return;

            if (Paused)
                ResumeContent();

            _running = false;
        }

        public void SetStateSlot(string slot)
        {
            if (int.TryParse(slot, out int slotInt))
                SetStateSlot(slotInt);
        }

        public void SetStateSlot(int slot) => _currentStateSlot = slot;

        public void SaveStateWithScreenshot() => _threadCommands.Enqueue(ThreadCommand.SaveStateWithScreenshot);

        public void SaveStateWithoutScreenshot() => _threadCommands.Enqueue(ThreadCommand.SaveStateWithoutScreenshot);

        public void LoadState() => _threadCommands.Enqueue(ThreadCommand.LoadState);

        public void SaveSRAM() => _threadCommands.Enqueue(ThreadCommand.SaveSRAM);

        public void LoadSRAM() => _threadCommands.Enqueue(ThreadCommand.LoadSRAM);

        public void TakeScreenshot(string screenshotPath) => MainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (_screenshotCoroutine == null && _texture != null && _running)
                _screenshotCoroutine = StartCoroutine(CoSaveScreenshot(screenshotPath));
        });

        private void LibretroThread()
        {
            if (!Directory.Exists(Settings.MainDirectory))
                Settings.MainDirectory = LibretroSettings.DefaultMainDirectory;

            LibretroWrapper wrapper = new LibretroWrapper((LibretroTargetPlatform)Application.platform, Settings.MainDirectory);
            if (!wrapper.StartContent(CoreName, GamesDirectory, GameNames?[0]))
            {
                Debug.LogError("Failed to start core/game combination");
                return;
            }

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
            wrapper.Graphics.Enable(graphicsProcessor);
            wrapper.Audio.Enable(_audioProcessor);
            wrapper.Input.Enable(_inputProcessor);

            wrapper.RewindEnabled = Settings.RewindEnabled;

            double gameFrameTime = 1.0 / wrapper.Game.VideoFps;

            _running = true;
            while (_running)
            {
                while (_threadCommands.Count > 0)
                {
                    if (_threadCommands.TryDequeue(out ThreadCommand command))
                    {
                        switch (command)
                        {
                            case ThreadCommand.SaveStateWithScreenshot:
                                if (wrapper.Serialization.SaveState(_currentStateSlot, out string screenshotPath))
                                    TakeScreenshot(screenshotPath);
                                break;
                            case ThreadCommand.SaveStateWithoutScreenshot:
                                _ = wrapper.Serialization.SaveState(_currentStateSlot);
                                break;
                            case ThreadCommand.LoadState:
                                _ = wrapper.Serialization.LoadState(_currentStateSlot);
                                break;
                            case ThreadCommand.SaveSRAM:
                                _ = wrapper.Serialization.SaveSRAM();
                                break;
                            case ThreadCommand.LoadSRAM:
                                _ = wrapper.Serialization.LoadSRAM();
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (Paused)
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

                wrapper.RewindEnabled = Settings.RewindEnabled;

                if (Settings.RewindEnabled)
                    wrapper.PerformRewind = PerformRewind;

                double currentTime = _stopwatch.Elapsed.TotalSeconds;
                double dt = currentTime - _startTime;
                _startTime = currentTime;

                double targetFrameTime = FastForwarding && FastForwardFactor > 0.0 ? gameFrameTime / FastForwardFactor : gameFrameTime;
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
            if (texture == null)
                return;

            _texture = texture as Texture2D;
            if (Renderer != null)
                Renderer.material.SetTexture(_shaderTextureId, texture);
            else
                RawImage.texture = texture;
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
