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
        private readonly short[] _buttons = new short[17];
        private readonly LibretroInstance _libretroInstance;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

        public LightgunHandler(InputActionMap inputActionMap, LibretroInstance libretroInstance)
        {
            _inputActionMap   = inputActionMap;
            _libretroInstance = libretroInstance;

            _inputActionMap.FindAction("ButtonTrigger").started  += ButtonTriggerStartedCallback;
            _inputActionMap.FindAction("ButtonTrigger").canceled += ButtonTriggerCanceledCallback;
            _inputActionMap.FindAction("ButtonReload").started   += ButtonReloadStartedCallback;
            _inputActionMap.FindAction("ButtonReload").canceled  += ButtonReloadCanceledCallback;
            _inputActionMap.FindAction("ButtonA").started        += ButtonAStartedCallback;
            _inputActionMap.FindAction("ButtonA").canceled       += ButtonACanceledCallback;
            _inputActionMap.FindAction("ButtonB").started        += ButtonBStartedCallback;
            _inputActionMap.FindAction("ButtonB").canceled       += ButtonBCanceledCallback;
            _inputActionMap.FindAction("ButtonStart").started    += ButtonStartStartedCallback;
            _inputActionMap.FindAction("ButtonStart").canceled   += ButtonStartCanceledCallback;
            _inputActionMap.FindAction("ButtonSelect").started   += ButtonSelectStartedCallback;
            _inputActionMap.FindAction("ButtonSelect").canceled  += ButtonSelectCanceledCallback;
            _inputActionMap.FindAction("ButtonC").started        += ButtonCStartedCallback;
            _inputActionMap.FindAction("ButtonC").canceled       += ButtonCCanceledCallback;
            _inputActionMap.FindAction("DPadUp").started         += DPadUpStartedCallback;
            _inputActionMap.FindAction("DPadUp").canceled        += DPadUpCanceledCallback;
            _inputActionMap.FindAction("DPadDown").started       += DPadDownStartedCallback;
            _inputActionMap.FindAction("DPadDown").canceled      += DPadDownCanceledCallback;
            _inputActionMap.FindAction("DPadLeft").started       += DPadLeftStartedCallback;
            _inputActionMap.FindAction("DPadLeft").canceled      += DPadLeftCanceledCallback;
            _inputActionMap.FindAction("DPadRight").started      += DPadRightStartedCallback;
            _inputActionMap.FindAction("DPadRight").canceled     += DPadRightCanceledCallback;

            _inputActionMap.Enable();
        }

        public void Dispose() => _inputActionMap.Dispose();

        public short IsButtonDown(RETRO_DEVICE_ID_LIGHTGUN button) => _buttons[(uint)button];

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

        private void ButtonTriggerStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.TRIGGER] = 1;

        private void ButtonReloadStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.RELOAD] = 1;

        private void ButtonAStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_A] = 1;

        private void ButtonBStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_B] = 1;

        private void ButtonStartStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.START] = 1;

        private void ButtonSelectStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.SELECT] = 1;

        private void ButtonCStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_C] = 1;

        private void DPadUpStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP] = 1;

        private void DPadDownStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN] = 1;

        private void DPadLeftStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT] = 1;

        private void DPadRightStartedCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT] = 1;

        private void ButtonTriggerCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.TRIGGER] = 0;

        private void ButtonReloadCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.RELOAD] = 0;

        private void ButtonACanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_A] = 0;

        private void ButtonBCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_B] = 0;

        private void ButtonStartCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.START] = 0;

        private void ButtonSelectCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.SELECT] = 0;

        private void ButtonCCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.AUX_C] = 0;

        private void DPadUpCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP] = 0;

        private void DPadDownCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN] = 0;

        private void DPadLeftCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT] = 0;

        private void DPadRightCanceledCallback(InputAction.CallbackContext context) => _buttons[(uint)RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT] = 0;
    }
}
