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
        [field: SerializeField, Layer] public int LightgunRaycastLayer { get; set; }
        [field: SerializeField] public Renderer Renderer               { get; set; }
        [field: SerializeField] public Collider Collider               { get; set; }
        [field: SerializeField] public InstanceSettings Settings       { get; set; }
        [field: SerializeField] public string CoreName                 { get; set; }
        [field: SerializeField] public string GamesDirectory           { get; set; }
        [field: SerializeField] public string[] GameNames              { get; set; }

        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _lightgunRaycastSource;

        public Action OnInstanceStarted { get; set; }
        public Action OnInstanceStopped { get; set; }

        public bool Running => _bridge is not null && _bridge.Running;

        public ControllersMap ControllersMap => _bridge.ControllersMap;

        public bool InputEnabled
        {
            get => _bridge is not null && _bridge.InputEnabled;
            set
            {
                if (_bridge is not null)
                    _bridge.InputEnabled = value;
            }
        }

        public bool DiskHandlerEnabled => _bridge.DiskHandlerEnabled;

        public bool FastForward
        {
            get => _bridge.FastForward;
            set => _bridge.FastForward = value;
        }

        public bool Rewind
        {
            get => _bridge.Rewind;
            set => _bridge.Rewind = value;
        }

        public (Options, Options) Options => _bridge.Options;

        public Ray LightgunRay { get; private set; }

        private readonly Dictionary<int, uint> _inputDevices = new() {
            { 0, (uint)RETRO_DEVICE.JOYPAD },
            { 1, (uint)RETRO_DEVICE.JOYPAD },
            { 2, (uint)RETRO_DEVICE.JOYPAD },
            { 3, (uint)RETRO_DEVICE.JOYPAD }
        };

        private Bridge _bridge;

        private void OnDisable() => StopContent();

        private void Update()
        {
            if (_bridge is null || !_camera)
            {
                LightgunRay = default;
                return;
            }

            Vector3 playerPosition = _camera.transform.position;
            _bridge.SetPlayerPosition(playerPosition.x, playerPosition.y, playerPosition.z, Vector3.Distance(playerPosition, transform.position), _camera.transform.forward.x, _camera.transform.forward.z);

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

            _bridge = new();
            _bridge.StartContent(this);
        }

        public void PauseContent() => _bridge?.PauseContent();

        public void ResumeContent() => _bridge?.ResumeContent();

        public void ResetContent() => _bridge?.ResetContent();

        public void StopContent() => _bridge?.StopContent();

        public uint GetControllerPortDevice(int port) => _inputDevices.TryGetValue(port, out uint device) ? device : (uint)RETRO_DEVICE.JOYPAD;

        public void SetControllerPortDevice(uint port, uint id)
        {
            _inputDevices[(int)port] = id;
            _bridge.SetControllerPortDevice(port, id);
        }

        public void SetStateSlot(int slot) => _bridge.SetStateSlot(slot);

        public void SaveStateWithScreenshot() => _bridge.SaveStateWithScreenshot();

        public void LoadState() => _bridge.LoadState();

        public void SetDiskIndex(int index) => _bridge.SetDiskIndex(index);

        public void SaveSRAM() => _bridge.SaveSRAM();

        public void LoadSRAM() => _bridge.LoadSRAM();

        public void SaveOptions(bool global) => _bridge.SaveOptions(global);

        public ReadOnlySpan<byte> GetSaveMemory() => _bridge.GetSaveMemory();

        public ReadOnlySpan<byte> GetRtcMemory() => _bridge.GetRtcMemory();

        public ReadOnlySpan<byte> GetSystemMemory() => _bridge.GetSystemMemory();

        public ReadOnlySpan<byte> GetVideoMemory() => _bridge.GetVideoMemory();
    }
}
