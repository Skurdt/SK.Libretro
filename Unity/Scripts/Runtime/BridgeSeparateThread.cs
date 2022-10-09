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

using System;
using System.Collections.Concurrent;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class BridgeSeparateThread : Bridge
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

        public override bool Running
        {
            get
            {
                lock (_lock)
                    return base.Running;
            }
            protected set
            {
                lock (_lock)
                    base.Running = value;
            }
        }

        public override bool Paused
        {
            get
            {
                lock (_lock)
                    return base.Paused;
            }
            protected set
            {
                lock (_lock)
                    base.Paused = value;
            }
        }

        public override int FastForwardFactor
        {
            get
            {
                lock (_lock)
                    return base.FastForwardFactor;
            }
            set
            {
                lock (_lock)
                    base.FastForwardFactor = math.clamp(value, 2, 32);
            }
        }

        public override bool FastForward
        {
            get
            {
                lock (_lock)
                    return base.FastForward;
            }
            set
            {
                lock (_lock)
                    base.FastForward = value;
            }
        }

        public override bool Rewind
        {
            get
            {
                lock (_lock)
                    return base.Rewind;
            }
            set
            {
                lock (_lock)
                    base.Rewind = value;
            }
        }

        public override bool InputEnabled
        {
            get
            {
                lock (_lock)
                    return base.InputEnabled;
            }

            set
            {
                lock (_lock)
                    base.InputEnabled = value;

                if (InputEnabled)
                    _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.EnableInput });
                else
                    _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.DisableInput });

            }
        }

        private readonly ManualResetEventSlim _manualResetEvent = new(false);
        private readonly ConcurrentQueue<ThreadCommand> _threadCommands = new();
        private readonly object _lock = new();

        private Thread _thread;

        public BridgeSeparateThread(LibretroInstance instance)
        : base(instance)
        {
            if (!UnityEngine.Object.FindObjectOfType<MainThreadDispatcher>())
                _ = new GameObject("MainThreadDispatcher", typeof(MainThreadDispatcher));
        }

        public override void Dispose()
        {
            base.Dispose();
            _ = _thread?.Join(1000);

            _thread = null;
            _manualResetEvent.Dispose();
        }

        public override void PauseContent()
        {
            base.PauseContent();
            _manualResetEvent.Reset();
        }

        public override void ResumeContent()
        {
            base.ResumeContent();
            _manualResetEvent.Set();
        }

        public override void SetStateSlot(string slot)
        {
            lock (_lock)
                base.SetStateSlot(slot);
        }

        public override void SetStateSlot(int slot)
        {
            lock (_lock)
                base.SetStateSlot(slot);
        }

        public override void SaveStateWithScreenshot() =>
            _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SaveStateWithScreenshot });

        public override void SaveStateWithoutScreenshot() =>
            _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SaveStateWithoutScreenshot });

        public override void LoadState() =>
            _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.LoadState });

        public override void SaveSRAM() =>
            _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SaveSRAM });

        public override void LoadSRAM() =>
            _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.LoadSRAM });

        public override void SetDiskIndex(int index) =>
            _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SetDiskIndex, Param0 = index });

        public override void SetControllerPortDevice(uint port, uint id) =>
            _threadCommands.Enqueue(new ThreadCommand { Type = ThreadCommandType.SetControllerPortDevice, Param0 = port, Param1 = id });

        protected override void StartContent()
        {
            _thread = new Thread(LibretroThread)
            {
                Name = $"LibretroThread_{CoreName}_{(GameNames.Length > 0 ? GameNames[0] : "nogame")}",
                IsBackground = true,
                Priority = System.Threading.ThreadPriority.Lowest
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        private void LibretroThread()
        {
            try
            {
                Wrapper wrapper = new((TargetPlatform)Application.platform, Settings.MainDirectory);
                if (!wrapper.StartContent(CoreName, GamesDirectory, GameNames?[0]))
                {
                    Debug.LogError("Failed to start core/game combination");
                    return;
                }

                if (GameNames != null)
                    foreach (string gameName in GameNames)
                        _ = wrapper.Disk?.AddImageIndex();

                if (wrapper.Core.HwAccelerated)
                {
                    // Running gl cores only works in builds, or if a debugger is attached to Unity instance. Set "_allowGLCoresInEditor" to true to bypass this.
                    if (Application.isEditor && !Settings.AllowGLCoresInEditor)
                    {
                        wrapper.StopContent();
                        Debug.LogError("Starting hardware accelerated cores is not supported in the editor");
                        return;
                    }

                    wrapper.InitHardwareContext();
                }

                IGraphicsProcessor graphicsProcessor = new GraphicsProcessorSeparateThread(wrapper.Game.VideoWidth, wrapper.Game.VideoHeight, SetTexture);
                wrapper.Graphics.Init(graphicsProcessor);
                wrapper.Graphics.Enabled = true;

                wrapper.Audio.Init(_audioProcessor);
                wrapper.Audio.Enabled = true;

                wrapper.Input.Init(_inputProcessor);
                wrapper.Input.Enabled = true;

                ControllersMap = wrapper.Input.DeviceMap;

                wrapper.RewindEnabled = Settings.RewindEnabled;

                double gameFrameTime = 1.0 / wrapper.Game.VideoFps;

                _manualResetEvent.Reset();
                MainThreadDispatcher.Enqueue(() =>
                {
                    InvokeOnStartedEvent();
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
                                            if (GameNames.Length > index)
                                                _ = wrapper.Disk?.SetImageIndexAuto((uint)index, $"{GamesDirectory}/{GameNames[index]}");
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

                    wrapper.RewindEnabled = Settings.RewindEnabled;

                    if (Settings.RewindEnabled)
                        wrapper.PerformRewind = Rewind;

                    double currentTime = _stopwatch.Elapsed.TotalSeconds;
                    double dt = currentTime - _startTime;
                    _startTime = currentTime;

                    double targetFrameTime = FastForward && FastForwardFactor > 0 ? gameFrameTime / FastForwardFactor : gameFrameTime;
                    if ((_accumulator += dt) >= targetFrameTime)
                    {
                        wrapper.RunFrame();
                        _accumulator = 0.0;
                    }

                    Thread.Sleep(1);
                }

                wrapper.StopContent();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
