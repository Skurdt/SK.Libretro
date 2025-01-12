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
        public short Wheel { get; private set; }

        private readonly InputActionMap _inputActionMap;

        private uint _buttons;

        public MouseHandler(InputActionMap inputActionMap)
        {
            _inputActionMap = inputActionMap;

            _inputActionMap.FindAction("PositionDelta").performed += PositionDeltaCallback;
            _inputActionMap.FindAction("PositionDelta").canceled  += PositionDeltaCallback;
            _inputActionMap.FindAction("WheelDelta").performed    += WheelDeltaCallback;
            _inputActionMap.FindAction("WheelDelta").canceled     += WheelDeltaCallback;
            _inputActionMap.FindAction("ButtonLeft").started      += ButtonLeftCallback;
            _inputActionMap.FindAction("ButtonLeft").canceled     += ButtonLeftCallback;
            _inputActionMap.FindAction("ButtonRight").started     += ButtonRightCallback;
            _inputActionMap.FindAction("ButtonRight").canceled    += ButtonRightCallback;
            _inputActionMap.FindAction("ButtonMiddle").started    += ButtonMiddleCallback;
            _inputActionMap.FindAction("ButtonMiddle").canceled   += ButtonMiddleCallback;
            _inputActionMap.FindAction("ButtonForward").started   += ButtonForwardCallback;
            _inputActionMap.FindAction("ButtonForward").canceled  += ButtonForwardCallback;
            _inputActionMap.FindAction("ButtonBack").started      += ButtonBackCallback;
            _inputActionMap.FindAction("ButtonBack").canceled     += ButtonBackCallback;

            _inputActionMap.Enable();
        }

        public void Dispose() => _inputActionMap.Dispose();

        public short IsButtonDown(RETRO_DEVICE_ID_MOUSE button) => _buttons.IsBitSetAsShort((uint)button);

        private void PositionDeltaCallback(InputAction.CallbackContext context) => (DeltaX, DeltaY) = context.ReadValue<Vector2>().ToShort();

        private void WheelDeltaCallback(InputAction.CallbackContext context) => Wheel = context.ReadValue<Vector2>().y.ToShort();

        private void ButtonLeftCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.LEFT);

        private void ButtonRightCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.RIGHT);

        private void ButtonMiddleCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.MIDDLE);

        private void ButtonForwardCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.BUTTON_4);

        private void ButtonBackCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.BUTTON_5);
    }
}
