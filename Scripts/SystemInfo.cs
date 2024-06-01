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
    internal sealed class SystemInfo
    {
        public readonly string LibraryName;
        public readonly string LibraryVersion;
        public readonly string[] ValidExtensions;
        public readonly bool NeedFullPath;
        public readonly bool BlockExtract;

        public SystemInfo(retro_system_info systemInfo)
        {
            LibraryName     = systemInfo.library_name.AsString();
            LibraryVersion  = systemInfo.library_version.AsString();
            ValidExtensions = systemInfo.valid_extensions.AsString().Split('|');
            NeedFullPath    = systemInfo.need_fullpath;
            BlockExtract    = systemInfo.block_extract;
        }
    }
}
