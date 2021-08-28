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

using static SK.Libretro.Header;

namespace SK.Libretro
{
    internal sealed class Input
    {
        public const int MAX_USERS_SUPPORTED = 2;

        public const int MAX_USERS              = 16;
        public const int FIRST_CUSTOM_BIND      = 16;
        public const int FIRST_LIGHTGUN_BIND    = (int)CustomBinds.ANALOG_BIND_LIST_END;
        public const int FIRST_MISC_CUSTOM_BIND = (int)CustomBinds.LIGHTGUN_BIND_LIST_END;
        public const int FIRST_META_KEY         = (int)CustomBinds.CUSTOM_BIND_LIST_END;

        public enum CustomBinds : uint
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

        public readonly ControllersMap DeviceMap = new ControllersMap();

        public readonly string[,] ButtonDescriptions = new string[MAX_USERS, FIRST_META_KEY];
        public bool HasInputDescriptors;

        public readonly retro_rumble_interface RumbleInterface = new retro_rumble_interface
        {
            set_rumble_state = (uint port, retro_rumble_effect effect, ushort strength) =>
            {
                Logger.Instance.LogDebug($"[Rumble] Port: {port} Effect: {effect} Strength: {strength}");
                return true;
            }
        };

        public retro_keyboard_callback KeyboardCallback;

        public bool Enabled { get; set; }

        private IInputProcessor _processor;

        public void Init(IInputProcessor inputProcessor) => _processor = inputProcessor;

        public void DeInit() => _processor = null;

        public void PollCallback()
        {
        }

        public short StateCallback(uint port, uint device, uint index, uint id)
        {
            if (_processor == null || !Enabled)
                return 0;

            //device &= RETRO_DEVICE_MASK;
            return device switch
            {
                RETRO_DEVICE_JOYPAD   => ProcessJoypadDevice(port, id),
                RETRO_DEVICE_MOUSE    => ProcessMouseDevice(port, id),
                RETRO_DEVICE_KEYBOARD => ProcessKeyboardDevice(port, id),
                RETRO_DEVICE_LIGHTGUN => ProcessLightgunDevice(port, id),
                RETRO_DEVICE_POINTER  => ProcessLightgunDevice(port, id),
                RETRO_DEVICE_ANALOG   => ProcessAnalogDevice(port, index, id),
                _ => 0,
            };
        }

        private short ProcessJoypadDevice(uint port, uint id)
            => id == RETRO_DEVICE_ID_JOYPAD_MASK ? _processor.JoypadButtons((int)port) : _processor.JoypadButton((int)port, id);

        private short ProcessMouseDevice(uint port, uint id)
        {
            switch (id)
            {
                case RETRO_DEVICE_ID_MOUSE_X:               return _processor.MouseX((int)port);
                case RETRO_DEVICE_ID_MOUSE_Y:               return _processor.MouseY((int)port);
                case RETRO_DEVICE_ID_MOUSE_WHEELUP:
                case RETRO_DEVICE_ID_MOUSE_WHEELDOWN:       return _processor.MouseWheel((int)port);
                case RETRO_DEVICE_ID_MOUSE_LEFT:
                case RETRO_DEVICE_ID_MOUSE_RIGHT:
                case RETRO_DEVICE_ID_MOUSE_MIDDLE:
                case RETRO_DEVICE_ID_MOUSE_BUTTON_4:
                case RETRO_DEVICE_ID_MOUSE_BUTTON_5:        return _processor.MouseButton((int)port, id);
                case RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELUP:
                case RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELDOWN:
                default:
                    break;
            }
            return 0;
        }

        private short ProcessKeyboardDevice(uint port, uint id) => id < (int)retro_key.RETROK_OEM_102 ? _processor.KeyboardKey((int)port, id) : (short)0;

        private short ProcessLightgunDevice(uint port, uint id)
        {
            switch (id)
            {
                case RETRO_DEVICE_ID_LIGHTGUN_X:
                case RETRO_DEVICE_ID_LIGHTGUN_SCREEN_X:     return _processor.LightgunX((int)port);
                case RETRO_DEVICE_ID_LIGHTGUN_Y:
                case RETRO_DEVICE_ID_LIGHTGUN_SCREEN_Y:     return _processor.LightgunY((int)port);
                case RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN: return BoolToShort(_processor.LightgunIsOffscreen((int)port));
                case RETRO_DEVICE_ID_LIGHTGUN_TRIGGER:
                case RETRO_DEVICE_ID_LIGHTGUN_RELOAD:
                case RETRO_DEVICE_ID_LIGHTGUN_AUX_A:
                case RETRO_DEVICE_ID_LIGHTGUN_AUX_B:
                case RETRO_DEVICE_ID_LIGHTGUN_START:
                case RETRO_DEVICE_ID_LIGHTGUN_SELECT:
                case RETRO_DEVICE_ID_LIGHTGUN_AUX_C:
                case RETRO_DEVICE_ID_LIGHTGUN_DPAD_UP:
                case RETRO_DEVICE_ID_LIGHTGUN_DPAD_DOWN:
                case RETRO_DEVICE_ID_LIGHTGUN_DPAD_LEFT:
                case RETRO_DEVICE_ID_LIGHTGUN_DPAD_RIGHT:   return _processor.LightgunButton((int)port, id);
                default:
                    break;
            }
            return 0;
        }

        private short ProcessAnalogDevice(uint port, uint index, uint id)
        {
            switch (index)
            {
                case RETRO_DEVICE_INDEX_ANALOG_LEFT:
                    switch (id)
                    {
                        case RETRO_DEVICE_ID_ANALOG_X: return _processor.AnalogLeftX((int)port);
                        case RETRO_DEVICE_ID_ANALOG_Y: return _processor.AnalogLeftY((int)port);
                        default:
                            break;
                    }
                    break;
                case RETRO_DEVICE_INDEX_ANALOG_RIGHT:
                    switch (id)
                    {
                        case RETRO_DEVICE_ID_ANALOG_X: return _processor.AnalogRightX((int)port);
                        case RETRO_DEVICE_ID_ANALOG_Y: return _processor.AnalogRightY((int)port);
                        default:
                            break;
                    }
                    break;
                case RETRO_DEVICE_INDEX_ANALOG_BUTTON:
                default:
                    break;
            }

            return 0;
        }

        private static short BoolToShort(bool boolValue) => (short)(boolValue ? 1 : 0);
    }
}
