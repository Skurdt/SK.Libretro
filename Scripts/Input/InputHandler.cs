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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class InputHandler
    {
        private const int MAX_USERS_SUPPORTED = 2;

        private const int MAX_USERS              = 16;
        private const int FIRST_CUSTOM_BIND      = 16;
        private const int FIRST_LIGHTGUN_BIND    = (int)CustomBinds.ANALOG_BIND_LIST_END;
        private const int FIRST_MISC_CUSTOM_BIND = (int)CustomBinds.LIGHTGUN_BIND_LIST_END;
        private const int FIRST_META_KEY         = (int)CustomBinds.CUSTOM_BIND_LIST_END;

        private enum CustomBinds : uint
        {
            // Analogs (RETRO_DEVICE_ANALOG)
            ANALOG_LEFT_X_PLUS = FIRST_CUSTOM_BIND,
            ANALOG_LEFT_X_MINUS,
            ANALOG_LEFT_Y_PLUS,
            ANALOG_LEFT_Y_MINUS,
            ANALOG_RIGHT_X_PLUS,
            ANALOG_RIGHT_X_MINUS,
            ANALOG_RIGHT_Y_PLUS,
            ANALOG_RIGHT_Y_MINUS,
            ANALOG_BIND_LIST_END,

            // Lightgun
            LIGHTGUN_TRIGGER = FIRST_LIGHTGUN_BIND,
            LIGHTGUN_RELOAD,
            LIGHTGUN_AUX_A,
            LIGHTGUN_AUX_B,
            LIGHTGUN_AUX_C,
            LIGHTGUN_START,
            LIGHTGUN_SELECT,
            LIGHTGUN_DPAD_UP,
            LIGHTGUN_DPAD_DOWN,
            LIGHTGUN_DPAD_LEFT,
            LIGHTGUN_DPAD_RIGHT,
            LIGHTGUN_BIND_LIST_END,

            // Turbo
            TURBO_ENABLE = FIRST_MISC_CUSTOM_BIND,

            CUSTOM_BIND_LIST_END,

            // Command binds. Not related to game input, only usable for port 0.
            FAST_FORWARD_KEY = FIRST_META_KEY,
            FAST_FORWARD_HOLD_KEY,
            SLOWMOTION_KEY,
            SLOWMOTION_HOLD_KEY,
            LOAD_STATE_KEY,
            SAVE_STATE_KEY,
            FULLSCREEN_TOGGLE_KEY,
            QUIT_KEY,
            STATE_SLOT_PLUS,
            STATE_SLOT_MINUS,
            REWIND,
            BSV_RECORD_TOGGLE,
            PAUSE_TOGGLE,
            FRAMEADVANCE,
            RESET,
            SHADER_NEXT,
            SHADER_PREV,
            CHEAT_INDEX_PLUS,
            CHEAT_INDEX_MINUS,
            CHEAT_TOGGLE,
            SCREENSHOT,
            MUTE,
            OSK,
            FPS_TOGGLE,
            SEND_DEBUG_INFO,
            NETPLAY_HOST_TOGGLE,
            NETPLAY_GAME_WATCH,
            ENABLE_HOTKEY,
            VOLUME_UP,
            VOLUME_DOWN,
            OVERLAY_NEXT,
            DISK_EJECT_TOGGLE,
            DISK_NEXT,
            DISK_PREV,
            GRAB_MOUSE_TOGGLE,
            GAME_FOCUS_TOGGLE,
            UI_COMPANION_TOGGLE,

            MENU_TOGGLE,

            RECORDING_TOGGLE,
            STREAMING_TOGGLE,

            AI_SERVICE,

            BIND_LIST_END,
            BIND_LIST_END_NULL
        };

        public bool Enabled { get; set; }
        public bool HasInputDescriptors { get; private set; }

        public readonly ControllersMap DeviceMap = new();

        private static readonly retro_input_poll_t _pollCallback         = PollCallback;
        private static readonly retro_input_state_t _stateCallback       = StateCallback;
        private static readonly retro_set_rumble_state_t _setRumbleState = SetRumbleState;

        private readonly IInputProcessor _processor;

        private readonly List<retro_input_descriptor> _inputDescriptors = new();
        private readonly string[,] _buttonDescriptions = new string[MAX_USERS, FIRST_META_KEY];

        private retro_keyboard_event_t _keyboardEvent;

        public InputHandler(IInputProcessor processor) => _processor = processor ?? new NullInputProcessor();

        public void SetCoreCallbacks(retro_set_input_poll_t setInputPoll, retro_set_input_state_t setInputState)
        {
            setInputPoll(_pollCallback);
            setInputState(_stateCallback);
        }

        public bool GetRumbleInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_rumble_interface rumbleInterface = data.ToStructure<retro_rumble_interface>();
            rumbleInterface.set_rumble_state = _setRumbleState.GetFunctionPointer();
            Marshal.StructureToPtr(rumbleInterface, data, false);
            return true;
        }

        public bool GetInputDeviceCapabilities(IntPtr data)
        {
            if (data.IsNull())
                return false;
            
            ulong bits = (1 << (int)RETRO_DEVICE.JOYPAD)
                       | (1 << (int)RETRO_DEVICE.MOUSE)
                       | (1 << (int)RETRO_DEVICE.KEYBOARD)
                       | (1 << (int)RETRO_DEVICE.LIGHTGUN)
                       | (1 << (int)RETRO_DEVICE.ANALOG)
                       | (1 << (int)RETRO_DEVICE.POINTER);

            data.Write(bits);
            return true;
        }

        public bool GetInputBitmasks(IntPtr data)
        {
            if (data.IsNull())
                return false;

            data.Write(true);
            return true;
        }

        public bool GetInputMaxUsers(IntPtr data)
        {
            if (data.IsNull())
                return false;

            data.Write(MAX_USERS_SUPPORTED);
            return true;
        }

        public bool SetInputDescriptors(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _inputDescriptors.Clear();

            retro_input_descriptor descriptor = data.ToStructure<retro_input_descriptor>();
            while (descriptor is not null && !descriptor.desc.IsNull())
            {
                _inputDescriptors.Add(new()
                {
                    port   = descriptor.port,
                    device = descriptor.device,
                    index  = descriptor.index,
                    id     = descriptor.id,
                    desc   = descriptor.desc
                });

                if (descriptor.device is RETRO_DEVICE.JOYPAD)
                    _buttonDescriptions[descriptor.port, descriptor.id] = descriptor.desc.AsString();
                else if (descriptor.device is RETRO_DEVICE.ANALOG)
                {
                    RETRO_DEVICE_ID_ANALOG id = (RETRO_DEVICE_ID_ANALOG)descriptor.id;
                    switch (id)
                    {
                        case RETRO_DEVICE_ID_ANALOG.X:
                        {
                            RETRO_DEVICE_INDEX_ANALOG index = (RETRO_DEVICE_INDEX_ANALOG)descriptor.index;
                            switch (index)
                            {
                                case RETRO_DEVICE_INDEX_ANALOG.LEFT:
                                {
                                    string desc = descriptor.desc.AsString();
                                    _buttonDescriptions[descriptor.port, (int)CustomBinds.ANALOG_LEFT_X_PLUS] = desc;
                                    _buttonDescriptions[descriptor.port, (int)CustomBinds.ANALOG_LEFT_X_MINUS] = desc;
                                }
                                break;
                                case RETRO_DEVICE_INDEX_ANALOG.RIGHT:
                                {
                                    string desc = descriptor.desc.AsString();
                                    _buttonDescriptions[descriptor.port, (int)CustomBinds.ANALOG_RIGHT_X_PLUS] = desc;
                                    _buttonDescriptions[descriptor.port, (int)CustomBinds.ANALOG_RIGHT_X_MINUS] = desc;
                                }
                                break;
                            }
                        }
                        break;
                        case RETRO_DEVICE_ID_ANALOG.Y:
                        {
                            RETRO_DEVICE_INDEX_ANALOG index = (RETRO_DEVICE_INDEX_ANALOG)descriptor.index;
                            switch (index)
                            {
                                case RETRO_DEVICE_INDEX_ANALOG.LEFT:
                                {
                                    string desc = descriptor.desc.AsString();
                                    _buttonDescriptions[descriptor.port, (int)CustomBinds.ANALOG_LEFT_Y_PLUS] = desc;
                                    _buttonDescriptions[descriptor.port, (int)CustomBinds.ANALOG_LEFT_Y_MINUS] = desc;
                                }
                                break;
                                case RETRO_DEVICE_INDEX_ANALOG.RIGHT:
                                {
                                    string desc = descriptor.desc.AsString();
                                    _buttonDescriptions[descriptor.port, (int)CustomBinds.ANALOG_RIGHT_Y_PLUS] = desc;
                                    _buttonDescriptions[descriptor.port, (int)CustomBinds.ANALOG_RIGHT_Y_MINUS] = desc;
                                }
                                break;
                            }
                        }
                        break;
                    }
                }

                data += Marshal.SizeOf(descriptor);
                data.ToStructure(descriptor);
            }

            HasInputDescriptors = _inputDescriptors.Count > 0;

            return true;
        }

        public bool SetKeyboardCallback(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_keyboard_callback callback = data.ToStructure<retro_keyboard_callback>();
            _keyboardEvent = callback.callback.GetDelegate<retro_keyboard_event_t>();
            return true;
        }

        public bool SetControllerInfo(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_controller_info controllerInfo = data.ToStructure<retro_controller_info>();
            int index = 0;
            while (controllerInfo.types.IsNotNull())
            {
                for (int deviceIndex = 0; deviceIndex < controllerInfo.num_types; ++deviceIndex)
                {
                    retro_controller_description controllerDescription = controllerInfo.types.ToStructure<retro_controller_description>();
                    if (controllerDescription.desc.IsNotNull())
                        DeviceMap.Add(index, new() { Description = controllerDescription.desc.AsString(), Device = controllerDescription.id });
                    
                    controllerInfo.types += Marshal.SizeOf(controllerDescription);
                    controllerInfo.types.ToStructure(controllerDescription);
                }

                data += Marshal.SizeOf(controllerInfo);
                data.ToStructure(controllerInfo);
                index++;
            }

            return true;
        }

        [MonoPInvokeCallback(typeof(retro_input_poll_t))]
        private static void PollCallback() { }

        [MonoPInvokeCallback(typeof(retro_input_state_t))]
        private static short StateCallback(uint port, RETRO_DEVICE device, uint index, uint id)
        {
            if (!Wrapper.Instance.InputHandler.Enabled)
                return 0;

            device &= (RETRO_DEVICE)RETRO.DEVICE_MASK;

            return device switch
            {
                RETRO_DEVICE.JOYPAD   => Wrapper.Instance.InputHandler.ProcessJoypadDevice(port, (RETRO_DEVICE_ID_JOYPAD)id),
                RETRO_DEVICE.MOUSE    => Wrapper.Instance.InputHandler.ProcessMouseDevice(port, (RETRO_DEVICE_ID_MOUSE)id),
                RETRO_DEVICE.KEYBOARD => Wrapper.Instance.InputHandler.ProcessKeyboardDevice(port, (retro_key)id),
                RETRO_DEVICE.LIGHTGUN => Wrapper.Instance.InputHandler.ProcessLightgunDevice(port, (RETRO_DEVICE_ID_LIGHTGUN)id),
                RETRO_DEVICE.POINTER  => Wrapper.Instance.InputHandler.ProcessPointerDevice(port, (RETRO_DEVICE_ID_POINTER)id),
                RETRO_DEVICE.ANALOG   => Wrapper.Instance.InputHandler.ProcessAnalogDevice(port, (RETRO_DEVICE_INDEX_ANALOG)index, (RETRO_DEVICE_ID_ANALOG)id),
                _ => 0
            };
        }

        private short ProcessJoypadDevice(uint port, RETRO_DEVICE_ID_JOYPAD id) => id is RETRO_DEVICE_ID_JOYPAD.MASK
                                                                                 ? _processor.JoypadButtons((int)port)
                                                                                 : _processor.JoypadButton((int)port, id);

        private short ProcessMouseDevice(uint port, RETRO_DEVICE_ID_MOUSE id) => id switch
        {
            RETRO_DEVICE_ID_MOUSE.X => _processor.MouseX((int)port),
            RETRO_DEVICE_ID_MOUSE.Y => _processor.MouseY((int)port),

            RETRO_DEVICE_ID_MOUSE.WHEELUP
            or RETRO_DEVICE_ID_MOUSE.WHEELDOWN => _processor.MouseWheel((int)port),

            RETRO_DEVICE_ID_MOUSE.LEFT
            or RETRO_DEVICE_ID_MOUSE.RIGHT
            or RETRO_DEVICE_ID_MOUSE.MIDDLE
            or RETRO_DEVICE_ID_MOUSE.BUTTON_4
            or RETRO_DEVICE_ID_MOUSE.BUTTON_5 => _processor.MouseButton((int)port, id),

            _ => 0
        };

        private short ProcessKeyboardDevice(uint port, retro_key id) => id < retro_key.RETROK_OEM_102
                                                                      ? _processor.KeyboardKey((int)port, id)
                                                                      : (short)0;

        private short ProcessLightgunDevice(uint port, RETRO_DEVICE_ID_LIGHTGUN id) => id switch
        {
            RETRO_DEVICE_ID_LIGHTGUN.X
            or RETRO_DEVICE_ID_LIGHTGUN.SCREEN_X   => _processor.LightgunX((int)port),
                                                   
            RETRO_DEVICE_ID_LIGHTGUN.Y             
            or RETRO_DEVICE_ID_LIGHTGUN.SCREEN_Y   => _processor.LightgunY((int)port),
                                                   
            RETRO_DEVICE_ID_LIGHTGUN.IS_OFFSCREEN  => BoolToShort(_processor.LightgunIsOffscreen((int)port)),

            RETRO_DEVICE_ID_LIGHTGUN.TRIGGER
            or RETRO_DEVICE_ID_LIGHTGUN.RELOAD
            or RETRO_DEVICE_ID_LIGHTGUN.AUX_A
            or RETRO_DEVICE_ID_LIGHTGUN.AUX_B
            or RETRO_DEVICE_ID_LIGHTGUN.START
            or RETRO_DEVICE_ID_LIGHTGUN.SELECT
            or RETRO_DEVICE_ID_LIGHTGUN.AUX_C
            or RETRO_DEVICE_ID_LIGHTGUN.DPAD_UP
            or RETRO_DEVICE_ID_LIGHTGUN.DPAD_DOWN
            or RETRO_DEVICE_ID_LIGHTGUN.DPAD_LEFT
            or RETRO_DEVICE_ID_LIGHTGUN.DPAD_RIGHT => _processor.LightgunButton((int)port, id),

            _ => 0
        };

        private short ProcessPointerDevice(uint port, RETRO_DEVICE_ID_POINTER id) => id switch
        {
            RETRO_DEVICE_ID_POINTER.X       => _processor.PointerX((int)port),
            RETRO_DEVICE_ID_POINTER.Y       => _processor.PointerY((int)port),
            RETRO_DEVICE_ID_POINTER.PRESSED => _processor.PointerPressed((int)port),
            RETRO_DEVICE_ID_POINTER.COUNT   => _processor.PointerCount((int)port),
            _ => 0
        };

        private short ProcessAnalogDevice(uint port, RETRO_DEVICE_INDEX_ANALOG index, RETRO_DEVICE_ID_ANALOG id) => index switch
        {
            RETRO_DEVICE_INDEX_ANALOG.LEFT => id switch
            {
                RETRO_DEVICE_ID_ANALOG.X => _processor.AnalogLeftX((int)port),
                RETRO_DEVICE_ID_ANALOG.Y => _processor.AnalogLeftY((int)port),
                _ => 0,
            },
            RETRO_DEVICE_INDEX_ANALOG.RIGHT => id switch
            {
                RETRO_DEVICE_ID_ANALOG.X => _processor.AnalogRightX((int)port),
                RETRO_DEVICE_ID_ANALOG.Y => _processor.AnalogRightY((int)port),
                _ => 0,
            },
            _ => 0
        };

        [MonoPInvokeCallback(typeof(retro_set_rumble_state_t))]
        private static bool SetRumbleState(uint port, retro_rumble_effect effect, ushort strength)
            => Wrapper.Instance.InputHandler._processor.SetRumbleState((int)port, effect, strength);

        private static short BoolToShort(bool boolValue) => (short)(boolValue ? 1 : 0);
    }
}
