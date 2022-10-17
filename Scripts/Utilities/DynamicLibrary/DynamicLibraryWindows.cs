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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class DynamicLibraryWindows : DynamicLibrary
    {
        [DllImport("kernel32", EntryPoint = "LoadLibraryA", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr PlatformLoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr PlatformGetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32", EntryPoint = "FreeLibrary", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool PlatformFreeLibrary(IntPtr hModule);

        public DynamicLibraryWindows(bool deleteFileOnDispose)
        : base("dll", deleteFileOnDispose)
        {
        }

        protected override void LoadLibrary()
        {
            _nativeHandle = PlatformLoadLibrary(Path);
            if (_nativeHandle.IsNull())
                throw new Exception(GetLastErrorMessage());
        }

        protected override IntPtr GetProcAddress(string functionName)
        {
            IntPtr procAddress = PlatformGetProcAddress(_nativeHandle, functionName);
            return procAddress.IsNotNull() ? procAddress : throw new Exception(GetLastErrorMessage());
        }

        protected override void FreeLibrary()
        {
            if (!PlatformFreeLibrary(_nativeHandle))
                throw new Exception(GetLastErrorMessage());
        }

        private string GetLastErrorMessage() => new Win32Exception(Marshal.GetLastWin32Error()).Message.Replace("\0", "").Replace("\r\n", "");
    }
}
