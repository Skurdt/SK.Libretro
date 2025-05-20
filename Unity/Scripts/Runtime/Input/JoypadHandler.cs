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
        public readonly short[] Buttons = new short[16];

        private readonly InputActionMap _inputActionMap;

        public JoypadHandler(InputActionMap inputActionMap)
        {
            _inputActionMap = inputActionMap;

            _inputActionMap.FindAction("DPadUp").started        += DPadUpStartedCallback;
            _inputActionMap.FindAction("DPadUp").canceled       += DPadUpCanceledCallback;
            _inputActionMap.FindAction("DPadDown").started      += DPadDownStartedCallback;
            _inputActionMap.FindAction("DPadDown").canceled     += DPadDownCanceledCallback;
            _inputActionMap.FindAction("DPadLeft").started      += DPadLeftStartedCallback;
            _inputActionMap.FindAction("DPadLeft").canceled     += DPadLeftCanceledCallback;
            _inputActionMap.FindAction("DPadRight").started     += DPadRightStartedCallback;
            _inputActionMap.FindAction("DPadRight").canceled    += DPadRightCanceledCallback;
            _inputActionMap.FindAction("ButtonStart").started   += ButtonStartStartedCallback;
            _inputActionMap.FindAction("ButtonStart").canceled  += ButtonStartCanceledCallback;
            _inputActionMap.FindAction("ButtonSelect").started  += ButtonSelectStartedCallback;
            _inputActionMap.FindAction("ButtonSelect").canceled += ButtonSelectCanceledCallback;
            _inputActionMap.FindAction("ButtonA").started       += ButtonAStartedCallback;
            _inputActionMap.FindAction("ButtonA").canceled      += ButtonACanceledCallback;
            _inputActionMap.FindAction("ButtonB").started       += ButtonBStartedCallback;
            _inputActionMap.FindAction("ButtonB").canceled      += ButtonBCanceledCallback;
            _inputActionMap.FindAction("ButtonX").started       += ButtonXStartedCallback;
            _inputActionMap.FindAction("ButtonX").canceled      += ButtonXCanceledCallback;
            _inputActionMap.FindAction("ButtonY").started       += ButtonYStartedCallback;
            _inputActionMap.FindAction("ButtonY").canceled      += ButtonYCanceledCallback;
            _inputActionMap.FindAction("ButtonL1").started      += ButtonL1StartedCallback;
            _inputActionMap.FindAction("ButtonL1").canceled     += ButtonL1CanceledCallback;
            _inputActionMap.FindAction("ButtonL2").started      += ButtonL2StartedCallback;
            _inputActionMap.FindAction("ButtonL2").canceled     += ButtonL2CanceledCallback;
            _inputActionMap.FindAction("ButtonL3").started      += ButtonL3StartedCallback;
            _inputActionMap.FindAction("ButtonL3").canceled     += ButtonL3CanceledCallback;
            _inputActionMap.FindAction("ButtonR1").started      += ButtonR1StartedCallback;
            _inputActionMap.FindAction("ButtonR1").canceled     += ButtonR1CanceledCallback;
            _inputActionMap.FindAction("ButtonR2").started      += ButtonR2StartedCallback;
            _inputActionMap.FindAction("ButtonR2").canceled     += ButtonR2CanceledCallback;
            _inputActionMap.FindAction("ButtonR3").started      += ButtonR3StartedCallback;
            _inputActionMap.FindAction("ButtonR3").canceled     += ButtonR3CanceledCallback;

            _inputActionMap.Enable();
        }

        public void Dispose() => _inputActionMap.Dispose();

        public short IsButtonDown(RETRO_DEVICE_ID_JOYPAD button) => Buttons[(uint)button];

        public void SetButtonState(RETRO_DEVICE_ID_JOYPAD button, bool pressed) => Buttons[(uint)button] = pressed ? (short)1 : (short)0;

        private void DPadUpStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.UP] = 1;

        private void DPadDownStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.DOWN] = 1;

        private void DPadLeftStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.LEFT] = 1;

        private void DPadRightStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.RIGHT] = 1;

        private void ButtonStartStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.START] = 1;

        private void ButtonSelectStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.SELECT] = 1;

        private void ButtonAStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.A] = 1;

        private void ButtonBStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.B] = 1;

        private void ButtonXStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.X] = 1;

        private void ButtonYStartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.Y] = 1;

        private void ButtonL1StartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.L] = 1;

        private void ButtonL2StartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.L2] = 1;

        private void ButtonL3StartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.L3] = 1;

        private void ButtonR1StartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.R] = 1;

        private void ButtonR2StartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.R2] = 1;

        private void ButtonR3StartedCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.R3] = 1;

        private void DPadUpCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.UP] = 0;

        private void DPadDownCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.DOWN] = 0;

        private void DPadLeftCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.LEFT] = 0;

        private void DPadRightCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.RIGHT] = 0;

        private void ButtonStartCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.START] = 0;

        private void ButtonSelectCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.SELECT] = 0;

        private void ButtonACanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.A] = 0;

        private void ButtonBCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.B] = 0;

        private void ButtonXCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.X] = 0;

        private void ButtonYCanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.Y] = 0;

        private void ButtonL1CanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.L] = 0;

        private void ButtonL2CanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.L2] = 0;

        private void ButtonL3CanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.L3] = 0;

        private void ButtonR1CanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.R] = 0;

        private void ButtonR2CanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.R2] = 0;

        private void ButtonR3CanceledCallback(InputAction.CallbackContext context) => Buttons[(uint)RETRO_DEVICE_ID_JOYPAD.R3] = 0;
    }
}
