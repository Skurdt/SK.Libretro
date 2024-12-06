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

namespace SK.Libretro
{
    internal sealed class MemoryHandler
    {
        private retro_memory_map _memoryMap;

        public ReadOnlySpan<byte> GetSaveMemory() => GetRam(RETRO_MEMORY.SAVE_RAM);

        public ReadOnlySpan<byte> GetRtcMemory() => GetRam(RETRO_MEMORY.RTC);

        public ReadOnlySpan<byte> GetSystemMemory() => GetRam(RETRO_MEMORY.SYSTEM_RAM);

        public ReadOnlySpan<byte> GetVideoMemory() => GetRam(RETRO_MEMORY.VIDEO_RAM);

        public bool SetMemoryMaps(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _memoryMap = data.ToStructure<retro_memory_map>();
            return true;
        }

        private unsafe ReadOnlySpan<byte> GetRam(RETRO_MEMORY type)
        {
            int len = (int)Wrapper.Instance.Core.GetMemorySize(type);
            return len > 0
                 ? new(Wrapper.Instance.Core.GetMemoryData(type).ToPointer(), len)
                 : ReadOnlySpan<byte>.Empty;
        }
    }
}
