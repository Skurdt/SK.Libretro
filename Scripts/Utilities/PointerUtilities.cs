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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal static class PointerUtilities
    {
        public static bool IsNull(this IntPtr ptr) => ptr == IntPtr.Zero;

        public static bool IsNotNull(this IntPtr ptr) => ptr != IntPtr.Zero;

        public static bool IsTrue(this IntPtr ptr) => ptr.ReadByte() > 0;

        public static bool IsFalse(this IntPtr ptr) => !ptr.IsTrue();

        public static byte ReadByte(this IntPtr ptr) => ptr.IsNotNull() ? Marshal.ReadByte(ptr) : (byte)0;

        public static ushort ReadUInt16(this IntPtr ptr) => Convert.ToUInt16(ptr.IsNotNull() ? Marshal.ReadInt16(ptr) : 0);

        public static ushort ReadUInt16(this IntPtr ptr, int offset) => Convert.ToUInt16(ptr.IsNotNull() ? Marshal.ReadInt16(ptr, offset) : 0);

        public static int ReadInt32(this IntPtr ptr) => ptr.IsNotNull() ? Marshal.ReadInt32(ptr) : 0;

        public static int ReadInt32(this IntPtr ptr, int offset) => ptr.IsNotNull() ? Marshal.ReadInt32(ptr, offset) : 0;

        public static uint ReadUInt32(this IntPtr ptr) => ptr.IsNotNull() ? Convert.ToUInt32(Marshal.ReadInt32(ptr)) : 0;

        public static uint ReadUInt32(this IntPtr ptr, int offset) => ptr.IsNotNull() ? Convert.ToUInt32(Marshal.ReadInt32(ptr, offset)) : 0;

        public static ulong ReadUInt64(this IntPtr ptr) => ptr.IsNotNull() ? Convert.ToUInt64(Marshal.ReadInt64(ptr)) : 0;

        public static void Write(this IntPtr ptr, bool value)
        {
            if (ptr.IsNotNull())
                Marshal.WriteByte(ptr, Convert.ToByte(value));
        }

        public static void Write(this IntPtr ptr, int value)
        {
            if (ptr.IsNotNull())
                Marshal.WriteInt32(ptr, value);
        }

        public static void Write(this IntPtr ptr, uint value)
        {
            if (ptr.IsNotNull())
                Marshal.WriteInt32(ptr, Convert.ToInt32(value));
        }

        public static void Write(this IntPtr ptr, ulong value)
        {
            if (ptr.IsNotNull())
                Marshal.WriteInt64(ptr, Convert.ToInt64(value));
        }

        public static void Write(this IntPtr ptr, IntPtr value)
        {
            if (ptr.IsNotNull())
                Marshal.WriteIntPtr(ptr, value);
        }

        public static T ToStructure<T>(this IntPtr ptr) => Marshal.PtrToStructure<T>(ptr);

        public static T GetDelegate<T>(this IntPtr ptr) where T : Delegate => ptr.IsNotNull() ? Marshal.GetDelegateForFunctionPointer<T>(ptr) : default;

        public static IntPtr GetFunctionPointer<T>(this T @delegate) where T : Delegate => Marshal.GetFunctionPointerForDelegate(@delegate);

        public static void ToStructure<T>(this IntPtr ptr, T @class) where T : class => Marshal.PtrToStructure(ptr, @class);

        public static string AsString(this IntPtr ptr) => ptr.IsNotNull() ? Marshal.PtrToStringAnsi(ptr) : "";

        public static IntPtr AsAllocatedPtr(this string str) => str is null ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(str);

        public static void Free(ref IntPtr ptr)
        {
            if (ptr.IsNotNull())
                Marshal.FreeHGlobal(ptr);
            SetToNull(ref ptr);
        }

        public static IntPtr Alloc(int size) => Marshal.AllocHGlobal(size);

        public static IntPtr Alloc<T>() => Marshal.AllocHGlobal(Marshal.SizeOf<T>());

        public static void Free(IList<IntPtr> ptrs)
        {
            for (int i = 0; i < ptrs.Count; i++)
            {
                IntPtr ptr = ptrs[i];
                Free(ref ptr);
            }

            ptrs.Clear();
        }

        public static void SetToNull(ref IntPtr ptr) => ptr = IntPtr.Zero;
    }
}
