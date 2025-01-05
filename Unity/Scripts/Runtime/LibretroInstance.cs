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
        [field: SerializeField] public Camera Camera { get; private set; }
        [field: SerializeField, Layer] public int LightgunRaycastLayer { get; set; }
        [field: SerializeField] public GameObject LightgunSource { get; set; }
        [field: SerializeField] public Renderer Renderer { get; set; }
        [field: SerializeField] public Collider Collider { get; set; }
        [field: SerializeField] public Transform Viewer { get; set; }
        [field: SerializeField] public InstanceSettings Settings { get; private set; }
        [field: SerializeField] public string CoreName { get; private set; }
        [field: SerializeField] public string GamesDirectory { get; private set; }
        [field: SerializeField] public string[] GameNames { get; private set; }

        public event Action OnInstanceStarted;
        public event Action OnInstanceStopped;

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

        public byte[] SaveMemory => Bridge.Instance.SaveMemory;

        public byte[] RtcMemory => Bridge.Instance.RtcMemory;

        public byte[] SystemMemory => Bridge.Instance.SystemMemory;

        public byte[] VideoMemory => Bridge.Instance.VideoMemory;

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
            if (!Camera)
            {
                LightgunRay = default;
                return;
            }

            if (!LightgunSource || !LightgunSource.activeSelf)
            {
                LightgunRay = Mouse.current is not null
                            ? Camera.ScreenPointToRay(Mouse.current.position.value)
                            : default;
                return;
            }

            Vector3 controllerPosition = LightgunSource.transform.position;
            Vector3 controllerDirection = LightgunSource.transform.forward;
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

            Bridge.Instance.StartContent(this, CoreName, GamesDirectory, GameNames, OnInstanceStarted, OnInstanceStopped);
        }

        public void PauseContent() => Bridge.Instance.PauseContent();

        public void ResumeContent() => Bridge.Instance.ResumeContent();

        public void ResetContent() => Bridge.Instance.ResetContent();

        public void StopContent() => Bridge.Instance.StopContent();

        public void AddPlayer(int index) => Bridge.Instance.AddPlayer(index);

        public void RemovePlayer(int index) => Bridge.Instance.RemovePlayer(index);

        public uint GetControllerPortDevice(int port) => _inputDevices.ContainsKey(port) ? _inputDevices[port] : (uint)RETRO_DEVICE.JOYPAD;

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
    }
}
