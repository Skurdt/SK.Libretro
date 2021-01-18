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
                _                 => null,
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
