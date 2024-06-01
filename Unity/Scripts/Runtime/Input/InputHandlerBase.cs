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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    internal abstract class InputHandlerBase
    {
        protected readonly Dictionary<InputAction, (Action<InputAction.CallbackContext> callback, bool performed)> _actions = new();

        public InputHandlerBase(InputActionAsset inputActionAsset)
        {
            InputActionMap actionMap = inputActionAsset.FindActionMap("Emulation");
            if (actionMap is null)
            {
                Debug.LogError($"ActionMap 'Emulation' not found");
                return;
            }

            AddActions(actionMap);

            RegisterCallbacks();
        }

        ~InputHandlerBase() => UnregisterCallbacks();

        protected abstract void AddActions(InputActionMap actionMap);

        private void RegisterCallbacks()
        {
            foreach ((InputAction action, (Action<InputAction.CallbackContext> callback, bool performed)) in _actions)
            {
                if (performed)
                    action.performed += callback;
                else
                    action.started += callback;
                action.canceled += callback;
            }
        }

        private void UnregisterCallbacks()
        {
            foreach ((InputAction action, (Action<InputAction.CallbackContext> callback, bool performed)) in _actions)
            {
                if (performed)
                    action.performed -= callback;
                else
                    action.started -= callback;
                action.canceled -= callback;
            }
        }
    }
}
