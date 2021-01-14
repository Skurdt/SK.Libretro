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

using UnityEngine;
using UnityEngine.InputSystem;
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro.Unity
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerInputProcessor : MonoBehaviour
    {
        public bool AnalogDirectionsToDigital { get; set; }

        public readonly bool[] JoypadButtons = new bool[NUM_JOYPAD_BUTTONS];

        public Vector2 MousePositionDelta { get; private set; } = Vector2.zero;
        public Vector2 MouseWheelDelta    { get; private set; } = Vector2.zero;
        public readonly bool[] MouseButtons = new bool[NUM_MOUSE_BUTTONS];

        public Vector2 AnalogLeft  { get; private set; } = Vector2.zero;
        public Vector2 AnalogRight { get; private set; } = Vector2.zero;

        private const int NUM_JOYPAD_BUTTONS = 16;
        private const int NUM_MOUSE_BUTTONS  = 5;

#pragma warning disable IDE0051 // Remove unused private members, Callbacks for the PlayerInput component
        private void OnDeviceLost(PlayerInput player) => Utilities.Logger.LogInfo($"Player #{player.playerIndex} device lost ({player.devices.Count}).");

        private void OnDeviceRegained(PlayerInput player) => Utilities.Logger.LogInfo($"Player #{player.playerIndex} device regained ({player.devices.Count}).");

        private void OnControlsChanged(PlayerInput player) => Utilities.Logger.LogInfo($"Player #{player.playerIndex} controls changed ({player.devices.Count}).");

        private void OnJoypadDirections(InputValue value)
        {
            Vector2 vec = value.Get<Vector2>();
            JoypadButtons[RETRO_DEVICE_ID_JOYPAD_UP]    = vec.y >=  0.2f;
            JoypadButtons[RETRO_DEVICE_ID_JOYPAD_DOWN]  = vec.y <= -0.2f;
            JoypadButtons[RETRO_DEVICE_ID_JOYPAD_LEFT]  = vec.x <= -0.2f;
            JoypadButtons[RETRO_DEVICE_ID_JOYPAD_RIGHT] = vec.x >=  0.2f;
        }

        private void OnAnalogLeft(InputValue value)
        {
            AnalogLeft = value.Get<Vector2>();
            if (AnalogDirectionsToDigital)
                OnJoypadDirections(value);
        }

        private void OnAnalogRight(InputValue value) => AnalogRight = value.Get<Vector2>();

        private void OnJoypadStartButton(InputValue value)  => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_START]  = value.isPressed;
        private void OnJoypadSelectButton(InputValue value) => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_SELECT] = value.isPressed;
        private void OnJoypadAButton(InputValue value)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_A]      = value.isPressed;
        private void OnJoypadBButton(InputValue value)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_B]      = value.isPressed;
        private void OnJoypadXButton(InputValue value)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_X]      = value.isPressed;
        private void OnJoypadYButton(InputValue value)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_Y]      = value.isPressed;
        private void OnJoypadLButton(InputValue value)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_L]      = value.isPressed;
        private void OnJoypadRButton(InputValue value)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_R]      = value.isPressed;
        private void OnJoypadL2Button(InputValue value)     => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_L2]     = value.isPressed;
        private void OnJoypadR2Button(InputValue value)     => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_R2]     = value.isPressed;
        private void OnJoypadL3Button(InputValue value)     => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_L3]     = value.isPressed;
        private void OnJoypadR3Button(InputValue value)     => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_R3]     = value.isPressed;

        private void OnMousePositionDelta(InputValue value) => MousePositionDelta = value.Get<Vector2>();
        private void OnMouseWheelDelta(InputValue value)    => MouseWheelDelta    = value.Get<Vector2>();
        private void OnMouseLeftButton(InputValue value)    => MouseButtons[0]    = value.isPressed;
        private void OnMouseRightButton(InputValue value)   => MouseButtons[1]    = value.isPressed;
        private void OnMouseMiddleButton(InputValue value)  => MouseButtons[2]    = value.isPressed;
        private void OnMouseForwardButton(InputValue value) => MouseButtons[3]    = value.isPressed;
        private void OnMouseBackButton(InputValue value)    => MouseButtons[4]    = value.isPressed;
#pragma warning restore IDE0051 // Remove unused private members
    }
}
