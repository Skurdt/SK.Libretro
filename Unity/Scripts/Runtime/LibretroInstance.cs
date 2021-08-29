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
using UnityEngine;

namespace SK.Libretro.Unity
{
    [DisallowMultipleComponent, DefaultExecutionOrder(-2)]
    public sealed class LibretroInstance : MonoBehaviour
    {
        [field: SerializeField] public Camera Camera { get; private set; }
        [field: SerializeField, Layer] public int RaycastLayer { get; private set; }
        [field: SerializeField] public Renderer Renderer { get; private set; }
        [field: SerializeField] public Collider Collider { get; private set; }
        [field: SerializeField] public Transform Viewer { get; private set; }
        [field: SerializeField] public BridgeSettings Settings { get; private set; }
        [field: SerializeField] public bool AllowGLCoreInEditor { get; private set; }

        public Action OnInstanceStarted;
        public Action OnInstanceStopped;

        public string CoreName;
        public string GamesDirectory;
        public string[] GameNames;

        public bool Running => _libretro.Running;
        public ControllersMap ControllersMap => _libretro.ControllersMap;
        public bool InputEnabled { get => _libretro.InputEnabled; set => _libretro.InputEnabled = value; }
        public bool FastForward { get => _libretro.FastForward; set => _libretro.FastForward = value; }
        public bool Rewind { get => _libretro.Rewind; set => _libretro.Rewind = value; }

        private Bridge _libretro;

        private void Awake()
        {
            if (Camera == null)
                Camera = Camera.main;

            if (Renderer == null)
                Renderer = GetComponent<Renderer>();

            if (Collider == null)
                Collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _libretro = new Bridge(this);
            SetContent();
            //StartContent();
        }

        private void OnApplicationQuit()
        {
            _libretro?.Dispose();
            _libretro = null;
        }

        private void OnDisable()
        {
            _libretro?.Dispose();
            _libretro = null;
        }

        public void SetContent() => _libretro.SetContent(CoreName, GamesDirectory, GameNames);
        public void StartContent()
        {
            SetContent();
            _libretro.StartContent(OnInstanceStarted, OnInstanceStopped);
        }

        public void PauseContent() => _libretro.PauseContent();
        public void ResumeContent() => _libretro.ResumeContent();
        public void StopContent() => _libretro.StopContent();
        public void SetControllerPortDevice(uint port, uint id) => _libretro.SetControllerPortDevice(port, id);
        public void SaveStateWithScreenshot() => _libretro.SaveStateWithScreenshot();
        public void LoadState() => _libretro.LoadState();
        public void SaveSRAM() => _libretro.SaveSRAM();
        public void LoadSRAM() => _libretro.LoadSRAM();
        public void SetDiskIndex(int index) => _libretro.SetDiskIndex(index);
        public byte GetMemoryByte(int address) => _libretro.GetMemoryByte(address);
    }
}
