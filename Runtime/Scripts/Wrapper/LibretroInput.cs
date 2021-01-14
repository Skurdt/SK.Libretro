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
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroInput
    {
        public IInputProcessor Processor;

        public const int MAX_USERS         = 16;
        public const int FIRST_CUSTOM_BIND = 16;
        public const int FIRST_META_KEY    = (int)CustomBinds.CUSTOM_BIND_LIST_END;

        private const int FIRST_LIGHTGUN_BIND    = (int)CustomBinds.ANALOG_BIND_LIST_END;
        private const int FIRST_MISC_CUSTOM_BIND = (int)CustomBinds.LIGHTGUN_BIND_LIST_END;

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

        public void PollCallback()
        {
        }

        public short StateCallback(uint port, uint device, uint index, uint id)
        {
            if (Processor == null)
                return 0;

            switch (device)
            {
                case RETRO_DEVICE_JOYPAD:   return ProcessJoypadDeviceState(port, id);
                case RETRO_DEVICE_MOUSE:    return ProcessMouseDeviceState(port, id);
                case RETRO_DEVICE_KEYBOARD: return ProcessKeyboardDeviceState(port, id);
                case RETRO_DEVICE_ANALOG:   return ProcessAnalogDeviceState(port, index, id);
                case RETRO_DEVICE_LIGHTGUN:
                case RETRO_DEVICE_POINTER:
                default:
                    return 0;
            }
        }

        private short ProcessJoypadDeviceState(uint port, uint button) => BoolToShort(Processor.JoypadButton((int)port, (int)button));

        private short ProcessMouseDeviceState(uint port, uint command)
        {
            switch (command)
            {
                case RETRO_DEVICE_ID_MOUSE_X:               return FloatToShort(Processor.MouseDeltaX((int)port));
                case RETRO_DEVICE_ID_MOUSE_Y:               return FloatToShort(Processor.MouseDeltaY((int)port));
                case RETRO_DEVICE_ID_MOUSE_LEFT:            return BoolToShort(Processor.MouseButton((int)port, 0));
                case RETRO_DEVICE_ID_MOUSE_RIGHT:           return BoolToShort(Processor.MouseButton((int)port, 1));
                case RETRO_DEVICE_ID_MOUSE_WHEELUP:
                case RETRO_DEVICE_ID_MOUSE_WHEELDOWN:       return FloatToShort(Processor.MouseWheelDeltaY((int)port));
                case RETRO_DEVICE_ID_MOUSE_MIDDLE:          return BoolToShort(Processor.MouseButton((int)port, 2));
                case RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELUP:
                case RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELDOWN: return FloatToShort(Processor.MouseWheelDeltaX((int)port));
                case RETRO_DEVICE_ID_MOUSE_BUTTON_4:        return BoolToShort(Processor.MouseButton((int)port, 3));
                case RETRO_DEVICE_ID_MOUSE_BUTTON_5:        return BoolToShort(Processor.MouseButton((int)port, 4));
                default:
                    return 0;
            }
        }

        private short ProcessKeyboardDeviceState(uint port, uint key) => BoolToShort(key < (int)retro_key.RETROK_OEM_102 && Processor.KeyboardKey((int)port, (int)key));

        private short ProcessAnalogDeviceState(uint port, uint index, uint axis)
        {
            switch (index)
            {
                case RETRO_DEVICE_INDEX_ANALOG_LEFT:
                    switch (axis)
                    {
                        case RETRO_DEVICE_ID_ANALOG_X: return FloatToShort(Processor.AnalogLeftValueX((int)port) * 0x8000);
                        case RETRO_DEVICE_ID_ANALOG_Y: return FloatToShort(Processor.AnalogLeftValueY((int)port) * 0x8000);
                        default:
                            break;
                    }
                    break;
                case RETRO_DEVICE_INDEX_ANALOG_RIGHT:
                    switch (axis)
                    {
                        case RETRO_DEVICE_ID_ANALOG_X: return FloatToShort(Processor.AnalogRightValueX((int)port) * 0x8000);
                        case RETRO_DEVICE_ID_ANALOG_Y: return FloatToShort(Processor.AnalogRightValueY((int)port) * 0x8000);
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

        private static short FloatToShort(float floatValue) => (short)math.clamp(math.round(floatValue), short.MinValue, short.MaxValue);
    }
}
