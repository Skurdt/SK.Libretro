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
using static SK.Libretro.SDL.GLContext;

namespace SK.Libretro.GL
{
    internal static class Header
    {
        public const int GL_BGRA                                       = 0x80E1;
        public const uint GL_UNSIGNED_BYTE                             = 0x1401;
        public const uint GL_TEXTURE_2D                                = 0x0DE1;
        public const uint GL_TEXTURE_MIN_FILTER                        = 0x2801;
        public const uint GL_TEXTURE_MAG_FILTER                        = 0x2800;
        public const int GL_NEAREST                                    = 0x2600;
        public const uint GL_FRAMEBUFFER                               = 0x8D40;
        public const uint GL_COLOR_ATTACHMENT0                         = 0x8CE0;
        public const uint GL_RENDERBUFFER                              = 0x8D41;
        public const uint GL_DEPTH_COMPONENT24                         = 0x81A6;
        public const uint GL_DEPTH_ATTACHMENT                          = 0x8D00;
        public const uint GL_DEPTH24_STENCIL8                          = 0x88F0;
        public const uint GL_STENCIL_ATTACHMENT                        = 0x8D20;
        public const uint GL_FRAMEBUFFER_COMPLETE                      = 0x8CD5;
        public const uint GL_PIXEL_PACK_BUFFER                         = 0x88EB;
        public const uint GL_MAP_READ_BIT                              = 0x0001;
        public const uint GL_MAP_PERSISTENT_BIT                        = 0x0040;
        public const uint GL_DYNAMIC_STORAGE_BIT                       = 0x0100;
        public const uint GL_SYNC_GPU_COMMANDS_COMPLETE                = 0x9117;
        public const ulong GL_TIMEOUT_IGNORED                          = 0xFFFFFFFFFFFFFFFF;
        public const uint GL_READ_FRAMEBUFFER                          = 0x8CA8;
        public const uint GL_DRAW_FRAMEBUFFER                          = 0x8CA9;
        public const uint GL_COLOR_BUFFER_BIT                          = 0x00004000;
        public const uint GL_ALREADY_SIGNALED                          = 0x911A;
        public const uint GL_CONDITION_SATISFIED                       = 0x911C;
        public const uint GL_FRAMEBUFFER_UNDEFINED                     = 0x8219;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT         = 0x8CD6;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8CD7;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER        = 0x8CDB;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER        = 0x8CDC;
        public const uint GL_FRAMEBUFFER_UNSUPPORTED                   = 0x8CDD;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE        = 0x8D56;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS      = 0x8DA8;

