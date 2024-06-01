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
    internal sealed class MouseHandler : InputHandlerBase
    {
        public short DeltaX { get; private set; }
        public short DeltaY { get; private set; }
        public short Wheel  { get; private set; }

        private uint _buttons;

        public MouseHandler(InputActionAsset inputActionAsset)
        : base(inputActionAsset)
        {
        }

        public short IsButtonDown(RETRO_DEVICE_ID_MOUSE button) => _buttons.IsBitSetAsShort((uint)button);

        protected override void AddActions(InputActionMap actionMap)
        {
            _actions.Add(actionMap.FindAction("MousePositionDelta"), (ctx => (DeltaX, DeltaY) = ctx.ReadValue<Vector2>().ToShort(), true));
            _actions.Add(actionMap.FindAction("MouseWheelDelta"), (ctx => Wheel = ctx.ReadValue<Vector2>().y.ToShort(), true));
            _actions.Add(actionMap.FindAction("MouseLeftButton"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.LEFT), false));
            _actions.Add(actionMap.FindAction("MouseRightButton"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.RIGHT), false));
            _actions.Add(actionMap.FindAction("MouseMiddleButton"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.MIDDLE), false));
            _actions.Add(actionMap.FindAction("MouseForwardButton"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.BUTTON_4), false));
            _actions.Add(actionMap.FindAction("MouseBackButton"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_MOUSE.BUTTON_5), false));
        }
    }
}
