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
    internal sealed class AnalogHandler : IDisposable
    {
        public LeftStickBehaviour LeftStickBehaviour { get; set; }

        public short LeftX  { get; private set; }
        public short LeftY  { get; private set; }
        public short RightX { get; private set; }
        public short RightY { get; private set; }

        private readonly InputActionMap _inputActionMap;
        private readonly JoypadHandler _joypadHandler;

        public AnalogHandler(InputActionMap inputActionMap, JoypadHandler joypadHandler, LeftStickBehaviour leftStickBehaviour)
        {
            _inputActionMap    = inputActionMap;
            _joypadHandler     = joypadHandler;
            LeftStickBehaviour = leftStickBehaviour;

            _inputActionMap.FindAction("StickLeft").performed  += StickLeftPerformedCallback;
            _inputActionMap.FindAction("StickLeft").canceled   += StickLeftCanceledCallback;
            _inputActionMap.FindAction("StickRight").performed += StickRightPerformedCallback;
            _inputActionMap.FindAction("StickRight").canceled  += StickRightCanceledCallback;

            _inputActionMap.Enable();
        }

        public void Dispose() => _inputActionMap.Dispose();

        private void StickLeftPerformedCallback(InputAction.CallbackContext context)
        {
            switch (LeftStickBehaviour)
            {
                case LeftStickBehaviour.AnalogOnly:
                    (LeftX, LeftY) = context.ReadValue<Vector2>().ToShort(0x7fff);
                    break;
                case LeftStickBehaviour.DigitalOnly:
                {
                    (LeftX, LeftY)             = (0, 0);
                    (short leftX, short leftY) = context.ReadValue<Vector2>().ToShort(0x7fff);
                    _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.UP, leftY > 0);
                    _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.DOWN, leftY < 0);
                    _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.LEFT, leftX < 0);
                    _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.RIGHT, leftX > 0);
                }
                break;
                case LeftStickBehaviour.AnalogAndDigital:
                    (LeftX, LeftY) = context.ReadValue<Vector2>().ToShort(0x7fff);
                    _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.UP, LeftY > 0);
                    _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.DOWN, LeftY < 0);
                    _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.LEFT, LeftX < 0);
                    _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.RIGHT, LeftX > 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StickLeftCanceledCallback(InputAction.CallbackContext context)
        {
            (LeftX, LeftY) = (0, 0);

            if (LeftStickBehaviour == LeftStickBehaviour.AnalogOnly)
                return;

            _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.UP, false);
            _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.DOWN, false);
            _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.LEFT, false);
            _joypadHandler.SetButtonState(RETRO_DEVICE_ID_JOYPAD.RIGHT, false);
        }

        private void StickRightPerformedCallback(InputAction.CallbackContext context) => (RightX, RightY) = context.ReadValue<Vector2>().ToShort(0x7fff);
        
        private void StickRightCanceledCallback(InputAction.CallbackContext context) => (RightX, RightY) = (0, 0);
    }
}
