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

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SK.Libretro.Unity
{
    public sealed class GamepadIconsExample : MonoBehaviour
    {
        [Serializable]
        public struct GamepadIcons
        {
            public Sprite buttonSouth;
            public Sprite buttonNorth;
            public Sprite buttonEast;
            public Sprite buttonWest;
            public Sprite startButton;
            public Sprite selectButton;
            public Sprite leftTrigger;
            public Sprite rightTrigger;
            public Sprite leftShoulder;
            public Sprite rightShoulder;
            public Sprite dpad;
            public Sprite dpadUp;
            public Sprite dpadDown;
            public Sprite dpadLeft;
            public Sprite dpadRight;
            public Sprite leftStick;
            public Sprite rightStick;
            public Sprite leftStickPress;
            public Sprite rightStickPress;

            public Sprite GetSprite(string controlPath) => controlPath switch
            {
                "buttonSouth"     => buttonSouth,
                "buttonNorth"     => buttonNorth,
                "buttonEast"      => buttonEast,
                "buttonWest"      => buttonWest,
                "start"           => startButton,
                "select"          => selectButton,
                "leftTrigger"     => leftTrigger,
                "rightTrigger"    => rightTrigger,
                "leftShoulder"    => leftShoulder,
                "rightShoulder"   => rightShoulder,
                "dpad"            => dpad,
                "dpad/up"         => dpadUp,
                "dpad/down"       => dpadDown,
                "dpad/left"       => dpadLeft,
                "dpad/right"      => dpadRight,
                "leftStick"       => leftStick,
                "rightStick"      => rightStick,
                "leftStickPress"  => leftStickPress,
                "rightStickPress" => rightStickPress,
                _                 => null
            };
        }

        [SerializeField] private GamepadIcons _xbox;
        [SerializeField] private GamepadIcons _ps4;

        private void OnEnable()
        {
            RebindActionUI[] rebindUIComponents = transform.GetComponentsInChildren<RebindActionUI>();
            foreach (RebindActionUI component in rebindUIComponents)
            {
                component.OnUpdateBindingUI.AddListener(OnUpdateBindingDisplay);
                component.UpdateBindingDisplay();
            }
        }

        private void OnUpdateBindingDisplay(RebindActionUI component, string bindingDisplayString, string deviceLayoutName, string controlPath)
        {
            if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath))
                return;

            Sprite icon = null;
            if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
                icon = _ps4.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
                icon = _xbox.GetSprite(controlPath);

            TMPro.TMP_Text textComponent = component.BindingText;
            if (textComponent == null)
                return;

            Transform imageGO = textComponent.transform.parent.Find("ActionBindingIcon");
            if (imageGO == null)
                return;

            Image imageComponent = imageGO.GetComponent<Image>();
            if (imageComponent == null)
                return;

            if (icon != null)
            {
                textComponent.gameObject.SetActive(false);
                imageComponent.sprite = icon;
                imageComponent.gameObject.SetActive(true);
            }
            else
            {
                textComponent.gameObject.SetActive(true);
                imageComponent.gameObject.SetActive(false);
            }
        }
    }
}
