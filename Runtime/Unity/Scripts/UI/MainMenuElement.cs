/* MIT License

 * Copyright (c) 2020 Skurdt
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

using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    [DisallowMultipleComponent]
    public abstract class MainMenuElement : MonoBehaviour
    {
        [SerializeField] protected InputActionAsset _inputActions;
        [SerializeField] protected GameObject _screen;
        [SerializeField] private MainMenuUI _menu;

        private void Start() => Hide();

        public void Show()
        {
            _inputActions.Disable();
            OnShow();

            _menu.HideAll();
            _screen.SetActive(true);
        }

        public void Hide()
        {
            OnHide();
            _inputActions.Enable();

            _menu.ShowAll();
            _screen.SetActive(false);
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }
    }
}
