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
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class PerfHandler
    {
        private static readonly retro_perf_get_time_usec_t _get_time_usec  = GetTimeUsec;
        private static readonly retro_get_cpu_features_t _get_cpu_features = GetCpuFeatures;         
        private static readonly retro_perf_get_counter_t _get_perf_counter = GetPerfCounter;         
        private static readonly retro_perf_register_t _perf_register       = PerfRegister;   
        private static readonly retro_perf_start_t _perf_start             = PerfStart;
        private static readonly retro_perf_stop_t _perf_stop               = PerfStop;
        private static readonly retro_perf_log_t _perf_log                 = PerfLog;

        [MonoPInvokeCallback(typeof(retro_perf_get_time_usec_t))]
        private static long GetTimeUsec() => 0;

        [MonoPInvokeCallback(typeof(retro_get_cpu_features_t))]
        private static ulong GetCpuFeatures() => 0;

        [MonoPInvokeCallback(typeof(retro_perf_get_counter_t))]
        private static ulong GetPerfCounter() => 0;

        [MonoPInvokeCallback(typeof(retro_perf_register_t))]
        private static void PerfRegister(ref retro_perf_counter counter) { }

        [MonoPInvokeCallback(typeof(retro_perf_start_t))]
        private static void PerfStart(ref retro_perf_counter counter) { }

        [MonoPInvokeCallback(typeof(retro_perf_stop_t))]
        private static void PerfStop(ref retro_perf_counter counter) { }

        [MonoPInvokeCallback(typeof(retro_perf_log_t))]
        private static void PerfLog() { }

        public bool GetPerfInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_perf_callback callback = data.ToStructure<retro_perf_callback>();

            callback.get_time_usec    = _get_time_usec.GetFunctionPointer();
            callback.get_cpu_features = _get_cpu_features.GetFunctionPointer();
            callback.get_perf_counter = _get_perf_counter.GetFunctionPointer();
            callback.perf_register    = _perf_register.GetFunctionPointer();
            callback.perf_start       = _perf_start.GetFunctionPointer();
            callback.perf_stop        = _perf_stop.GetFunctionPointer();
            callback.perf_log         = _perf_log.GetFunctionPointer();

            Marshal.StructureToPtr(callback, data, false);
            return true;
        }
    }
}
