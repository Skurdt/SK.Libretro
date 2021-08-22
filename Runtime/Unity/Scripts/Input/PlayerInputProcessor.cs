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
        private const int NUM_JOYPAD_BUTTONS = 16;
        private const int NUM_KEYBOARD_KEYS  = 324;
        private const int NUM_MOUSE_BUTTONS  = 5;

        public bool AnalogDirectionsToDigital { get; set; }

        public readonly bool[] JoypadButtons = new bool[NUM_JOYPAD_BUTTONS];
        public readonly bool[] KeyboardKeys  = new bool[NUM_KEYBOARD_KEYS];
        public readonly bool[] MouseButtons = new bool[NUM_MOUSE_BUTTONS];

        public Vector2 MousePositionDelta { get; private set; } = Vector2.zero;
        public Vector2 MouseWheelDelta    { get; private set; } = Vector2.zero;

        public Vector2 AnalogLeft  { get; private set; } = Vector2.zero;
        public Vector2 AnalogRight { get; private set; } = Vector2.zero;

        private void Update()
        {
            for (int i = 0; i < KeyboardKeys.Length; ++i)
                KeyboardKeys[i] = Input.GetKey((KeyCode)i);
        }

        public void OnDeviceLost(PlayerInput player) => Utilities.Logger.Instance.LogInfo($"Player #{player.playerIndex} device lost ({player.devices.Count}).");

        public void OnDeviceRegained(PlayerInput player) => Utilities.Logger.Instance.LogInfo($"Player #{player.playerIndex} device regained ({player.devices.Count}).");

        public void OnControlsChanged(PlayerInput player) => Utilities.Logger.Instance.LogInfo($"Player #{player.playerIndex} controls changed ({player.devices.Count}).");

        public void OnDPad(InputAction.CallbackContext context) => HandleDPad(GetVector2(context));

        public void OnAnalogLeft(InputAction.CallbackContext context)
        {
            AnalogLeft = GetVector2(context);
            if (AnalogDirectionsToDigital)
                HandleDPad(AnalogLeft);
        }

        public void OnAnalogRight(InputAction.CallbackContext context) => AnalogRight = GetVector2(context);

        public void OnJoypadStartButton(InputAction.CallbackContext context)  => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_START]  = context.performed;
        public void OnJoypadSelectButton(InputAction.CallbackContext context) => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_SELECT] = context.performed;
        public void OnJoypadAButton(InputAction.CallbackContext context)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_A]      = context.performed;
        public void OnJoypadBButton(InputAction.CallbackContext context)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_B]      = context.performed;
        public void OnJoypadXButton(InputAction.CallbackContext context)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_X]      = context.performed;
        public void OnJoypadYButton(InputAction.CallbackContext context)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_Y]      = context.performed;
        public void OnJoypadLButton(InputAction.CallbackContext context)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_L]      = context.performed;
        public void OnJoypadRButton(InputAction.CallbackContext context)      => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_R]      = context.performed;
        public void OnJoypadL2Button(InputAction.CallbackContext context)     => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_L2]     = context.performed;
        public void OnJoypadR2Button(InputAction.CallbackContext context)     => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_R2]     = context.performed;
        public void OnJoypadL3Button(InputAction.CallbackContext context)     => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_L3]     = context.performed;
        public void OnJoypadR3Button(InputAction.CallbackContext context)     => JoypadButtons[RETRO_DEVICE_ID_JOYPAD_R3]     = context.performed;

        public void OnMousePositionDelta(InputAction.CallbackContext context) => MousePositionDelta = GetVector2(context);
        public void OnMouseWheelDelta(InputAction.CallbackContext context)    => MouseWheelDelta    = GetVector2(context);
        public void OnMouseLeftButton(InputAction.CallbackContext context)    => MouseButtons[0]    = context.performed;
        public void OnMouseRightButton(InputAction.CallbackContext context)   => MouseButtons[1]    = context.performed;
        public void OnMouseMiddleButton(InputAction.CallbackContext context)  => MouseButtons[2]    = context.performed;
        public void OnMouseForwardButton(InputAction.CallbackContext context) => MouseButtons[3]    = context.performed;
        public void OnMouseBackButton(InputAction.CallbackContext context)    => MouseButtons[4]    = context.performed;

        private static Vector2 GetVector2(InputAction.CallbackContext context) => context.performed ? context.ReadValue<Vector2>() : Vector2.zero;

        private void HandleDPad(Vector2 vec)
        {
            JoypadButtons[RETRO_DEVICE_ID_JOYPAD_UP]    = vec.y >=  0.2f;
            JoypadButtons[RETRO_DEVICE_ID_JOYPAD_DOWN]  = vec.y <= -0.2f;
            JoypadButtons[RETRO_DEVICE_ID_JOYPAD_LEFT]  = vec.x <= -0.2f;
            JoypadButtons[RETRO_DEVICE_ID_JOYPAD_RIGHT] = vec.x >=  0.2f;
        }
    }
}
