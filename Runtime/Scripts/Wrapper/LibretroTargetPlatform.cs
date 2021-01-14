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

namespace SK.Libretro
{
    // Mimic Unity's RuntimePlatform
    internal enum LibretroTargetPlatform
    {
        OSXEditor          = 0,
        OSXPlayer          = 1,
        WindowsPlayer      = 2,
        OSXWebPlayer       = 3,
        OSXDashboardPlayer = 4,
        WindowsWebPlayer   = 5,
        WindowsEditor      = 7,
        IPhonePlayer       = 8,
        PS3                = 9,
        XBOX360            = 10,
        Android            = 11,
        NaCl               = 12,
        LinuxPlayer        = 13,
        FlashPlayer        = 15,
        LinuxEditor        = 16,
        WebGLPlayer        = 17,
        MetroPlayerX86     = 18,
        WSAPlayerX86       = 18,
        MetroPlayerX64     = 19,
        WSAPlayerX64       = 19,
        MetroPlayerARM     = 20,
        WSAPlayerARM       = 20,
        WP8Player          = 21,
        BB10Player         = 22,
        BlackBerryPlayer   = 22,
        TizenPlayer        = 23,
        PSP2               = 24,
        PS4                = 25,
        PSM                = 26,
        XboxOne            = 27,
        SamsungTVPlayer    = 28,
        WiiU               = 30,
        tvOS               = 31,
        Switch             = 32,
        Lumin              = 33,
        Stadia             = 34
    }
}
