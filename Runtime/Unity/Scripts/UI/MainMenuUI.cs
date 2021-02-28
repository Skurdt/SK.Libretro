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
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    [DisallowMultipleComponent, ExecuteAlways]
    public sealed class MainMenuUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private GameObject _common;
        [SerializeField] private GameObject _coreSpecific;

        private void Awake() => HideAll();

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_inputActions != null)
                _inputActions.Disable();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == null && _inputActions != null)
                _inputActions.Enable();
        }

        public void ShowAll()
        {
            ShowCommonOptions();
            ShowCoreSpecificOptions();
        }

        public void HideAll()
        {
            HideCommonOptions();
            HideCoreSpecificOptions();
        }

        public void ShowCommonOptions()
        {
            if (_common != null)
                _common.SetActive(true);
        }

        public void HideCommonOptions()
        {
            if (_common != null)
                _common.SetActive(false);
        }

        public void ShowCoreSpecificOptions()
        {
            if (_coreSpecific != null)
                _coreSpecific.SetActive(true);
        }

        public void HideCoreSpecificOptions()
        {
            if (_coreSpecific != null)
                _coreSpecific.SetActive(false);
        }
    }
}