        [DllImport("Opengl32", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void glBindTexture(uint target, uint texture);

        [DllImport("Opengl32", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void glTexImage2D(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);

        [DllImport("Opengl32", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void glTexParameteri(uint target, uint pname, int param);

        [DllImport("Opengl32", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void glFinish();

        [DllImport("Opengl32", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void glFlush();

        [DllImport("Opengl32", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glGenTexturesDelegate(int n, out uint textures);
        public static glGenTexturesDelegate glGenTextures;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glDeleteTexturesDelegate(int n, ref uint textures);
        public static glDeleteTexturesDelegate glDeleteTextures;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glGenFramebuffersDelegate(int n, out uint framebuffers);
        public static glGenFramebuffersDelegate glGenFramebuffers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glBindFramebufferDelegate(uint target, uint framebuffer);
        public static glBindFramebufferDelegate glBindFramebuffer;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glFramebufferTexture2DDelegate(uint target, uint attachment, uint textarget, uint texture, int level);
        public static glFramebufferTexture2DDelegate glFramebufferTexture2D;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glDrawBuffersDelegate(int n, ref uint bufs);
        public static glDrawBuffersDelegate glDrawBuffers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate uint glCheckFramebufferStatusDelegate(uint target);
        public static glCheckFramebufferStatusDelegate glCheckFramebufferStatus;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glGenRenderbuffersDelegate(int n, out uint renderbuffers);
        public static glGenRenderbuffersDelegate glGenRenderbuffers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glBindRenderbufferDelegate(uint target, uint renderbuffer);
        public static glBindRenderbufferDelegate glBindRenderbuffer;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glRenderbufferStorageDelegate(uint target, uint internalformat, int width, int height);
        public static glRenderbufferStorageDelegate glRenderbufferStorage;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glFramebufferRenderbufferDelegate(uint target, uint attachment, uint renderbuffertarget, uint renderbuffer);
        public static glFramebufferRenderbufferDelegate glFramebufferRenderbuffer;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glDeleteFramebuffersDelegate(int n, ref uint framebuffers);
        public static glDeleteFramebuffersDelegate glDeleteFramebuffers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glDeleteRenderbuffersDelegate(int n, ref uint renderbuffers);
        public static glDeleteRenderbuffersDelegate glDeleteRenderbuffers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr glFenceSyncDelegate(uint condition, uint flags);
        public static glFenceSyncDelegate glFenceSync;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glDeleteSyncDelegate(IntPtr sync);
        public static glDeleteSyncDelegate glDeleteSync;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glWaitSyncDelegate(IntPtr sync, uint flags, ulong timeout);
        public static glWaitSyncDelegate glWaitSync;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate uint glClientWaitSyncDelegate(IntPtr sync, uint flags, ulong timeout);
        public static glClientWaitSyncDelegate glClientWaitSync;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glBindBufferDelegate(uint target, uint buffer);
        public static glBindBufferDelegate glBindBuffer;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe delegate void glGenBuffersDelegate(int n, uint* buffers);
        public static glGenBuffersDelegate glGenBuffers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe delegate void glDeleteBuffersDelegate(int n, uint* buffers);
        public static glDeleteBuffersDelegate glDeleteBuffers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glBufferStorageDelegate(uint target, long size, IntPtr data, uint flags);
        public static glBufferStorageDelegate glBufferStorage;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr glMapBufferRangeDelegate(uint target, long offset, long length, uint access);
        public static glMapBufferRangeDelegate glMapBufferRange;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glUnmapBufferDelegate(uint target);
        public static glUnmapBufferDelegate glUnmapBuffer;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void glBlitFramebufferDelegate(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, uint mask, uint filter);
        public static glBlitFramebufferDelegate glBlitFramebuffer;

        public static bool GLLoadSymbols()
        {
            try
            {
                glGenTextures = GLLoadSymbol<glGenTexturesDelegate>();
                glDeleteTextures = GLLoadSymbol<glDeleteTexturesDelegate>();
                glGenFramebuffers = GLLoadSymbol<glGenFramebuffersDelegate>();
                glBindFramebuffer = GLLoadSymbol<glBindFramebufferDelegate>();
                glFramebufferTexture2D = GLLoadSymbol<glFramebufferTexture2DDelegate>();
                glDrawBuffers = GLLoadSymbol<glDrawBuffersDelegate>();
                glCheckFramebufferStatus = GLLoadSymbol<glCheckFramebufferStatusDelegate>();
                glGenRenderbuffers = GLLoadSymbol<glGenRenderbuffersDelegate>();
                glBindRenderbuffer = GLLoadSymbol<glBindRenderbufferDelegate>();
                glRenderbufferStorage = GLLoadSymbol<glRenderbufferStorageDelegate>();
                glFramebufferRenderbuffer = GLLoadSymbol<glFramebufferRenderbufferDelegate>();
                glDeleteFramebuffers = GLLoadSymbol<glDeleteFramebuffersDelegate>();
                glDeleteRenderbuffers = GLLoadSymbol<glDeleteRenderbuffersDelegate>();
                glFenceSync = GLLoadSymbol<glFenceSyncDelegate>();
                glDeleteSync = GLLoadSymbol<glDeleteSyncDelegate>();
                glWaitSync = GLLoadSymbol<glWaitSyncDelegate>();
                glClientWaitSync = GLLoadSymbol<glClientWaitSyncDelegate>();
                glBindBuffer = GLLoadSymbol<glBindBufferDelegate>();
                glGenBuffers = GLLoadSymbol<glGenBuffersDelegate>();
                glDeleteBuffers = GLLoadSymbol<glDeleteBuffersDelegate>();
                glBufferStorage = GLLoadSymbol<glBufferStorageDelegate>();
                glMapBufferRange = GLLoadSymbol<glMapBufferRangeDelegate>();
                glUnmapBuffer = GLLoadSymbol<glUnmapBufferDelegate>();
                glBlitFramebuffer = GLLoadSymbol<glBlitFramebufferDelegate>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static T GLLoadSymbol<T>() where T : Delegate
        {
            var functionName = typeof(T).Name.Replace("Delegate", "");
            var procAddr = SDL_GL_GetProcAddress(functionName);
            return procAddr != IntPtr.Zero
                 ? procAddr.GetDelegate<T>()
                 : throw new Exception($"Failed to load GL function for {functionName}");
        }
    }
}
