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
        LeftStickBehaviour LeftStickBehaviour { get; set; }

        short JoypadButton(int port, RETRO_DEVICE_ID_JOYPAD button);
        short JoypadButtons(int port);

        short MouseX(int port);
        short MouseY(int port);
        short MouseWheel(int port);
        short MouseButton(int port, RETRO_DEVICE_ID_MOUSE button);

        short KeyboardKey(int port, retro_key key);

        short LightgunX(int port);
        short LightgunY(int port);
        bool LightgunIsOffscreen(int port);
        short LightgunButton(int port, RETRO_DEVICE_ID_LIGHTGUN button);

        short AnalogLeftX(int port);
        short AnalogLeftY(int port);
        short AnalogRightX(int port);
        short AnalogRightY(int port);

        short PointerX(int port);
        short PointerY(int port);
        short PointerPressed(int port);
        short PointerCount(int port);

        bool SetRumbleState(int port, retro_rumble_effect effect, ushort strength);
    }
}
