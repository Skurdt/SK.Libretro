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
    // typedef void (RETRO_CALLCONV *retro_location_set_interval_t) (unsigned interval_ms, unsigned interval_distance);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_location_set_interval_t(uint interval_ms, uint interval_distance);
    // typedef bool (RETRO_CALLCONV *retro_location_start_t) (void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_location_start_t();
    // typedef void (RETRO_CALLCONV *retro_location_stop_t) (void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_location_stop_t();
    // typedef bool (RETRO_CALLCONV *retro_location_get_position_t) (double* lat, double* lon, double* horiz_accuracy, double* vert_accuracy);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_location_get_position_t(ref double lat, ref double lon, ref double horiz_accuracy, ref double vert_accuracy);
    // typedef void (RETRO_CALLCONV *retro_location_lifetime_status_t) (void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_location_lifetime_status_t();

    internal struct retro_location_callback
    {
        public IntPtr start;         // retro_location_start_t
        public IntPtr stop;          // retro_location_stop_t
        public IntPtr get_position;  // retro_location_get_position_t
        public IntPtr set_interval;  // retro_location_set_interval_t

        public IntPtr initialized;   // retro_location_lifetime_status_t
        public IntPtr deinitialized; // retro_location_lifetime_status_t
    }
}
