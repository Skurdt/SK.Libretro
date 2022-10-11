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
using System.Collections.Concurrent;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class BridgeSeparateThread : BridgeMainThread
    {
        public override bool Running
        {
            get
            {
                lock (_lock)
                    return _running;
            }
            protected set
            {
                lock (_lock)
                    _running = value;
            }
        }

        public override bool Paused
        {
            get
            {
                lock (_lock)
                    return _paused;
            }
            protected set
            {
                lock (_lock)
                    _paused = value;
            }
        }

        public override int FastForwardFactor
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

        public override bool FastForward
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

        public override bool Rewind
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

        public override bool InputEnabled
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
                    _threadCommands.Enqueue(new EnableInputThreadCommand(value));
                }
            }
        }

        private readonly ManualResetEventSlim _manualResetEvent = new(false);
        private readonly ConcurrentQueue<IThreadCommand> _threadCommands = new();
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
            lock (_lock)
            {
                base.Dispose();
                _ = _thread?.Join(1000);

                _thread = null;
                _manualResetEvent.Dispose();
            }
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
            _threadCommands.Enqueue(new SaveStateWithScreenshotThreadCommand(_currentStateSlot, TakeScreenshot));

        public override void SaveStateWithoutScreenshot() =>
            _threadCommands.Enqueue(new SaveStateWithoutScreenshotThreadCommand(_currentStateSlot));

        public override void LoadState() =>
            _threadCommands.Enqueue(new LoadStateThreadCommand(_currentStateSlot));

        public override void SaveSRAM() =>
            _threadCommands.Enqueue(new SaveSRAMThreadCommand());

        public override void LoadSRAM() =>
            _threadCommands.Enqueue(new LoadSRAMThreadCommand());

        public override void SetDiskIndex(int index) =>
            _threadCommands.Enqueue(new SetDiskIndexThreadCommand(GamesDirectory, GameNames, index));

        public override void SetControllerPortDevice(uint port, RETRO_DEVICE device) =>
            _threadCommands.Enqueue(new SetControllerPortDeviceThreadCommand(port, device));

        public override void TakeScreenshot(string screenshotPath) =>
            MainThreadDispatcher.Enqueue(() => base.TakeScreenshot(screenshotPath));

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

        protected override void InvokeOnStartedEvent()
        {
            _manualResetEvent.Reset();
            MainThreadDispatcher.Enqueue(() =>
            {
                base.InvokeOnStartedEvent();
                _manualResetEvent.Set();
            });
            _manualResetEvent.Wait();
        }

        protected override IGraphicsProcessor GetGraphicsProcessor(int videoWidth, int videoHeight) =>
            new GraphicsProcessorSeparateThread(videoWidth, videoHeight, SetTexture);

        private void LibretroThread()
        {
            try
            {
                Wrapper wrapper = InitializeWrapper();
                double gameFrameTime = 1.0 / wrapper.Game.VideoFps;

                Running = true;
                while (Running)
                {
                    lock (_lock)
                    {
                        while (_threadCommands.Count > 0)
                            if (_threadCommands.TryDequeue(out IThreadCommand command))
                                command.Execute(wrapper);
                    }

                    if (Paused)
                        _manualResetEvent.Wait();

                    if (!Running)
                        break;

                    wrapper.RewindEnabled = _settings.RewindEnabled;

                    if (_settings.RewindEnabled)
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

                lock (_lock)
                    wrapper.StopContent();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private interface IThreadCommand
        {
            public void Execute(Wrapper wrapper);
        }

        private struct EnableInputThreadCommand : IThreadCommand
        {
            private readonly bool _enabled;

            public EnableInputThreadCommand(bool enable) =>
                _enabled = enable;

            public void Execute(Wrapper wrapper) =>
                wrapper.Input.Enabled = _enabled;
        }

        private struct SaveStateWithScreenshotThreadCommand : IThreadCommand
        {
            private readonly int _currentStateSlot;
            private readonly Action<string> _takeScreenshotFunc;

            public SaveStateWithScreenshotThreadCommand(int currentStateSlot, Action<string> takeScreenshotFunc) =>
                (_currentStateSlot, _takeScreenshotFunc) = (currentStateSlot, takeScreenshotFunc);
            
            public void Execute(Wrapper wrapper)
            {
                if (wrapper.Serialization.SaveState(_currentStateSlot, out string screenshotPath))
                    _takeScreenshotFunc(screenshotPath);
            }
        }

        private struct SaveStateWithoutScreenshotThreadCommand : IThreadCommand
        {
            private readonly int _currentStateSlot;

            public SaveStateWithoutScreenshotThreadCommand(int currentStateSlot) =>
                _currentStateSlot = currentStateSlot;
            
            public void Execute(Wrapper wrapper) =>
                wrapper.Serialization.SaveState(_currentStateSlot);
        }

        private struct LoadStateThreadCommand : IThreadCommand
        {
            private readonly int _currentStateSlot;

            public LoadStateThreadCommand(int currentStateSlot) =>
                _currentStateSlot = currentStateSlot;

            public void Execute(Wrapper wrapper) =>
                wrapper.Serialization.LoadState(_currentStateSlot);
        }

        private struct SaveSRAMThreadCommand : IThreadCommand
        {
            public void Execute(Wrapper wrapper) =>
                wrapper.Serialization.SaveSRAM();
        }

        private struct LoadSRAMThreadCommand : IThreadCommand
        {
            public void Execute(Wrapper wrapper) =>
                wrapper.Serialization.LoadSRAM();
        }

        private struct SetDiskIndexThreadCommand : IThreadCommand
        {
            private readonly string _gamesDirectory;
            private readonly string[] _gameNames;
            private readonly int _index;

            public SetDiskIndexThreadCommand(string gamesDirectory, string[] gameNames, int index) =>
                (_gamesDirectory, _gameNames, _index) = (gamesDirectory, gameNames, index);

            public void Execute(Wrapper wrapper)
            {
                if (_index >= 0 && _index < _gameNames.Length)
                    _ = wrapper.Disk?.SetImageIndexAuto((uint)_index, $"{_gamesDirectory}/{_gameNames[_index]}");
            }
        }

        private struct SetControllerPortDeviceThreadCommand : IThreadCommand
        {
            private readonly uint _port;
            private readonly RETRO_DEVICE _device;
            
            public SetControllerPortDeviceThreadCommand(uint port, RETRO_DEVICE device) =>
                (_port, _device) = (port, device);
            
            public void Execute(Wrapper wrapper) =>
                wrapper.Core.retro_set_controller_port_device(_port, _device);
        }
    }
}
