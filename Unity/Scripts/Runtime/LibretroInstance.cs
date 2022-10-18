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
    [DisallowMultipleComponent, DefaultExecutionOrder(-2)]
    public sealed class LibretroInstance : MonoBehaviour
    {
        [field: SerializeField] public bool UseSeparateThread { get; private set; }
        [field: SerializeField] public Camera Camera { get; private set; }
        [field: SerializeField, Layer] public int LightgunRaycastLayer { get; private set; }
        [field: SerializeField] public Renderer Renderer { get; private set; }
        [field: SerializeField] public Collider Collider { get; private set; }
        [field: SerializeField] public Transform Viewer { get; private set; }
        [field: SerializeField] public BridgeSettings Settings { get; private set; }
        [field: SerializeField] public bool AllowGLCoreInEditor { get; private set; }
        [field: SerializeField] public string CoreName { get; private set; }
        [field: SerializeField] public string GamesDirectory { get; private set; }
        [field: SerializeField] public string[] GameNames { get; private set; }

        public Action OnInstanceStarted;
        public Action OnInstanceStopped;

        public bool Running => _libretro.Running;
        public ControllersMap ControllersMap => _libretro.ControllersMap;
        public bool InputEnabled { get => _libretro.InputEnabled; set => _libretro.InputEnabled = value; }
        public bool FastForward { get => _libretro.FastForward; set => _libretro.FastForward = value; }
        public bool Rewind { get => _libretro.Rewind; set => _libretro.Rewind = value; }

        private Instance _libretro;

        private void OnDisable() => StopContent();

        private void Update()
        {
            if (_libretro is null || UseSeparateThread)
                return;

            _libretro.TickMainThread();
        }

        public void Initialize(string coreName, string gamesDirectory, params string[] gameNames)
        {
            CoreName       = coreName;
            GamesDirectory = gamesDirectory;
            GameNames      = gameNames;
        }

        public void DeInitialize()
        {
            CoreName       = null;
            GamesDirectory = null;
            GameNames      = null;
        }

        public void StartContent()
        {
            _libretro = UseSeparateThread
                      ? new InstanceSeparateThread(this)
                      : new InstanceMainThread(this);
            _libretro.SetContent(CoreName, GamesDirectory, GameNames);
            _libretro.StartContent(OnInstanceStarted, OnInstanceStopped);
        }

        public void PauseContent() => _libretro.PauseContent();

        public void ResumeContent() => _libretro.ResumeContent();

        public void ResetContent() => _libretro.ResetContent();

        public void StopContent()
        {
            _libretro?.StopContent();
            _libretro?.Dispose();
            _libretro = null;
        }

        public void SetControllerPortDevice(uint port, RETRO_DEVICE id) => _libretro.SetControllerPortDevice(port, id);

        public void SaveStateWithScreenshot() => _libretro.SaveStateWithScreenshot();

        public void LoadState() => _libretro.LoadState();

        public void SaveSRAM() => _libretro.SaveSRAM();

        public void LoadSRAM() => _libretro.LoadSRAM();

        public void SetDiskIndex(int index) => _libretro.SetDiskIndex(index);
    }
}
