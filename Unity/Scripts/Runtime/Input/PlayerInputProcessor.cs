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
        public MouseHandler MouseHandler       { get; private set; }
        public KeyboardHandler KeyboardHandler { get; private set; }
        public LightgunHandler LightgunHandler { get; private set; }
        public AnalogHandler AnalogHandler     { get; private set; }
        public PointerHandler PointerHandler   { get; private set; }

        private LibretroInputActions _inputActions;

        public void Init(LeftStickBehaviour leftStickBehaviour)
        {
            JoypadHandler   = new();
            MouseHandler    = new();
            KeyboardHandler = new();
            LightgunHandler = new(_libretroInstanceVariable.Current);
            AnalogHandler   = new(JoypadHandler, leftStickBehaviour);
            PointerHandler  = new();

            _inputActions = new();
            _inputActions.Joypad.SetCallbacks(JoypadHandler);
            _inputActions.Mouse.SetCallbacks(MouseHandler);
            _inputActions.Keyboard.SetCallbacks(KeyboardHandler);
            _inputActions.Lightgun.SetCallbacks(LightgunHandler);
            _inputActions.Analog.SetCallbacks(AnalogHandler);
            _inputActions.Pointer.SetCallbacks(PointerHandler);
            _inputActions.Enable();
        }

        private void OnDestroy()
        {
            _inputActions.Disable();
            _inputActions.Joypad.RemoveCallbacks(JoypadHandler);
            _inputActions.Mouse.RemoveCallbacks(MouseHandler);
            _inputActions.Keyboard.RemoveCallbacks(KeyboardHandler);
            _inputActions.Lightgun.RemoveCallbacks(LightgunHandler);
            _inputActions.Analog.RemoveCallbacks(AnalogHandler);
            _inputActions.Pointer.RemoveCallbacks(PointerHandler);
            _inputActions.Dispose();
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
