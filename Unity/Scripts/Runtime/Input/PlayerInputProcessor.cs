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
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    [RequireComponent(typeof(PlayerInput))]
    internal sealed class PlayerInputProcessor : MonoBehaviour
    {
        [SerializeField] private LibretroInstanceVariable _libretroInstanceVariable;

        public JoypadHandler JoypadHandler     { get; private set; }
        public AnalogHandler AnalogHandler     { get; private set; }
        public MouseHandler MouseHandler       { get; private set; }
        public KeyboardHandler KeyboardHandler { get; private set; }
        public LightgunHandler LightgunHandler { get; private set; }
        public PointerHandler PointerHandler   { get; private set; }

        private PlayerInput _playerInput;

        public void Init(LeftStickBehaviour leftStickBehaviour)
        {
            _playerInput = GetComponent<PlayerInput>();

            JoypadHandler   = new(_playerInput.actions.FindActionMap("Joypad"));
            AnalogHandler   = new(_playerInput.actions.FindActionMap("Analog"), JoypadHandler, leftStickBehaviour);
            MouseHandler    = new(_playerInput.actions.FindActionMap("Mouse"));
            KeyboardHandler = new(_playerInput.actions.FindActionMap("Keyboard"));
            LightgunHandler = new(_playerInput.actions.FindActionMap("Lightgun"), _libretroInstanceVariable.Current);
            PointerHandler  = new(_playerInput.actions.FindActionMap("Pointer"));
        }

        private void OnDestroy()
        {
            _playerInput.actions.Disable();
            JoypadHandler.Dispose();
            AnalogHandler.Dispose();
            MouseHandler.Dispose();
            KeyboardHandler.Dispose();
            LightgunHandler.Dispose();
            PointerHandler.Dispose();
        }

        private void Update()
        {
            KeyboardHandler.Update();
            LightgunHandler.Update();
        }

        public bool SetRumbleState(retro_rumble_effect effect, ushort strength)
        {
            float low  = effect is retro_rumble_effect.RETRO_RUMBLE_WEAK   ? strength / (float)ushort.MaxValue : 0f;
            float high = effect is retro_rumble_effect.RETRO_RUMBLE_STRONG ? strength / (float)ushort.MaxValue : 0f;
            Gamepad.current?.SetMotorSpeeds(low, high);
            return true;
        }
    }
}
