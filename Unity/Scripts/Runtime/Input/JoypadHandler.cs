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
    internal sealed class JoypadHandler : InputHandlerBase
    {
        public short Buttons => (short)_buttons;

        private uint _buttons;

        public JoypadHandler(InputActionAsset inputActionAsset)
        : base(inputActionAsset)
        {
        }

        public short IsButtonDown(RETRO_DEVICE_ID_JOYPAD button) => _buttons.IsBitSetAsShort((uint)button);

        public void SetButtonState(RETRO_DEVICE_ID_JOYPAD button, bool pressed) => _buttons.SetBit((uint)button, pressed);

        protected override void AddActions(InputActionMap actionMap)
        {
            _actions.Add(actionMap.FindAction("JoypadDPadUp"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.UP), false));
            _actions.Add(actionMap.FindAction("JoypadDPadDown"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.DOWN), false));
            _actions.Add(actionMap.FindAction("JoypadDPadLeft"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.LEFT), false));
            _actions.Add(actionMap.FindAction("JoypadDPadRight"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.RIGHT), false));
            _actions.Add(actionMap.FindAction("JoypadStart"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.START), false));
            _actions.Add(actionMap.FindAction("JoypadSelect"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.SELECT), false));
            _actions.Add(actionMap.FindAction("JoypadA"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.A), false));
            _actions.Add(actionMap.FindAction("JoypadB"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.B), false));
            _actions.Add(actionMap.FindAction("JoypadX"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.X), false));
            _actions.Add(actionMap.FindAction("JoypadY"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.Y), false));
            _actions.Add(actionMap.FindAction("JoypadL1"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L), false));
            _actions.Add(actionMap.FindAction("JoypadL2"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L2), false));
            _actions.Add(actionMap.FindAction("JoypadL3"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.L3), false));
            _actions.Add(actionMap.FindAction("JoypadR1"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R), false));
            _actions.Add(actionMap.FindAction("JoypadR2"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R2), false));
            _actions.Add(actionMap.FindAction("JoypadR3"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_JOYPAD.R3), false));
        }
    }
}
