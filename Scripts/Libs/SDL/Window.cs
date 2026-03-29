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
    internal class Window : IDisposable
    {
        protected virtual ulong Flags { get;  } = 0;

        private const uint SDL_INIT_VIDEO = 0x00000020u;
        private const ulong SDL_WINDOW_HIDDEN = 0x0000000000000008ul;

        private readonly ulong _flags = 0;
        
        private IntPtr _handle = IntPtr.Zero;

        public Window(string title, int width, int height, bool hidden)
        {
            if (_handle != IntPtr.Zero)
                Dispose();
            
            if (!SDL_InitSubSystem(SDL_INIT_VIDEO))
                throw new Exception($"SDL_InitSubSystem: {SDL_GetError()}");

            if (hidden)
                _flags |= SDL_WINDOW_HIDDEN;

            _flags |= Flags;

            var window = SDL_CreateWindow(title, width, height, _flags);
            if (window == IntPtr.Zero)
                throw new Exception($"SDL_CreateWindow: {SDL_GetError()}");

            _handle = window;
        }

        public void Resize(int width, int height)
        {
            if (_handle == IntPtr.Zero)
                return;

            _ = SDL_SetWindowSize(_handle, width, height);
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                SDL_DestroyWindow(_handle);
                _handle = IntPtr.Zero;
            }

            SDL_QuitSubSystem(SDL_INIT_VIDEO);
        }

        public static implicit operator IntPtr(Window window) => window._handle;

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr SDL_CreateWindow([MarshalAs(UnmanagedType.LPStr)] string title, int w, int h, ulong flags);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SDL_DestroyWindow(IntPtr window);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SDL_SetWindowSize(IntPtr window, int w, int h);
    }
}
