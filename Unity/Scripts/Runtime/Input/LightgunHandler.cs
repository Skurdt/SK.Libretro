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
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal sealed class LightgunHandler : IDisposable
    {
        public short X       { get; private set; }
        public short Y       { get; private set; }
        public bool InScreen { get; private set; }

        private const int RANGE_X = 0x7aa8;
        private const int RANGE_Y = 0x7fff;

        private readonly InputActionMap _inputActionMap;
        private readonly LibretroInstance _libretroInstance;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        private uint _buttons;

        public LightgunHandler(InputActionMap inputActionMap, LibretroInstance libretroInstance)
        {
            _inputActionMap   = inputActionMap;
            _libretroInstance = libretroInstance;

            _inputActionMap.FindAction("ButtonTrigger").started  += ButtonTriggerCallback;
            _inputActionMap.FindAction("ButtonTrigger").canceled += ButtonTriggerCallback;
            _inputActionMap.FindAction("ButtonReload").started   += ButtonReloadCallback;
            _inputActionMap.FindAction("ButtonReload").canceled  += ButtonReloadCallback;
            _inputActionMap.FindAction("ButtonA").started        += ButtonACallback;
            _inputActionMap.FindAction("ButtonA").canceled       += ButtonACallback;
            _inputActionMap.FindAction("ButtonB").started        += ButtonBCallback;
            _inputActionMap.FindAction("ButtonB").canceled       += ButtonBCallback;
            _inputActionMap.FindAction("ButtonStart").started    += ButtonStartCallback;
            _inputActionMap.FindAction("ButtonStart").canceled   += ButtonStartCallback;
            _inputActionMap.FindAction("ButtonSelect").started   += ButtonSelectCallback;
            _inputActionMap.FindAction("ButtonSelect").canceled  += ButtonSelectCallback;
            _inputActionMap.FindAction("ButtonC").started        += ButtonCCallback;
            _inputActionMap.FindAction("ButtonC").canceled       += ButtonCCallback;
            _inputActionMap.FindAction("DPadUp").started         += DPadUpCallback;
            _inputActionMap.FindAction("DPadUp").canceled        += DPadUpCallback;
            _inputActionMap.FindAction("DPadDown").started       += DPadDownCallback;
            _inputActionMap.FindAction("DPadDown").canceled      += DPadDownCallback;
            _inputActionMap.FindAction("DPadLeft").started       += DPadLeftCallback;
            _inputActionMap.FindAction("DPadLeft").canceled      += DPadLeftCallback;
            _inputActionMap.FindAction("DPadRight").started      += DPadRightCallback;
            _inputActionMap.FindAction("DPadRight").canceled     += DPadRightCallback;

            _inputActionMap.Enable();
        }

        public void Dispose() => _inputActionMap.Dispose();

        public short IsButtonDown(RETRO_DEVICE_ID_LIGHTGUN button) => _buttons.IsBitSetAsShort((uint)button);

        public void Update()
        {
            if (!_libretroInstance)
            {
                X = Y = -0x8000;
                InScreen = false;
                return;
            }

            Ray ray = _libretroInstance.LightgunRay;
            int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, math.INFINITY, 1 << _libretroInstance.LightgunRaycastLayer);
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

            X = (short)(math.remap(0f, 1f, -1f, 1f, _raycastHits[0].textureCoord.x) * RANGE_X);
            Y = (short)(-math.remap(0f, 1f, -1f, 1f, _raycastHits[0].textureCoord.y) * RANGE_Y);
        }

        private void ButtonTriggerCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.TRIGGER);

        private void ButtonReloadCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.RELOAD);

        private void ButtonACallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_A);

        private void ButtonBCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_B);

        private void ButtonStartCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.START);

        private void ButtonSelectCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.SELECT);

        private void ButtonCCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_C);

        private void DPadUpCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP);

        private void DPadDownCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN);

        private void DPadLeftCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT);

        private void DPadRightCallback(InputAction.CallbackContext context) => _buttons.ToggleBit((uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT);
    }
}
