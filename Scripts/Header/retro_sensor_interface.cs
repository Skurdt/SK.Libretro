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

using System;
using System.Runtime.InteropServices;

namespace SK.Libretro.Header
{
    // typedef bool (RETRO_CALLCONV* retro_set_sensor_state_t) (unsigned port, enum retro_sensor_action action, unsigned rate);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_set_sensor_state_t(uint port, retro_sensor_action action, uint rate);
    // typedef float (RETRO_CALLCONV* retro_sensor_get_input_t) (unsigned port, unsigned id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate float retro_sensor_get_input_t(uint port, uint id);

    internal struct retro_sensor_interface
    {
        public IntPtr set_sensor_state; // retro_set_sensor_state_t
        public IntPtr get_sensor_input; // retro_sensor_get_input_t
    }
}
