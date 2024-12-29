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
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal sealed class JoypadHandler : LibretroInputActions.IJoypadActions
    {
        public short Buttons => (short)_buttons;

        private uint _buttons;

        public short IsButtonDown(RETRO_DEVICE_ID_JOYPAD button) => _buttons.IsBitSetAsShort((uint)button);

        public void SetButtonState(RETRO_DEVICE_ID_JOYPAD button, bool pressed) => _buttons.SetBit((uint)button, pressed);

        public void OnJoypadDPadUp(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.UP);
        }
        
        public void OnJoypadDPadDown(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.DOWN);
        }
        
        public void OnJoypadDPadLeft(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.LEFT);
        }
        
        public void OnJoypadDPadRight(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.RIGHT);
        }
        
        public void OnJoypadStart(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.START);
        }
        
        public void OnJoypadSelect(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.SELECT);
        }
        
        public void OnJoypadA(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.A);
        }
        
        public void OnJoypadB(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.B);
        }
        
        public void OnJoypadX(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.X);
        }
        
        public void OnJoypadY(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.Y);
        }
        
        public void OnJoypadL1(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L);
        }
        
        public void OnJoypadL2(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L2);
        }
        
        public void OnJoypadL3(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L3);
        }
        
        public void OnJoypadR1(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R);
        }
        
        public void OnJoypadR2(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R2);
        }
        
        public void OnJoypadR3(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R3);
        }
    }
}
