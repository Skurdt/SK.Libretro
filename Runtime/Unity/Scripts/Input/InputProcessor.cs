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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    [RequireComponent(typeof(PlayerInputManager))]
    public sealed class InputProcessor : MonoBehaviour, IInputProcessor
    {
        public bool AnalogDirectionsToDigital { get; set; }

        private readonly Dictionary<int, PlayerInputProcessor> _controls = new Dictionary<int, PlayerInputProcessor>();

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Input Callback")]
        private void OnPlayerJoined(PlayerInput player)
        {
            Utilities.Logger.LogInfo($"Player #{player.playerIndex} joined ({player.currentControlScheme}).");
            if (!_controls.ContainsKey(player.playerIndex))
            {
                PlayerInputProcessor processor = player.GetComponent<PlayerInputProcessor>();
                if (processor != null)
                {
                    processor.AnalogDirectionsToDigital = AnalogDirectionsToDigital;
                    _controls.Add(player.playerIndex, processor);
                }
            }
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Input Callback")]
        private void OnPlayerLeft(PlayerInput player)
        {
            Utilities.Logger.LogInfo($"Player #{player.playerIndex} left ({player.currentControlScheme}).");
            if (_controls.ContainsKey(player.playerIndex))
                _ = _controls.Remove(player.playerIndex);
        }

        public bool JoypadButton(int port, int button) => _controls.ContainsKey(port) && _controls[port].JoypadButtons[button];

        public float MouseDeltaX(int port)            => _controls.ContainsKey(port) ?  _controls[port].MousePositionDelta.x : 0f;
        public float MouseDeltaY(int port)            => _controls.ContainsKey(port) ? -_controls[port].MousePositionDelta.y : 0f;
        public float MouseWheelDeltaX(int port)       => _controls.ContainsKey(port) ?  _controls[port].MouseWheelDelta.x    : 0f;
        public float MouseWheelDeltaY(int port)       => _controls.ContainsKey(port) ?  _controls[port].MouseWheelDelta.y    : 0f;
        public bool MouseButton(int port, int button) => _controls.ContainsKey(port) && _controls[port].MouseButtons[button];

        public bool KeyboardKey(int port, int key) => _controls.ContainsKey(port) && Input.GetKey((KeyCode)key);

        public float AnalogLeftValueX(int port)  => _controls.ContainsKey(port) ?  _controls[port].AnalogLeft.x  : 0f;
        public float AnalogLeftValueY(int port)  => _controls.ContainsKey(port) ? -_controls[port].AnalogLeft.y  : 0f;
        public float AnalogRightValueX(int port) => _controls.ContainsKey(port) ?  _controls[port].AnalogRight.x : 0f;
        public float AnalogRightValueY(int port) => _controls.ContainsKey(port) ? -_controls[port].AnalogRight.y : 0f;
    }
}
