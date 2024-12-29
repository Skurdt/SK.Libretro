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
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal sealed class MouseHandler : LibretroInputActions.IMouseActions
    {
        public short DeltaX { get; private set; }
        public short DeltaY { get; private set; }
        public short Wheel { get; private set; }

        private uint _buttons;

        public short IsButtonDown(RETRO_DEVICE_ID_MOUSE button) => _buttons.IsBitSetAsShort((uint)button);

        public void OnMousePositionDelta(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled)
                (DeltaX, DeltaY) = context.ReadValue<Vector2>().ToShort();
        }

        public void OnMouseWheelDelta(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled)
                Wheel = context.ReadValue<Vector2>().y.ToShort();
        }

        public void OnMouseLeftButton(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.LEFT);
        }

        public void OnMouseRightButton(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.RIGHT);
        }

        public void OnMouseMiddleButton(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.MIDDLE);
        }

        public void OnMouseForwardButton(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.BUTTON_4);
        }

        public void OnMouseBackButton(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled)
                _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.BUTTON_5);
        }
    }
}
