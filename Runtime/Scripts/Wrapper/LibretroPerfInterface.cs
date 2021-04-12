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

using SK.Libretro.Utilities;
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroPerfInterface
    {
        public long GetTimeUsec()
        {
            Logger.Instance.LogWarning("RetroPerfGetTimeUsec");
            return 0;
        }

        public ulong GetCounter()
        {
            Logger.Instance.LogWarning("RetroPerfGetCounter");
            return 0;
        }

        public ulong GetCPUFeatures()
        {
            Logger.Instance.LogWarning("RetroGetCPUFeatures");
            return 0;
        }

        public void Log() => Logger.Instance.LogWarning("RetroPerfLog");

        public void Register(ref retro_perf_counter _/*counter*/) => Logger.Instance.LogWarning("RetroPerfRegister");

        public void RetroPerfStart(ref retro_perf_counter _/*counter*/) => Logger.Instance.LogWarning("RetroPerfStart");

        public void RetroPerfStop(ref retro_perf_counter _/*counter*/) => Logger.Instance.LogWarning("RetroPerfStop");
    }
}
