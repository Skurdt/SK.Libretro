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

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static SK.Libretro.Header;

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

        private LibretroInputActions _inputActions;

        private uint _joypadButtons   = 0;
        private uint _keyboardKeys    = 0;
        private uint _mouseButtons    = 0;
        private uint _lightgunButtons = 0;

        private readonly RaycastHit[] _pointerRaycastHits = new RaycastHit[1];

        private void Awake()
        {
            _inputActions = new LibretroInputActions();
            _inputActions.Enable();

            PlayerInput playerInputComponent = GetComponent<PlayerInput>();
            playerInputComponent.onDeviceLost      += playerInput => Logger.Instance.LogInfo($"Player #{playerInput.playerIndex} device lost ({playerInput.devices.Count}).");
            playerInputComponent.onDeviceRegained  += playerInput => Logger.Instance.LogInfo($"Player #{playerInput.playerIndex} device regained ({playerInput.devices.Count}).");
            playerInputComponent.onControlsChanged += playerInput => Logger.Instance.LogInfo($"Player #{playerInput.playerIndex} controls changed ({playerInput.devices.Count}).");

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

        public short JoypadButton(uint button) => _joypadButtons.IsBitSetAsShort(button);
        public short MouseButton(uint button) => _mouseButtons.IsBitSetAsShort(button);
        public short KeyboardKey(uint key) => _keyboardKeys.IsBitSetAsShort(key);
        public short LightgunButton(uint button) => _lightgunButtons.IsBitSetAsShort(button);

        private void RegisterJoypadCallbacks()
        {
            _inputActions.RetroPad.DPadUp.started     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_UP);
            _inputActions.RetroPad.DPadUp.canceled    += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_UP);
            _inputActions.RetroPad.DPadDown.started   += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_DOWN);
            _inputActions.RetroPad.DPadDown.canceled  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_DOWN);
            _inputActions.RetroPad.DPadLeft.started   += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_LEFT);
            _inputActions.RetroPad.DPadLeft.canceled  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_LEFT);
            _inputActions.RetroPad.DPadRight.started  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_RIGHT);
            _inputActions.RetroPad.DPadRight.canceled += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_RIGHT);
            _inputActions.RetroPad.Start.started      += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_START);
            _inputActions.RetroPad.Start.canceled     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_START);
            _inputActions.RetroPad.Select.started     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_SELECT);
            _inputActions.RetroPad.Select.canceled    += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_SELECT);
            _inputActions.RetroPad.A.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_A);
            _inputActions.RetroPad.A.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_A);
            _inputActions.RetroPad.B.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_B);
            _inputActions.RetroPad.B.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_B);
            _inputActions.RetroPad.X.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_X);
            _inputActions.RetroPad.X.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_X);
            _inputActions.RetroPad.Y.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_Y);
            _inputActions.RetroPad.Y.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_Y);
            _inputActions.RetroPad.L1.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_L);
            _inputActions.RetroPad.L1.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_L);
            _inputActions.RetroPad.L2.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_L2);
            _inputActions.RetroPad.L2.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_L2);
            _inputActions.RetroPad.L3.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_L3);
            _inputActions.RetroPad.L3.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_L3);
            _inputActions.RetroPad.R1.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_R);
            _inputActions.RetroPad.R1.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_R);
            _inputActions.RetroPad.R2.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_R2);
            _inputActions.RetroPad.R2.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_R2);
            _inputActions.RetroPad.R3.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_R3);
            _inputActions.RetroPad.R3.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD_R3);
        }

        private void RegisterMouseCallbacks()
        {
            _inputActions.RetroPad.MousePositionDelta.performed += ctx => (MouseX, MouseY) = ctx.ReadValue<Vector2>().ToShort();
            _inputActions.RetroPad.MousePositionDelta.canceled  += ctx => MouseX = MouseY = 0;
            _inputActions.RetroPad.MouseWheelDelta.performed    += ctx => MouseWheel = ctx.ReadValue<Vector2>().y.ToShort();
            _inputActions.RetroPad.MouseWheelDelta.canceled     += ctx => MouseWheel = 0;
            _inputActions.RetroPad.MouseLeftButton.started      += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_LEFT);
            _inputActions.RetroPad.MouseLeftButton.canceled     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_LEFT);
            _inputActions.RetroPad.MouseRightButton.started     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_RIGHT);
            _inputActions.RetroPad.MouseRightButton.canceled    += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_RIGHT);
            _inputActions.RetroPad.MouseMiddleButton.started    += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_MIDDLE);
            _inputActions.RetroPad.MouseMiddleButton.canceled   += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_MIDDLE);
            _inputActions.RetroPad.MouseForwardButton.started   += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_BUTTON_4);
            _inputActions.RetroPad.MouseForwardButton.canceled  += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_BUTTON_4);
            _inputActions.RetroPad.MouseBackButton.started      += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_BUTTON_5);
            _inputActions.RetroPad.MouseBackButton.canceled     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE_BUTTON_5);
        }

        private void RegisterLightgunCallbacks()
        {
            _inputActions.RetroPad.LightgunTrigger.started       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_TRIGGER);
            _inputActions.RetroPad.LightgunTrigger.canceled      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_TRIGGER);
            _inputActions.RetroPad.LightgunReload.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_RELOAD);
            _inputActions.RetroPad.LightgunReload.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_RELOAD);
            _inputActions.RetroPad.LightgunA.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_AUX_A);
            _inputActions.RetroPad.LightgunA.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_AUX_A);
            _inputActions.RetroPad.LightgunB.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_AUX_B);
            _inputActions.RetroPad.LightgunB.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_AUX_B);
            _inputActions.RetroPad.LightgunStart.started         += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_START);
            _inputActions.RetroPad.LightgunStart.canceled        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_START);
            _inputActions.RetroPad.LightgunSelect.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_SELECT);
            _inputActions.RetroPad.LightgunSelect.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_SELECT);
            _inputActions.RetroPad.LightgunC.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_AUX_C);
            _inputActions.RetroPad.LightgunC.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_AUX_C);
            _inputActions.RetroPad.LightgunDPadUp.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_DPAD_UP);
            _inputActions.RetroPad.LightgunDPadUp.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_DPAD_UP);
            _inputActions.RetroPad.LightgunDPadDown.started      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_DPAD_DOWN);
            _inputActions.RetroPad.LightgunDPadDown.canceled     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_DPAD_DOWN);
            _inputActions.RetroPad.LightgunDPadLeft.started      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_DPAD_LEFT);
            _inputActions.RetroPad.LightgunDPadLeft.canceled     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_DPAD_LEFT);
            _inputActions.RetroPad.LightgunDPadRight.started     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_DPAD_RIGHT);
            _inputActions.RetroPad.LightgunDPadRight.canceled    += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN_DPAD_RIGHT);
        }

        private void RegisterAnalogCallbacks()
        {
            _inputActions.RetroPad.AnalogLeft.performed += ctx =>
            {
                (AnalogLeftX, AnalogLeftY) = ctx.ReadValue<Vector2>().ToShort(0x7fff);
                if (AnalogDirectionsToDigital)
                {
                    _joypadButtons.SetBitIf(RETRO_DEVICE_ID_JOYPAD_UP, AnalogLeftY > 0);
                    _joypadButtons.SetBitIf(RETRO_DEVICE_ID_JOYPAD_DOWN, AnalogLeftY < 0);
                    _joypadButtons.SetBitIf(RETRO_DEVICE_ID_JOYPAD_LEFT, AnalogLeftX > 0);
                    _joypadButtons.SetBitIf(RETRO_DEVICE_ID_JOYPAD_RIGHT, AnalogLeftX < 0);
                }
            };
            _inputActions.RetroPad.AnalogLeft.canceled += ctx =>
            {
                AnalogLeftX = AnalogLeftY = 0;
                if (AnalogDirectionsToDigital)
                {
                    _joypadButtons.UnsetBit(RETRO_DEVICE_ID_JOYPAD_UP);
                    _joypadButtons.UnsetBit(RETRO_DEVICE_ID_JOYPAD_DOWN);
                    _joypadButtons.UnsetBit(RETRO_DEVICE_ID_JOYPAD_LEFT);
                    _joypadButtons.UnsetBit(RETRO_DEVICE_ID_JOYPAD_RIGHT);
                }
            };
            _inputActions.RetroPad.AnalogRight.performed += ctx => (AnalogRightX, AnalogRightY) = ctx.ReadValue<Vector2>().ToShort(0x7fff);
            _inputActions.RetroPad.AnalogRight.canceled  += ctx => AnalogRightX = AnalogRightY = 0;
        }
        private void JoypadCallback(in uint id) => _joypadButtons.ToggleBit(id);
        private void MouseCallback(in uint id) => _mouseButtons.ToggleBit(id);
        private void LightgunCallback(in uint id) => _lightgunButtons.ToggleBit(id);

        private void HandleKeyboardKeys()
        {
            for (uint i = 0; i < NUM_KEYBOARD_KEYS; ++i)
                _keyboardKeys.SetBit(i, UnityEngine.Input.GetKey((KeyCode)i));
        }

        private void HandleLightgunPosition()
        {
            if (Mouse.current == null || _libretroInstanceVariable.Current == null)
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

    public static class LibretroCSharpExtensions
    {
        public static (short, short) ToShort(this Vector2 vec, int mul = 1) => (vec.x.ToShort(mul), vec.y.ToShort(mul));
        public static short ToShort(this float floatValue, int mul = 1) => (short)(math.clamp(math.round(floatValue), short.MinValue, short.MaxValue) * mul);

        public static void SetBit(this ref uint bits, in uint bit) => bits |= 1u << (int)bit;
        public static void SetBit(this ref uint bits, in uint bit, in bool enable)
        {
            if (enable)
                bits.SetBit(bit);
            else
                bits.UnsetBit(bit);
        }
        public static void SetBitIf(this ref uint bits, in uint bit, in bool cond)
        {
            if (cond)
                bits |= 1u << (int)bit;
        }
        public static void UnsetBit(this ref uint bits, in uint bit) => bits &= ~(1u << (int)bit);
        public static void ToggleBit(this ref uint bits, in uint bit) => bits ^= 1u << (int)bit;
        public static bool IsBitSet(this uint bits, in uint bit) => (bits & (int)bit) != 0;
        public static short IsBitSetAsShort(this uint bits, in uint bit) => (short)((bits >> (int)bit) & 1);
    }
}
