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
    // typedef retro_time_t (RETRO_CALLCONV *retro_perf_get_time_usec_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long retro_perf_get_time_usec_t();
    // typedef retro_perf_tick_t (RETRO_CALLCONV *retro_perf_get_counter_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate ulong retro_perf_get_counter_t();
    // typedef uint64_t (RETRO_CALLCONV *retro_get_cpu_features_t)(void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate ulong retro_get_cpu_features_t();
    // typedef void (RETRO_CALLCONV *retro_perf_log_t) (void);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_perf_log_t();
    // typedef void (RETRO_CALLCONV *retro_perf_register_t)(struct retro_perf_counter *counter);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_perf_register_t(ref retro_perf_counter counter);
    // typedef void (RETRO_CALLCONV *retro_perf_start_t)(struct retro_perf_counter *counter);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_perf_start_t(ref retro_perf_counter counter);
    // typedef void (RETRO_CALLCONV *retro_perf_stop_t)(struct retro_perf_counter *counter);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void retro_perf_stop_t(ref retro_perf_counter counter);

    internal struct retro_perf_callback
    {
        public IntPtr get_time_usec;    // retro_perf_get_time_usec_t
        public IntPtr get_cpu_features; // retro_get_cpu_features_t
        public IntPtr get_perf_counter; // retro_perf_get_counter_t
        public IntPtr perf_register;    // retro_perf_register_t
        public IntPtr perf_start;       // retro_perf_start_t
        public IntPtr perf_stop;        // retro_perf_stop_t
        public IntPtr perf_log;         // retro_perf_log_t
    }
}
