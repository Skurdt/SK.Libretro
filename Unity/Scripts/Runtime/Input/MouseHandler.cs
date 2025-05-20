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
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal sealed class MouseHandler : IDisposable
    {
        public short DeltaX { get; private set; }
        public short DeltaY { get; private set; }
        public short Wheel  { get; private set; }

        private readonly InputActionMap _inputActionMap;
        private readonly short[] _buttons = new short[11];

        public MouseHandler(InputActionMap inputActionMap)
        {
            _inputActionMap = inputActionMap;

            _inputActionMap.FindAction("PositionDelta").performed += PositionDeltaCallback;
            _inputActionMap.FindAction("PositionDelta").canceled  += PositionDeltaCallback;
            _inputActionMap.FindAction("WheelDelta").performed    += WheelDeltaCallback;
            _inputActionMap.FindAction("WheelDelta").canceled     += WheelDeltaCallback;
            _inputActionMap.FindAction("ButtonLeft").started      += ButtonLeftStartedCallback;
            _inputActionMap.FindAction("ButtonLeft").canceled     += ButtonLeftCanceledCallback;
            _inputActionMap.FindAction("ButtonRight").started     += ButtonRightStartedCallback;
            _inputActionMap.FindAction("ButtonRight").canceled    += ButtonRightCanceledCallback;
            _inputActionMap.FindAction("ButtonMiddle").started    += ButtonMiddleStartedCallback;
            _inputActionMap.FindAction("ButtonMiddle").canceled   += ButtonMiddleCanceledCallback;
            _inputActionMap.FindAction("ButtonForward").started   += ButtonForwardStartedCallback;
            _inputActionMap.FindAction("ButtonForward").canceled  += ButtonForwardCanceledCallback;
            _inputActionMap.FindAction("ButtonBack").started      += ButtonBackStartedCallback;
            _inputActionMap.FindAction("ButtonBack").canceled     += ButtonBackCanceledCallback;

            _inputActionMap.Enable();
        }

        public void Dispose() => _inputActionMap.Dispose();

        public short IsButtonDown(RETRO_DEVICE_ID_MOUSE button) => _buttons[(uint)button];

        private void PositionDeltaCallback(InputAction.CallbackContext context) => (DeltaX, DeltaY) = context.ReadValue<Vector2>().ToShort();

        private void WheelDeltaCallback(InputAction.CallbackContext context) => Wheel = context.ReadValue<Vector2>().y.ToShort();

        private void ButtonLeftStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.LEFT] = 1;

        private void ButtonRightStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.RIGHT] = 1;

        private void ButtonMiddleStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.MIDDLE] = 1;

        private void ButtonForwardStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.BUTTON_4] = 1;

        private void ButtonBackStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.BUTTON_5] = 1;

        private void ButtonLeftCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.LEFT] = 0;

        private void ButtonRightCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.RIGHT] = 0;

        private void ButtonMiddleCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.MIDDLE] = 0;

        private void ButtonForwardCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.BUTTON_4] = 0;

        private void ButtonBackCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_MOUSE.BUTTON_5] = 0;
    }
}
