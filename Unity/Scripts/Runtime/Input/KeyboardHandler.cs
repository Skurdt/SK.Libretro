﻿/* MIT License

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
    internal sealed class KeyboardHandler : IDisposable
    {
        private const uint NUM_KEYBOARD_KEYS = 320;

        private readonly InputActionMap _inputActionMap;
        private readonly short[] _keys = new short[NUM_KEYBOARD_KEYS];

        public KeyboardHandler(InputActionMap inputActionMap)
        {
            _inputActionMap = inputActionMap;

            _inputActionMap.FindAction("Key").started  += KeyStartedCallback;
            _inputActionMap.FindAction("Key").canceled += KeyCanceledCallback;

            _inputActionMap.Enable();
        }

        public void Dispose() => _inputActionMap.Dispose();

        public short IsKeyDown(retro_key key) => _keys[(uint)key];

        private void KeyStartedCallback(InputAction.CallbackContext context) { }

        private void KeyCanceledCallback(InputAction.CallbackContext context) { }

        public void Update()
        {
            for (uint i = 0; i < NUM_KEYBOARD_KEYS; ++i)
                _keys[i] = (short)(Input.GetKey((KeyCode)i) ? 1 : 0);
        }
    }
}
