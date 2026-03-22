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

namespace SK.Libretro
{
    internal static class SDL
    {
        public const uint INIT_VIDEO = 0x00000020u;
        public const uint INIT_AUDIO = 0x00000010u;

        public const ulong WINDOW_OPENGL = 0x0000000000000002ul;
        public const ulong WINDOW_HIDDEN = 0x0000000000000008ul;

        public enum GLAttr
        {
            RED_SIZE,
            GREEN_SIZE,
            BLUE_SIZE,
            ALPHA_SIZE,
            BUFFER_SIZE,
            DOUBLEBUFFER,
            DEPTH_SIZE,
            STENCIL_SIZE,
            ACCUM_RED_SIZE,
            ACCUM_GREEN_SIZE,
            ACCUM_BLUE_SIZE,
            ACCUM_ALPHA_SIZE,
            STEREO,
            MULTISAMPLEBUFFERS,
            MULTISAMPLESAMPLES,
            ACCELERATED_VISUAL,
            RETAINED_BACKING,
            CONTEXT_MAJOR_VERSION,
            CONTEXT_MINOR_VERSION,
            CONTEXT_FLAGS,
            CONTEXT_PROFILE_MASK,
            SHARE_WITH_CURRENT_CONTEXT,
            FRAMEBUFFER_SRGB_CAPABLE,
            CONTEXT_RELEASE_BEHAVIOR,
            CONTEXT_RESET_NOTIFICATION,
            CONTEXT_NO_ERROR,
            FLOATBUFFERS,
            EGL_PLATFORM
        }

        public const int GL_CONTEXT_PROFILE_CORE = 0x0001;

        public const uint AUDIO_DEVICE_DEFAULT_OUTPUT = 0xFFFFFFFFu;

        public enum AudioFormat : ushort
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

        [StructLayout(LayoutKind.Sequential)]
        public struct AudioSpec
        {
            public AudioFormat format;
            public int channels;
            public int freq;
        }

        private const string LIB_NAME = "SDL3";

        [DllImport(LIB_NAME, EntryPoint = "SDL_GetError", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern string GetError();

        [DllImport(LIB_NAME, EntryPoint = "SDL_InitSubSystem", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool InitSubSystem(uint flags);

        [DllImport(LIB_NAME, EntryPoint = "SDL_QuitSubSystem", CallingConvention = CallingConvention.Cdecl)]
        public static extern void QuitSubSystem(uint flags);

        [DllImport(LIB_NAME, EntryPoint = "SDL_CreateWindow", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr CreateWindow(string title, int w, int h, ulong flags);

        [DllImport(LIB_NAME, EntryPoint = "SDL_DestroyWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyWindow(IntPtr window);

        [DllImport(LIB_NAME, EntryPoint = "SDL_GL_SetAttribute", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GL_SetAttribute(GLAttr attr, int value);

        [DllImport(LIB_NAME, EntryPoint = "SDL_GL_CreateContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_CreateContext(IntPtr window);

        [DllImport(LIB_NAME, EntryPoint = "SDL_GL_DestroyContext", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GL_DestroyContext(IntPtr context);

        [DllImport(LIB_NAME, EntryPoint = "SDL_GL_MakeCurrent", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GL_MakeCurrent(IntPtr window, IntPtr context);

        [DllImport(LIB_NAME, EntryPoint = "SDL_GL_GetProcAddress", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GL_GetProcAddress(string proc);

        [DllImport(LIB_NAME, EntryPoint = "SDL_GetAudioDeviceFormat", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetAudioDeviceFormat(uint devid, out AudioSpec spec, out int sample_frames);

        [DllImport(LIB_NAME, EntryPoint = "SDL_OpenAudioDevice", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint OpenAudioDevice(uint devid, ref AudioSpec spec);

        [DllImport(LIB_NAME, EntryPoint = "SDL_CloseAudioDevice", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseAudioDevice(uint devid);

        [DllImport(LIB_NAME, EntryPoint = "SDL_CreateAudioStream", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateAudioStream(ref AudioSpec src_spec, ref AudioSpec dst_spec);

        [DllImport(LIB_NAME, EntryPoint = "SDL_DestroyAudioStream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyAudioStream(IntPtr stream);

        [DllImport(LIB_NAME, EntryPoint = "SDL_BindAudioStream", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindAudioStream(uint devid, IntPtr stream);

        [DllImport(LIB_NAME, EntryPoint = "SDL_UnbindAudioStream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnbindAudioStream(IntPtr stream);

        [DllImport(LIB_NAME, EntryPoint = "SDL_ResumeAudioDevice", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ResumeAudioDevice(uint devid);

        [DllImport(LIB_NAME, EntryPoint = "SDL_PutAudioStreamData", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool PutAudioStreamData(IntPtr stream, IntPtr buf, int len);
    }
}
