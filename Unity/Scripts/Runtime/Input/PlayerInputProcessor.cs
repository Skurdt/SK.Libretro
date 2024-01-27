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
            _inputActions.Emulation.DPadUp.started     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.UP);
            _inputActions.Emulation.DPadUp.canceled    += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.UP);
            _inputActions.Emulation.DPadDown.started   += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.DOWN);
            _inputActions.Emulation.DPadDown.canceled  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.DOWN);
            _inputActions.Emulation.DPadLeft.started   += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.LEFT);
            _inputActions.Emulation.DPadLeft.canceled  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.LEFT);
            _inputActions.Emulation.DPadRight.started  += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.RIGHT);
            _inputActions.Emulation.DPadRight.canceled += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.RIGHT);
            _inputActions.Emulation.Start.started      += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.START);
            _inputActions.Emulation.Start.canceled     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.START);
            _inputActions.Emulation.Select.started     += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.SELECT);
            _inputActions.Emulation.Select.canceled    += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.SELECT);
            _inputActions.Emulation.A.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.A);
            _inputActions.Emulation.A.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.A);
            _inputActions.Emulation.B.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.B);
            _inputActions.Emulation.B.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.B);
            _inputActions.Emulation.X.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.X);
            _inputActions.Emulation.X.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.X);
            _inputActions.Emulation.Y.started          += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.Y);
            _inputActions.Emulation.Y.canceled         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.Y);
            _inputActions.Emulation.L1.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L);
            _inputActions.Emulation.L1.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L);
            _inputActions.Emulation.L2.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L2);
            _inputActions.Emulation.L2.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L2);
            _inputActions.Emulation.L3.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L3);
            _inputActions.Emulation.L3.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.L3);
            _inputActions.Emulation.R1.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R);
            _inputActions.Emulation.R1.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R);
            _inputActions.Emulation.R2.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R2);
            _inputActions.Emulation.R2.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R2);
            _inputActions.Emulation.R3.started         += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R3);
            _inputActions.Emulation.R3.canceled        += ctx => JoypadCallback(RETRO_DEVICE_ID_JOYPAD.R3);
        }

        private void RegisterMouseCallbacks()
        {
            _inputActions.Emulation.MousePositionDelta.performed += ctx => (MouseX, MouseY) = ctx.ReadValue<Vector2>().ToShort();
            _inputActions.Emulation.MousePositionDelta.canceled  += ctx => MouseX = MouseY = 0;
            _inputActions.Emulation.MouseWheelDelta.performed    += ctx => MouseWheel = ctx.ReadValue<Vector2>().y.ToShort();
            _inputActions.Emulation.MouseWheelDelta.canceled     += ctx => MouseWheel = 0;
            _inputActions.Emulation.MouseLeftButton.started      += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.LEFT);
            _inputActions.Emulation.MouseLeftButton.canceled     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.LEFT);
            _inputActions.Emulation.MouseRightButton.started     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.RIGHT);
            _inputActions.Emulation.MouseRightButton.canceled    += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.RIGHT);
            _inputActions.Emulation.MouseMiddleButton.started    += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.MIDDLE);
            _inputActions.Emulation.MouseMiddleButton.canceled   += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.MIDDLE);
            _inputActions.Emulation.MouseForwardButton.started   += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.BUTTON_4);
            _inputActions.Emulation.MouseForwardButton.canceled  += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.BUTTON_4);
            _inputActions.Emulation.MouseBackButton.started      += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.BUTTON_5);
            _inputActions.Emulation.MouseBackButton.canceled     += ctx => MouseCallback(RETRO_DEVICE_ID_MOUSE.BUTTON_5);
        }

        private void RegisterLightgunCallbacks()
        {
            _inputActions.Emulation.LightgunTrigger.started       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.TRIGGER);
            _inputActions.Emulation.LightgunTrigger.canceled      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.TRIGGER);
            _inputActions.Emulation.LightgunReload.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.RELOAD);
            _inputActions.Emulation.LightgunReload.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.RELOAD);
            _inputActions.Emulation.LightgunA.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_A);
            _inputActions.Emulation.LightgunA.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_A);
            _inputActions.Emulation.LightgunB.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_B);
            _inputActions.Emulation.LightgunB.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_B);
            _inputActions.Emulation.LightgunStart.started         += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.START);
            _inputActions.Emulation.LightgunStart.canceled        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.START);
            _inputActions.Emulation.LightgunSelect.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.SELECT);
            _inputActions.Emulation.LightgunSelect.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.SELECT);
            _inputActions.Emulation.LightgunC.started             += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_C);
            _inputActions.Emulation.LightgunC.canceled            += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.AUX_C);
            _inputActions.Emulation.LightgunDPadUp.started        += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP);
            _inputActions.Emulation.LightgunDPadUp.canceled       += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP);
            _inputActions.Emulation.LightgunDPadDown.started      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN);
            _inputActions.Emulation.LightgunDPadDown.canceled     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN);
            _inputActions.Emulation.LightgunDPadLeft.started      += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT);
            _inputActions.Emulation.LightgunDPadLeft.canceled     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT);
            _inputActions.Emulation.LightgunDPadRight.started     += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT);
            _inputActions.Emulation.LightgunDPadRight.canceled    += ctx => LightgunCallback(RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT);
        }

        private void RegisterAnalogCallbacks()
        {
            _inputActions.Emulation.AnalogLeft.performed += ctx =>
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
            _inputActions.Emulation.AnalogLeft.canceled += ctx =>
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
            _inputActions.Emulation.AnalogRight.performed += ctx => (AnalogRightX, AnalogRightY) = ctx.ReadValue<Vector2>().ToShort(0x7fff);
            _inputActions.Emulation.AnalogRight.canceled  += ctx => AnalogRightX = AnalogRightY = 0;
        }
        private void JoypadCallback(in RETRO_DEVICE_ID_JOYPAD id) => _joypadButtons.ToggleBit((uint)id);
        private void MouseCallback(in RETRO_DEVICE_ID_MOUSE id) => _mouseButtons.ToggleBit((uint)id);
        private void LightgunCallback(in RETRO_DEVICE_ID_LIGHTGUN id) => _lightgunButtons.ToggleBit((uint)id);

        private void HandleKeyboardKeys()
        {
            for (uint i = 0; i < NUM_KEYBOARD_KEYS; ++i)
                _keyboardKeys.SetBit(i, UnityEngine.Input.GetKey((KeyCode)i));
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
