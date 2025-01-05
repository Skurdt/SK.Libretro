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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    [DisallowMultipleComponent]
    public sealed class LibretroInstance : MonoBehaviour
    {
        [field: SerializeField, Layer] public int LightgunRaycastLayer { get; private set; }
        [field: SerializeField] public Renderer Renderer               { get; private set; }
        [field: SerializeField] public Collider Collider               { get; private set; }
        [field: SerializeField] public InstanceSettings Settings       { get; private set; }
        [field: SerializeField] public string CoreName                 { get; private set; }
        [field: SerializeField] public string GamesDirectory           { get; private set; }
        [field: SerializeField] public string[] GameNames              { get; private set; }

        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _lightgunRaycastSource;

        public Action OnInstanceStarted { get; set; }
        public Action OnInstanceStopped { get; set; }

        public bool Running => Bridge.Instance.Running;

        public ControllersMap ControllersMap => Bridge.Instance.ControllersMap;

        public bool InputEnabled
        {
            get => Bridge.Instance.InputEnabled;
            set => Bridge.Instance.InputEnabled = value;
        }

        public bool DiskHandlerEnabled => Bridge.Instance.DiskHandlerEnabled;

        public bool FastForward
        {
            get => Bridge.Instance.FastForward;
            set => Bridge.Instance.FastForward = value;
        }

        public bool Rewind
        {
            get => Bridge.Instance.Rewind;
            set => Bridge.Instance.Rewind = value;
        }

        public (Options, Options) Options => Bridge.Instance.Options;

        public Ray LightgunRay { get; private set; }

        private readonly Dictionary<int, uint> _inputDevices = new() {
            { 0, (uint)RETRO_DEVICE.JOYPAD },
            { 1, (uint)RETRO_DEVICE.JOYPAD },
            { 2, (uint)RETRO_DEVICE.JOYPAD },
            { 3, (uint)RETRO_DEVICE.JOYPAD }
        };

        private void OnDisable() => StopContent();

        private void Update()
        {
            if (!_camera)
            {
                LightgunRay = default;
                return;
            }

            if (!_lightgunRaycastSource || !_lightgunRaycastSource.activeSelf)
            {
                LightgunRay = Mouse.current is not null
                            ? _camera.ScreenPointToRay(Mouse.current.position.value)
                            : default;
                return;
            }

            Vector3 controllerPosition = _lightgunRaycastSource.transform.position;
            Vector3 controllerDirection = _lightgunRaycastSource.transform.forward;
            LightgunRay = new(controllerPosition, controllerDirection);
        }

        public void Initialize(string coreName, string gamesDirectory, params string[] gameNames)
        {
            CoreName       = coreName;
            GamesDirectory = gamesDirectory;
            GameNames      = gameNames;

            Settings ??= new();
        }

        public void DeInitialize()
        {
            CoreName       = null;
            GamesDirectory = null;
            GameNames      = null;
        }

        public void StartContent()
        {
            if (Running)
                return;

            if (!Renderer)
                Renderer = GetComponent<Renderer>();

            if (!Collider)
                Collider = GetComponent<Collider>();

            Bridge.Instance.StartContent(this);
        }

        public void PauseContent() => Bridge.Instance.PauseContent();

        public void ResumeContent() => Bridge.Instance.ResumeContent();

        public void ResetContent() => Bridge.Instance.ResetContent();

        public void StopContent() => Bridge.Instance.StopContent();

        public void AddPlayer(int port, int device) => Bridge.Instance.AddPlayer(port, device);

        public void RemovePlayer(int port) => Bridge.Instance.RemovePlayer(port);

        public uint GetControllerPortDevice(int port) => _inputDevices.TryGetValue(port, out uint device) ? device : (uint)RETRO_DEVICE.JOYPAD;

        public void SetControllerPortDevice(uint port, uint id)
        {
            _inputDevices[(int)port] = id;
            Bridge.Instance.SetControllerPortDevice(port, id);
        }

        public void SetStateSlot(int slot) => Bridge.Instance.SetStateSlot(slot);

        public void SaveStateWithScreenshot() => Bridge.Instance.SaveStateWithScreenshot();

        public void LoadState() => Bridge.Instance.LoadState();

        public void SetDiskIndex(int index) => Bridge.Instance.SetDiskIndex(index);

        public void SaveSRAM() => Bridge.Instance.SaveSRAM();

        public void LoadSRAM() => Bridge.Instance.LoadSRAM();

        public void SaveOptions(bool global) => Bridge.Instance.SaveOptions(global);

        public ReadOnlySpan<byte> GetSaveMemory() => Bridge.Instance.GetSaveMemory();

        public ReadOnlySpan<byte> GetRtcMemory() => Bridge.Instance.GetRtcMemory();

        public ReadOnlySpan<byte> GetSystemMemory() => Bridge.Instance.GetSystemMemory();

        public ReadOnlySpan<byte> GetVideoMemory() => Bridge.Instance.GetVideoMemory();
    }
}
