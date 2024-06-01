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
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal sealed class LightgunHandler : InputHandlerBase
    {
        public short Buttons => (short)_buttons;
        public short X       { get; private set; }
        public short Y       { get; private set; }
        public bool InScreen { get; private set; }

        private readonly LibretroInstance _libretroInstance;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        private uint _buttons;

        public LightgunHandler(InputActionAsset inputActionAsset, LibretroInstance libretroInstance)
        : base(inputActionAsset)
            => _libretroInstance = libretroInstance;

        public short IsButtonDown(RETRO_DEVICE_ID_LIGHTGUN button) => _buttons.IsBitSetAsShort((uint)button);

        public void Update(Vector2 aimPosition)
        {
            if (!_libretroInstance)
            {
                X = Y = -0x8000;
                InScreen = false;
                return;
            }

            Ray ray = _libretroInstance.Camera.ScreenPointToRay(aimPosition);
            int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, float.PositiveInfinity, LayerMask.GetMask(LayerMask.LayerToName(_libretroInstance.LightgunRaycastLayer)));
            if (hitCount <= 0)
            {
                X = Y = -0x8000;
                InScreen = false;
                return;
            }

            InScreen = _raycastHits[0].collider == _libretroInstance.Collider;
            if (!InScreen)
            {
                X = Y = -0x8000;
                return;
            }

            X = (short)(math.remap(0f, 1f, -1f, 1f, _raycastHits[0].textureCoord.x) * 0x7fff);
            Y = (short)(-math.remap(0f, 1f, -1f, 1f, _raycastHits[0].textureCoord.y) * 0x7fff);
        }

        protected override void AddActions(InputActionMap actionMap)
        {
            _actions.Add(actionMap.FindAction("LightgunTrigger"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.TRIGGER), false));
            _actions.Add(actionMap.FindAction("LightgunReload"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.RELOAD), false));
            _actions.Add(actionMap.FindAction("LightgunA"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_A), false));
            _actions.Add(actionMap.FindAction("LightgunB"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_B), false));
            _actions.Add(actionMap.FindAction("LightgunStart"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.START), false));
            _actions.Add(actionMap.FindAction("LightgunSelect"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.SELECT), false));
            _actions.Add(actionMap.FindAction("LightgunC"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_C), false));
            _actions.Add(actionMap.FindAction("LightgunDPadUp"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP), false));
            _actions.Add(actionMap.FindAction("LightgunDPadDown"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN), false));
            _actions.Add(actionMap.FindAction("LightgunDPadLeft"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT), false));
            _actions.Add(actionMap.FindAction("LightgunDPadRight"), (ctx => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT), false));
        }
    }
}
