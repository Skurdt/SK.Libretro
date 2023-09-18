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
using UnityEngine;

namespace SK.Libretro.Unity
{
    [DisallowMultipleComponent]
    public sealed class LibretroInstance : MonoBehaviour
    {
        [field: SerializeField] public Camera Camera { get; set; }
        [field: SerializeField, Layer] public int LightgunRaycastLayer { get; set; }
        [field: SerializeField] public Renderer Renderer { get; set; }
        [field: SerializeField] public Collider Collider { get; set; }
        [field: SerializeField] public Transform Viewer { get; set; }
        [field: SerializeField] public InstanceSettings Settings { get; set; }
        [field: SerializeField] public string TempDirectory { get; set; }
        [field: SerializeField] public string LibretroDirectory { get; set; }
        [field: SerializeField] public string CoreName { get; set; }
        [field: SerializeField] public string GamesDirectory { get; set; }
        [field: SerializeField] public string[] GameNames { get; set; }

        public event Action OnInstanceStarted;
        public event Action OnInstanceStopped;

        public bool Running => _bridge is not null && _bridge.Running;

        public ControllersMap ControllersMap => _bridge is not null ? _bridge.ControllersMap : ControllersMap.Empty;

        public bool InputEnabled
        {
            get => _bridge is not null && _bridge.InputEnabled;
            set
            {
                if (_bridge is not null)
                    _bridge.InputEnabled = value;
            }
        }

        public bool DiskHandlerEnabled => _bridge is not null && _bridge.DiskHandlerEnabled;

        public bool FastForward
        {
            get => _bridge is not null && _bridge.FastForward;
            set
            {
                if (_bridge is not null)
                    _bridge.FastForward = value;
            }
        }

        public bool Rewind
        {
            get => _bridge is not null && _bridge.Rewind;
            set
            {
                if (_bridge is not null)
                    _bridge.Rewind = value;
            }
        }

        public (Options, Options) Options => _bridge is not null ? _bridge.Options : default;

        public byte[] SaveMemory => _bridge is not null ? _bridge.SaveMemory : Array.Empty<byte>();

        public byte[] RtcMemory => _bridge is not null ? _bridge.RtcMemory : Array.Empty<byte>();

        public byte[] SystemMemory => _bridge is not null ? _bridge.SystemMemory : Array.Empty<byte>();

        public byte[] VideoMemory => _bridge is not null ? _bridge.VideoMemory : Array.Empty<byte>();

        private Bridge _bridge;

        private void OnDisable() => StopContent();

        public void Initialize(string libretroDirectory, string tempDirectory, string coreName, string gamesDirectory, params string[] gameNames)
        {
            LibretroDirectory = libretroDirectory;
            TempDirectory     = tempDirectory;
            CoreName          = coreName;
            GamesDirectory    = gamesDirectory;
            GameNames         = gameNames;
        }

        public void DeInitialize()
        {
            LibretroDirectory = null;
            TempDirectory     = null;
            CoreName          = null;
            GamesDirectory    = null;
            GameNames         = null;
        }

        public void StartContent()
        {
            if (Running)
                return;

            _bridge = new(this);
            _bridge.StartContent(LibretroDirectory, TempDirectory, CoreName, GamesDirectory, GameNames, OnInstanceStarted, OnInstanceStopped);
        }

        public void PauseContent() => _bridge?.PauseContent();

        public void ResumeContent() => _bridge?.ResumeContent();

        public void ResetContent() => _bridge?.ResetContent();

        public void StopContent()
        {
            _bridge?.StopContent();
            _bridge = null;
        }

        public void SetControllerPortDevice(uint port, RETRO_DEVICE id) => _bridge?.SetControllerPortDevice(port, id);

        public void SetStateSlot(int slot) => _bridge?.SetStateSlot(slot);

        public void SaveStateWithScreenshot() => _bridge?.SaveStateWithScreenshot();

        public void LoadState() => _bridge?.LoadState();

        public void SetDiskIndex(int index) => _bridge?.SetDiskIndex(index);

        public void SaveSRAM() => _bridge?.SaveSRAM();

        public void LoadSRAM() => _bridge?.LoadSRAM();

        public void SaveOptions(bool global) => _bridge?.SaveOptions(global);
    }
}
