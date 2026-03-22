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

namespace SK.Libretro
{
    public interface IInputProcessor
    {
        public LeftStickBehaviour LeftStickBehaviour { get; set; }

        public short JoypadButton(int port, RETRO_DEVICE_ID_JOYPAD button);
        public short JoypadButtons(int port);

        public short MouseX(int port);
        public short MouseY(int port);
        public short MouseWheel(int port);
        public short MouseButton(int port, RETRO_DEVICE_ID_MOUSE button);

        public short KeyboardKey(int port, retro_key key);

        public short LightgunX(int port);
        public short LightgunY(int port);
        public bool LightgunIsOffscreen(int port);
        public short LightgunButton(int port, RETRO_DEVICE_ID_LIGHTGUN button);

        public short AnalogLeftX(int port);
        public short AnalogLeftY(int port);
        public short AnalogRightX(int port);
        public short AnalogRightY(int port);

        public short PointerX(int port);
        public short PointerY(int port);
        public short PointerPressed(int port);
        public short PointerCount(int port);

        public bool SetRumbleState(int port, retro_rumble_effect effect, ushort strength);
    }
}
