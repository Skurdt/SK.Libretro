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

namespace SK.Libretro.Header
{
    public enum RETRO_DEVICE_ID_LIGHTGUN
    {
        SCREEN_X     = 13,
        SCREEN_Y     = 14,
        IS_OFFSCREEN = 15,
        TRIGGER      = 2,
        RELOAD       = 16,
        AUX_A        = 3,
        AUX_B        = 4,
        START        = 6,
        SELECT       = 7,
        AUX_C        = 8,
        DPAD_UP      = 9,
        DPAD_DOWN    = 10,
        DPAD_LEFT    = 11,
        DPAD_RIGHT   = 12,

        X            = 0,
        Y            = 1,
        CURSOR       = 3,
        TURBO        = 4,
        PAUSE        = 5
    }
}
