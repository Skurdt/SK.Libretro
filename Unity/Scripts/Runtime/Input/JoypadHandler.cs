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
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal sealed class JoypadHandler : IDisposable
    {
        public short Buttons => (short)_buttons;

        private readonly InputActionMap _inputActionMap;

        private uint _buttons;

        public JoypadHandler(InputActionMap inputActionMap)
        {
            _inputActionMap = inputActionMap;

            _inputActionMap.FindAction("DPadUp").started        += DPadUpCallback;
            _inputActionMap.FindAction("DPadUp").canceled       += DPadUpCallback;
            _inputActionMap.FindAction("DPadDown").started      += DPadDownCallback;
            _inputActionMap.FindAction("DPadDown").canceled     += DPadDownCallback;
            _inputActionMap.FindAction("DPadLeft").started      += DPadLeftCallback;
            _inputActionMap.FindAction("DPadLeft").canceled     += DPadLeftCallback;
            _inputActionMap.FindAction("DPadRight").started     += DPadRightCallback;
            _inputActionMap.FindAction("DPadRight").canceled    += DPadRightCallback;
            _inputActionMap.FindAction("ButtonStart").started   += ButtonStartCallback;
            _inputActionMap.FindAction("ButtonStart").canceled  += ButtonStartCallback;
            _inputActionMap.FindAction("ButtonSelect").started  += ButtonSelectCallback;
            _inputActionMap.FindAction("ButtonSelect").canceled += ButtonSelectCallback;
            _inputActionMap.FindAction("ButtonA").started       += ButtonACallback;
            _inputActionMap.FindAction("ButtonA").canceled      += ButtonACallback;
            _inputActionMap.FindAction("ButtonB").started       += ButtonBCallback;
            _inputActionMap.FindAction("ButtonB").canceled      += ButtonBCallback;
            _inputActionMap.FindAction("ButtonX").started       += ButtonXCallback;
            _inputActionMap.FindAction("ButtonX").canceled      += ButtonXCallback;
            _inputActionMap.FindAction("ButtonY").started       += ButtonYCallback;
            _inputActionMap.FindAction("ButtonY").canceled      += ButtonYCallback;
            _inputActionMap.FindAction("ButtonL1").started      += ButtonL1Callback;
            _inputActionMap.FindAction("ButtonL1").canceled     += ButtonL1Callback;
            _inputActionMap.FindAction("ButtonL2").started      += ButtonL2Callback;
            _inputActionMap.FindAction("ButtonL2").canceled     += ButtonL2Callback;
            _inputActionMap.FindAction("ButtonL3").started      += ButtonL3Callback;
            _inputActionMap.FindAction("ButtonL3").canceled     += ButtonL3Callback;
            _inputActionMap.FindAction("ButtonR1").started      += ButtonR1Callback;
            _inputActionMap.FindAction("ButtonR1").canceled     += ButtonR1Callback;
            _inputActionMap.FindAction("ButtonR2").started      += ButtonR2Callback;
            _inputActionMap.FindAction("ButtonR2").canceled     += ButtonR2Callback;
            _inputActionMap.FindAction("ButtonR3").started      += ButtonR3Callback;
            _inputActionMap.FindAction("ButtonR3").canceled     += ButtonR3Callback;

            _inputActionMap.Enable();
        }

        public void Dispose() => _inputActionMap.Dispose();

        public short IsButtonDown(RETRO_DEVICE_ID_JOYPAD button) => _buttons.IsBitSetAsShort((uint)button);

        public void SetButtonState(RETRO_DEVICE_ID_JOYPAD button, bool pressed) => _buttons.SetBit((uint)button, pressed);

        private void DPadUpCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.UP);

        private void DPadDownCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.DOWN);

        private void DPadLeftCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.LEFT);

        private void DPadRightCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.RIGHT);

        private void ButtonStartCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.START);

        private void ButtonSelectCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.SELECT);

        private void ButtonACallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.A);

        private void ButtonBCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.B);

        private void ButtonXCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.X);

        private void ButtonYCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.Y);

        private void ButtonL1Callback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L);

        private void ButtonL2Callback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L2);

        private void ButtonL3Callback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L3);

        private void ButtonR1Callback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R);

        private void ButtonR2Callback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R2);

        private void ButtonR3Callback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R3);
    }
}
