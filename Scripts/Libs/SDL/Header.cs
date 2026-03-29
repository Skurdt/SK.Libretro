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

namespace SK.Libretro.SDL
{
    internal static class Header
    {
        public const string LIB_NAME = "SDL3";

        public const uint SDL_INIT_AUDIO = 0x00000010u;

        public const uint SDL_AUDIO_DEVICE_DEFAULT_OUTPUT = 0xFFFFFFFFu;

        public enum SDL_AudioFormat : ushort
        {
            U8    = 0x0008,
            S8    = 0x8008,
            S16LE = 0x8010,
            S16BE = 0x9010,
            S32LE = 0x8020,
            S32BE = 0x9020,
            F32LE = 0x8120,
            F32BE = 0x9120,
            S16   = S16LE,
            S32   = S32LE,
            F32   = F32LE
        }

        public delegate void SDL_AudioStreamCallback(IntPtr userdata, IntPtr stream, int additional_amount, int total_amount);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct SDL_AudioSpec
        {
            public SDL_AudioFormat format;
            public int channels;
            public int freq;
        }

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string SDL_GetError();

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_InitSubSystem(uint flags);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SDL_QuitSubSystem(uint flags);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr SDL_Vulkan_GetVkGetInstanceProcAddr();

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr SDL_Vulkan_GetInstanceExtensions(out uint count);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_Vulkan_CreateSurface(IntPtr window, IntPtr instance, IntPtr allocator, out IntPtr surface);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern void SDL_Vulkan_DestroySurface(IntPtr instance, IntPtr surface, IntPtr allocator);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_GetAudioDeviceFormat(uint devid, out SDL_AudioSpec spec, out int sample_frames);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern uint SDL_OpenAudioDevice(uint devid, ref SDL_AudioSpec spec);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SDL_CloseAudioDevice(uint devid);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr SDL_CreateAudioStream(ref SDL_AudioSpec src_spec, ref SDL_AudioSpec dst_spec);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SDL_DestroyAudioStream(IntPtr stream);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_BindAudioStream(uint devid, IntPtr stream);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SDL_UnbindAudioStream(IntPtr stream);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_LockAudioStream(IntPtr stream);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_UnlockAudioStream(IntPtr stream);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr SDL_OpenAudioDeviceStream(uint devid, ref SDL_AudioSpec spec, IntPtr callback, IntPtr userdata);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_ResumeAudioStreamDevice(IntPtr stream);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_ResumeAudioDevice(uint devid);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_PutAudioStreamData(IntPtr stream, IntPtr buf, int len);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_FlushAudioStream(IntPtr stream);
    }
}
