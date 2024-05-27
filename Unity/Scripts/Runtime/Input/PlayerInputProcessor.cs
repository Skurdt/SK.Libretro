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
    [RequireComponent(typeof(PlayerInput))]
    internal sealed class PlayerInputProcessor : MonoBehaviour
    {
        [SerializeField] private LibretroInstanceVariable _libretroInstanceVariable;

        public bool AnalogDirectionsToDigital { get; set; }
        public short JoypadButtons => (short)_joypadButtons;

        public short MouseX     { get; private set; }
        public short MouseY     { get; private set; }
        public short MouseWheel { get; private set; }

        public short LightgunX          { get; private set; }
        public short LightgunY          { get; private set; }
        public bool LightgunIsOffscreen { get; private set; }

        public short AnalogLeftX  { get; private set; }
        public short AnalogLeftY  { get; private set; }
        public short AnalogRightX { get; private set; }
        public short AnalogRightY { get; private set; }

        private const uint NUM_KEYBOARD_KEYS = 320;

        public LibretroInputActions InputActions { get; set; }

        private uint _joypadButtons   = 0;
        private uint _keyboardKeys    = 0;
        private uint _mouseButtons    = 0;
        private uint _lightgunButtons = 0;

        private readonly RaycastHit[] _pointerRaycastHits = new RaycastHit[1];

        private void Awake()
        {
            PlayerInput playerInputComponent = GetComponent<PlayerInput>();
            playerInputComponent.onDeviceLost      += playerInput => Debug.Log($"Player #{playerInput.playerIndex} device lost ({playerInput.devices.Count}).");
            playerInputComponent.onDeviceRegained  += playerInput => Debug.Log($"Player #{playerInput.playerIndex} device regained ({playerInput.devices.Count}).");
            playerInputComponent.onControlsChanged += playerInput => Debug.Log($"Player #{playerInput.playerIndex} controls changed ({playerInput.devices.Count}).");

            RegisterJoypadCallbacks();
            RegisterMouseCallbacks();
            RegisterLightgunCallbacks();
            RegisterAnalogCallbacks();
        }

        private void Update()
        {
            HandleKeyboardKeys();
            HandleLightgunPosition();
        }

        public short JoypadButton(RETRO_DEVICE_ID_JOYPAD button) => _joypadButtons.IsBitSetAsShort((uint)button);
        public short MouseButton(RETRO_DEVICE_ID_MOUSE button) => _mouseButtons.IsBitSetAsShort((uint)button);
        public short KeyboardKey(retro_key key) => _keyboardKeys.IsBitSetAsShort((uint)key);
        public short LightgunButton(RETRO_DEVICE_ID_LIGHTGUN button) => _lightgunButtons.IsBitSetAsShort((uint)button);

        private void RegisterJoypadCallbacks()
        {
            InputActions.Emulation.DPadUp.started     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.UP);
            InputActions.Emulation.DPadUp.canceled    += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.UP);
            InputActions.Emulation.DPadDown.started   += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.DOWN);
            InputActions.Emulation.DPadDown.canceled  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.DOWN);
            InputActions.Emulation.DPadLeft.started   += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.LEFT);
            InputActions.Emulation.DPadLeft.canceled  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.LEFT);
            InputActions.Emulation.DPadRight.started  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.RIGHT);
            InputActions.Emulation.DPadRight.canceled += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.RIGHT);
            InputActions.Emulation.Start.started      += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.START);
            InputActions.Emulation.Start.canceled     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.START);
            InputActions.Emulation.Select.started     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.SELECT);
            InputActions.Emulation.Select.canceled    += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.SELECT);
            InputActions.Emulation.A.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.A);
            InputActions.Emulation.A.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.A);
            InputActions.Emulation.B.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.B);
            InputActions.Emulation.B.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.B);
            InputActions.Emulation.X.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.X);
            InputActions.Emulation.X.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.X);
            InputActions.Emulation.Y.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.Y);
            InputActions.Emulation.Y.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.Y);
            InputActions.Emulation.L1.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L);
            InputActions.Emulation.L1.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L);
            InputActions.Emulation.L2.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L2);
            InputActions.Emulation.L2.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L2);
            InputActions.Emulation.L3.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L3);
            InputActions.Emulation.L3.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L3);
            InputActions.Emulation.R1.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R);
            InputActions.Emulation.R1.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R);
            InputActions.Emulation.R2.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R2);
            InputActions.Emulation.R2.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R2);
            InputActions.Emulation.R3.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R3);
            InputActions.Emulation.R3.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R3);
        }

        private void RegisterMouseCallbacks()
        {
            InputActions.Emulation.MousePositionDelta.performed += ctx => (MouseX, MouseY) = ctx.ReadValue<Vector2>().ToShort();
            InputActions.Emulation.MousePositionDelta.canceled  += ctx => MouseX = MouseY = 0;
            InputActions.Emulation.MouseWheelDelta.performed    += ctx => MouseWheel = ctx.ReadValue<Vector2>().y.ToShort();
            InputActions.Emulation.MouseWheelDelta.canceled     += ctx => MouseWheel = 0;
            InputActions.Emulation.MouseLeftButton.started      += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.LEFT);
            InputActions.Emulation.MouseLeftButton.canceled     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.LEFT);
            InputActions.Emulation.MouseRightButton.started     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.RIGHT);
            InputActions.Emulation.MouseRightButton.canceled    += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.RIGHT);
            InputActions.Emulation.MouseMiddleButton.started    += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.MIDDLE);
            InputActions.Emulation.MouseMiddleButton.canceled   += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.MIDDLE);
            InputActions.Emulation.MouseForwardButton.started   += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.BUTTON_4);
            InputActions.Emulation.MouseForwardButton.canceled  += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.BUTTON_4);
            InputActions.Emulation.MouseBackButton.started      += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.BUTTON_5);
            InputActions.Emulation.MouseBackButton.canceled     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.BUTTON_5);
        }

        private void RegisterLightgunCallbacks()
        {
            InputActions.Emulation.LightgunTrigger.started       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.TRIGGER);
            InputActions.Emulation.LightgunTrigger.canceled      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.TRIGGER);
            InputActions.Emulation.LightgunReload.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.RELOAD);
            InputActions.Emulation.LightgunReload.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.RELOAD);
            InputActions.Emulation.LightgunA.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_A);
            InputActions.Emulation.LightgunA.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_A);
            InputActions.Emulation.LightgunB.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_B);
            InputActions.Emulation.LightgunB.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_B);
            InputActions.Emulation.LightgunStart.started         += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.START);
            InputActions.Emulation.LightgunStart.canceled        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.START);
            InputActions.Emulation.LightgunSelect.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.SELECT);
            InputActions.Emulation.LightgunSelect.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.SELECT);
            InputActions.Emulation.LightgunC.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_C);
            InputActions.Emulation.LightgunC.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_C);
            InputActions.Emulation.LightgunDPadUp.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP);
            InputActions.Emulation.LightgunDPadUp.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP);
            InputActions.Emulation.LightgunDPadDown.started      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN);
            InputActions.Emulation.LightgunDPadDown.canceled     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN);
            InputActions.Emulation.LightgunDPadLeft.started      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT);
            InputActions.Emulation.LightgunDPadLeft.canceled     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT);
            InputActions.Emulation.LightgunDPadRight.started     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT);
            InputActions.Emulation.LightgunDPadRight.canceled    += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT);
        }

        private void RegisterAnalogCallbacks()
        {
            InputActions.Emulation.AnalogLeft.performed += ctx =>
            {
                (AnalogLeftX, AnalogLeftY) = ctx.ReadValue<Vector2>().ToShort(0x7fff);
                if (AnalogDirectionsToDigital)
                {
                    _joypadButtons.SetBit((uint)RETRO_DEVICE_ID_JOYPAD.UP, AnalogLeftY > 0);
                    _joypadButtons.SetBit((uint)RETRO_DEVICE_ID_JOYPAD.DOWN, AnalogLeftY < 0);
                    _joypadButtons.SetBit((uint)RETRO_DEVICE_ID_JOYPAD.LEFT, AnalogLeftX < 0);
                    _joypadButtons.SetBit((uint)RETRO_DEVICE_ID_JOYPAD.RIGHT, AnalogLeftX > 0);
                }
            };
            InputActions.Emulation.AnalogLeft.canceled += ctx =>
            {
                AnalogLeftX = AnalogLeftY = 0;
                if (AnalogDirectionsToDigital)
                {
                    _joypadButtons.UnsetBit((uint)RETRO_DEVICE_ID_JOYPAD.UP);
                    _joypadButtons.UnsetBit((uint)RETRO_DEVICE_ID_JOYPAD.DOWN);
                    _joypadButtons.UnsetBit((uint)RETRO_DEVICE_ID_JOYPAD.LEFT);
                    _joypadButtons.UnsetBit((uint)RETRO_DEVICE_ID_JOYPAD.RIGHT);
                }
            };
            InputActions.Emulation.AnalogRight.performed += ctx => (AnalogRightX, AnalogRightY) = ctx.ReadValue<Vector2>().ToShort(0x7fff);
            InputActions.Emulation.AnalogRight.canceled  += ctx => AnalogRightX = AnalogRightY = 0;
        }
        private void JoypadCallback(in RETRO_DEVICE_ID_JOYPAD id) => _joypadButtons.ToggleBit((uint)id);
        private void MouseCallback(in RETRO_DEVICE_ID_MOUSE id) => _mouseButtons.ToggleBit((uint)id);
        private void LightgunCallback(in RETRO_DEVICE_ID_LIGHTGUN id) => _lightgunButtons.ToggleBit((uint)id);

        private void HandleKeyboardKeys()
        {
            //for (uint i = 0; i < NUM_KEYBOARD_KEYS; ++i)
            //    _keyboardKeys.SetBit(i, Input.GetKey((KeyCode)i));
        }

        private void HandleLightgunPosition()
        {
            if (Mouse.current is null || _libretroInstanceVariable.Current == null)
            {
                LightgunX = LightgunY = -0x8000;
                return;
            }

            Ray ray = _libretroInstanceVariable.Current.Camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            int hitCount = Physics.RaycastNonAlloc(ray, _pointerRaycastHits, float.PositiveInfinity, LayerMask.GetMask(LayerMask.LayerToName(_libretroInstanceVariable.Current.LightgunRaycastLayer)));
            bool inScreen = hitCount > 0 && _pointerRaycastHits[0].collider is MeshCollider meshCollider && meshCollider == _libretroInstanceVariable.Current.Collider;
            if (inScreen)
            {
                Vector2 coords = _pointerRaycastHits[0].textureCoord;
                LightgunX = (short)(math.remap(0f, 1f, -1f, 1f, coords.x) * 0x7fff);
                LightgunY = (short)(-math.remap(0f, 1f, -1f, 1f, coords.y) * 0x7fff);
            }
            else
                LightgunX = LightgunY = -0x8000;
            LightgunIsOffscreen = !inScreen;
        }
    }
}
