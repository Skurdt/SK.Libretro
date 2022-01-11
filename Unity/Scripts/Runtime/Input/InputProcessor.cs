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
    internal sealed class InputProcessor : MonoBehaviour, IInputProcessor
    {
        public bool AnalogToDigital { get; set; }

        private readonly Dictionary<int, PlayerInputProcessor> _controls = new();

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Input Callback")]
        private void OnPlayerJoined(PlayerInput player)
        {
            player.actions.Enable();

            Logger.Instance.LogInfo($"Player #{player.playerIndex} joined ({player.currentControlScheme}).");
            if (!_controls.ContainsKey(player.playerIndex))
            {
                PlayerInputProcessor processor = player.GetComponent<PlayerInputProcessor>();
                if (processor != null)
                {
                    processor.AnalogDirectionsToDigital = AnalogToDigital;
                    _controls.Add(player.playerIndex, processor);
                }
            }
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Input Callback")]
        private void OnPlayerLeft(PlayerInput player)
        {
            Logger.Instance.LogInfo($"Player #{player.playerIndex} left ({player.currentControlScheme}).");
            if (_controls.ContainsKey(player.playerIndex))
                _ = _controls.Remove(player.playerIndex);
        }

        public short JoypadButton(int port, uint button) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.JoypadButton(button) : (short)0;
        public short JoypadButtons(int port)             => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.JoypadButtons : (short)0;

        public short MouseX(int port)                   => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.MouseX : (short)0;
        public short MouseY(int port)                   => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? (short)-processor.MouseY : (short)0;
        public short MouseWheel(int port)               => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.MouseWheel : (short)0;
        public short MouseButton(int port, uint button) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.MouseButton(button) : (short)0;

        public short KeyboardKey(int port, uint key) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.KeyboardKey(key) : (short)0;

        public short LightgunX(int port)                   => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.LightgunX : (short)0;
        public short LightgunY(int port)                   => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.LightgunY : (short)0;
        public bool LightgunIsOffscreen(int port)          => _controls.TryGetValue(port, out PlayerInputProcessor processor) && processor.LightgunIsOffscreen;
        public short LightgunButton(int port, uint button) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.LightgunButton(button) : (short)0;

        public short AnalogLeftX(int port)  => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.AnalogLeftX : (short)0;
        public short AnalogLeftY(int port)  => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? (short)-processor.AnalogLeftY : (short)0;
        public short AnalogRightX(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? processor.AnalogRightX : (short)0;
        public short AnalogRightY(int port) => _controls.TryGetValue(port, out PlayerInputProcessor processor) ? (short)-processor.AnalogRightY : (short)0;
    }
}
