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
using static SK.Libretro.SDL.Header;

namespace SK.Libretro.SDL
{
    internal sealed class GLContext : IDisposable
    {
        private const int SDL_GL_CONTEXT_PROFILE_CORE = 0x0001;

        private Window _window = null;
        private IntPtr _handle = IntPtr.Zero;

        public GLContext(Window window, int majorVersion, int minorVersion, bool shared = false)
        {
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_RED_SIZE, 8))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_GREEN_SIZE, 8))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_BLUE_SIZE, 8))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_ALPHA_SIZE, 8))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_DEPTH_SIZE, 24))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_STENCIL_SIZE, 8))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_DOUBLEBUFFER, 1))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_CONTEXT_MAJOR_VERSION, majorVersion))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_CONTEXT_MINOR_VERSION, minorVersion))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE))
                throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");

            if (shared)
            {
                if (!SDL_GL_SetAttribute(SDL_GLAttr.SDL_SHARE_WITH_CURRENT_CONTEXT, 1))
                    throw new Exception($"SDL_GL_SetAttribute: {SDL_GetError()}");
            }

            var context = SDL_GL_CreateContext(window);
            if (context == IntPtr.Zero)
                throw new Exception($"GLContext.Create - SDL_GL_CreateContext: {SDL_GetError()}");

            _window = window;
            _handle = context;
        }

        public void Dispose()
        {
            if (_handle == IntPtr.Zero)
                return;

            if (!SDL_GL_DestroyContext(_handle))
                throw new Exception($"GLContext.Destroy - SDL_GL_DestroyContext: {SDL_GetError()}");

            _handle = IntPtr.Zero;
            _window = null;
        }

        public bool MakeCurrent() => _window is not null && _handle != IntPtr.Zero && SDL_GL_MakeCurrent(_window, _handle);

        private enum SDL_GLAttr
        {
            SDL_RED_SIZE,
            SDL_GREEN_SIZE,
            SDL_BLUE_SIZE,
            SDL_ALPHA_SIZE,
            SDL_BUFFER_SIZE,
            SDL_DOUBLEBUFFER,
            SDL_DEPTH_SIZE,
            SDL_STENCIL_SIZE,
            SDL_ACCUM_RED_SIZE,
            SDL_ACCUM_GREEN_SIZE,
            SDL_ACCUM_BLUE_SIZE,
            SDL_ACCUM_ALPHA_SIZE,
            SDL_STEREO,
            SDL_MULTISAMPLEBUFFERS,
            SDL_MULTISAMPLESAMPLES,
            SDL_ACCELERATED_VISUAL,
            SDL_RETAINED_BACKING,
            SDL_CONTEXT_MAJOR_VERSION,
            SDL_CONTEXT_MINOR_VERSION,
            SDL_CONTEXT_FLAGS,
            SDL_CONTEXT_PROFILE_MASK,
            SDL_SHARE_WITH_CURRENT_CONTEXT,
            SDL_FRAMEBUFFER_SRGB_CAPABLE,
            SDL_CONTEXT_RELEASE_BEHAVIOR,
            SDL_CONTEXT_RESET_NOTIFICATION,
            SDL_CONTEXT_NO_ERROR,
            SDL_FLOATBUFFERS,
            SDL_EGL_PLATFORM
        }

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SDL_GL_SetAttribute(SDL_GLAttr attr, int value);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr SDL_GL_CreateContext(IntPtr window);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SDL_GL_DestroyContext(IntPtr context);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SDL_GL_MakeCurrent(IntPtr window, IntPtr context);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr SDL_GL_GetProcAddress([MarshalAs(UnmanagedType.LPStr)] string proc);
    }
}
