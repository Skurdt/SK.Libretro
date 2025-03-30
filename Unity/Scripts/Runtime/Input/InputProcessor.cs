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
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    [RequireComponent(typeof(PlayerInputManager))]
    internal sealed class InputProcessor : MonoBehaviour, IInputProcessor
    {
        public LeftStickBehaviour LeftStickBehaviour { get; set; }

        private readonly ConcurrentDictionary<int, PlayerInputProcessor> _controls = new();

        private PlayerInputManager _playerInputManager;

        private void Awake() => _playerInputManager = GetComponent<PlayerInputManager>();

        private void OnEnable()
        {
            _playerInputManager.onPlayerJoined += OnPlayerJoined;
            _playerInputManager.onPlayerLeft   += OnPlayerLeft;
        }

        private void OnDisable()
        {
            _playerInputManager.onPlayerJoined -= OnPlayerJoined;
            _playerInputManager.onPlayerLeft   -= OnPlayerLeft;
        }

        public short JoypadButton(int port, RETRO_DEVICE_ID_JOYPAD button) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.JoypadHandler.IsButtonDown(button) : (short)0;
        public short JoypadButtons(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.JoypadHandler.Buttons: (short)0;

        public short MouseX(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.MouseHandler.DeltaX : (short)0;
        public short MouseY(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? (short)-processor.MouseHandler.DeltaY : (short)0;
        public short MouseWheel(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.MouseHandler.Wheel : (short)0;
        public short MouseButton(int port, RETRO_DEVICE_ID_MOUSE button) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.MouseHandler.IsButtonDown(button) : (short)0;

        public short KeyboardKey(int port, retro_key key) => _controls.TryGetValue(0, out PlayerInputProcessor processor) ? processor.KeyboardHandler.IsKeyDown(key) : (short)0;

        public short LightgunX(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.LightgunHandler.X : (short)0;
        public short LightgunY(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.LightgunHandler.Y : (short)0;
        public bool LightgunIsOffscreen(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) && !processor.LightgunHandler.InScreen;
        public short LightgunButton(int port, RETRO_DEVICE_ID_LIGHTGUN button) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.LightgunHandler.IsButtonDown(button) : (short)0;

        public short AnalogLeftX(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.AnalogHandler.LeftX : (short)0;
        public short AnalogLeftY(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? (short)-processor.AnalogHandler.LeftY : (short)0;
        public short AnalogRightX(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.AnalogHandler.RightX : (short)0;
        public short AnalogRightY(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? (short)-processor.AnalogHandler.RightY : (short)0;

        public short PointerX(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.PointerHandler.X : (short)0;
        public short PointerY(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.PointerHandler.Y : (short)0;
        public short PointerPressed(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.PointerHandler.Pressed : (short)0;
        public short PointerCount(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.PointerHandler.Count : (short)0;

        public bool SetRumbleState(int port, retro_rumble_effect effect, ushort strength) => _controls.TryGetValue(port, out PlayerInputProcessor processor) && processor.SetRumbleState(effect, strength);

        private void OnPlayerJoined(PlayerInput playerInput)
        {
            playerInput.onDeviceLost     += OnDeviceLost;
            playerInput.onDeviceRegained += OnDeviceRegained;

            if (_controls.ContainsKey(playerInput.playerIndex))
                return;

            if (!playerInput.TryGetComponent(out PlayerInputProcessor processor))
                return;

            processor.Init(LeftStickBehaviour);

            _ = _controls.TryAdd(playerInput.playerIndex, processor);
            Debug.Log($"Player #{playerInput.playerIndex} joined ({playerInput.currentControlScheme}).");
        }

        private void OnPlayerLeft(PlayerInput playerInput)
        {
            playerInput.onDeviceLost     -= OnDeviceLost;
            playerInput.onDeviceRegained -= OnDeviceRegained;

            if (!_controls.TryRemove(playerInput.playerIndex, out _))
                return;

            if (!_controls.TryGetValue(playerInput.playerIndex, out PlayerInputProcessor processor))
                return;

            Debug.Log($"Player #{playerInput.playerIndex} left ({playerInput.currentControlScheme}).");
        }

        private static void OnDeviceLost(PlayerInput playerInput) => Debug.Log($"Player {playerInput.playerIndex} device lost ({string.Join(", ", playerInput.devices)})");

        private static void OnDeviceRegained(PlayerInput playerInput) => Debug.Log($"Player {playerInput.playerIndex} device regained ({string.Join(", ", playerInput.devices)})");
    }
}
