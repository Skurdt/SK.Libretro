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
    internal static unsafe class GL
    {
        public const int BGRA                        = 0x80E1;
        public const uint UNSIGNED_BYTE              = 0x1401;
        public const uint TEXTURE_2D                 = 0x0DE1;
        public const int RGBA8                       = 0x8058;
        public const uint RGBA                       = 0x1908;
        public const uint TEXTURE_MIN_FILTER         = 0x2801;
        public const uint TEXTURE_MAG_FILTER         = 0x2800;
        public const int NEAREST                     = 0x2600;
        public const uint FRAMEBUFFER                = 0x8D40;
        public const uint COLOR_ATTACHMENT0          = 0x8CE0;
        public const uint RENDERBUFFER               = 0x8D41;
        public const uint DEPTH_COMPONENT24          = 0x81A6;
        public const uint DEPTH_ATTACHMENT           = 0x8D00;
        public const uint DEPTH24_STENCIL8           = 0x88F0;
        public const uint STENCIL_ATTACHMENT         = 0x8D20;
        public const uint FRAMEBUFFER_COMPLETE       = 0x8CD5;
        public const uint PIXEL_PACK_BUFFER          = 0x88EB;
        public const uint MAP_READ_BIT               = 0x0001;
        public const uint MAP_PERSISTENT_BIT         = 0x0040;
        public const uint DYNAMIC_STORAGE_BIT        = 0x0100;
        public const uint SYNC_GPU_COMMANDS_COMPLETE = 0x9117;
        public const ulong TIMEOUT_IGNORED           = 0xFFFFFFFFFFFFFFFF;
        public const uint READ_FRAMEBUFFER           = 0x8CA8;
        public const uint DRAW_FRAMEBUFFER           = 0x8CA9;
        public const uint COLOR_BUFFER_BIT           = 0x00004000;
        public const uint ALREADY_SIGNALED           = 0x911A;
        public const uint CONDITION_SATISFIED        = 0x911C;
        public const uint FRAMEBUFFER_UNDEFINED       = 0x8219;
        public const uint FRAMEBUFFER_INCOMPLETE_ATTACHMENT = 0x8CD6;
        public const uint FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8CD7;
        public const uint FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER = 0x8CDB;
        public const uint FRAMEBUFFER_INCOMPLETE_READ_BUFFER = 0x8CDC;
        public const uint FRAMEBUFFER_UNSUPPORTED = 0x8CDD;
        public const uint FRAMEBUFFER_INCOMPLETE_MULTISAMPLE = 0x8D56;
        public const uint FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS = 0x8DA8;

        [DllImport("Opengl32", EntryPoint = "glBindTexture")]
        public static extern void BindTexture(uint target, uint texture);

        [DllImport("Opengl32", EntryPoint = "glTexImage2D")]
        public static extern void TexImage2D(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);

        [DllImport("Opengl32", EntryPoint = "glTexParameteri")]
        public static extern void TexParameteri(uint target, uint pname, int param);

        [DllImport("Opengl32", EntryPoint = "glFinish")]
        public static extern void Finish();

        [DllImport("Opengl32", EntryPoint = "glFlush")]
        public static extern void Flush();

        [DllImport("Opengl32", EntryPoint = "glGetString")]
        public static extern IntPtr GetString(uint name);

        [DllImport("Opengl32", EntryPoint = "glReadPixels")]
        public static extern void ReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr data);

        public static GenTexturesDelegate GenTextures;
        public static DeleteTexturesDelegate DeleteTextures;
        public static GenFramebuffersDelegate GenFramebuffers;
        public static BindFramebufferDelegate BindFramebuffer;
        public static FramebufferTexture2DDelegate FramebufferTexture2D;
        public static DrawBuffersDelegate DrawBuffers;
        public static CheckFramebufferStatusDelegate CheckFramebufferStatus;
        public static GenRenderbuffersDelegate GenRenderbuffers;
        public static BindRenderbufferDelegate BindRenderbuffer;
        public static RenderbufferStorageDelegate RenderbufferStorage;
        public static FramebufferRenderbufferDelegate FramebufferRenderbuffer;
        public static DeleteFramebuffersDelegate DeleteFramebuffers;
        public static DeleteRenderbuffersDelegate DeleteRenderbuffers;
        public static FenceSyncDelegate FenceSync;
        public static DeleteSyncDelegate DeleteSync;
        public static WaitSyncDelegate WaitSync;
        public static ClientWaitSyncDelegate ClientWaitSync;
        public static BindBufferDelegate BindBuffer;
        public static GenBuffersDelegate GenBuffers;
        public static DeleteBuffersDelegate DeleteBuffers;
        public static BufferStorageDelegate BufferStorage;
        public static MapBufferRangeDelegate MapBufferRange;
        public static UnmapBufferDelegate UnmapBuffer;
        public static BlitFramebufferDelegate BlitFramebuffer;

        public static void LoadFunctions()
        {
            LoadFunction(out GenTextures);
            LoadFunction(out DeleteTextures);
            LoadFunction(out GenFramebuffers);
            LoadFunction(out BindFramebuffer);
            LoadFunction(out FramebufferTexture2D);
            LoadFunction(out DrawBuffers);
            LoadFunction(out CheckFramebufferStatus);
            LoadFunction(out GenRenderbuffers);
            LoadFunction(out BindRenderbuffer);
            LoadFunction(out RenderbufferStorage);
            LoadFunction(out FramebufferRenderbuffer);
            LoadFunction(out DeleteFramebuffers);
            LoadFunction(out DeleteRenderbuffers);
            LoadFunction(out FenceSync);
            LoadFunction(out DeleteSync);
            LoadFunction(out WaitSync);
            LoadFunction(out ClientWaitSync);
            LoadFunction(out BindBuffer);
            LoadFunction(out GenBuffers);
            LoadFunction(out DeleteBuffers);
            LoadFunction(out BufferStorage);
            LoadFunction(out MapBufferRange);
            LoadFunction(out UnmapBuffer);
            LoadFunction(out BlitFramebuffer);
        }

        private static void LoadFunction<T>(out T function) where T : Delegate
        {
            var functionName = "gl" + typeof(T).Name.Replace("Delegate", "");
            var proc = SDL.GL_GetProcAddress(functionName);
            if (proc == IntPtr.Zero)
                throw new InvalidOperationException($"Failed to load OpenGL function: {functionName}");
            function = Marshal.GetDelegateForFunctionPointer<T>(proc);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void GenTexturesDelegate(int n, out uint textures);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void DeleteTexturesDelegate(int n, ref uint textures);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void GenFramebuffersDelegate(int n, out uint framebuffers);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void BindFramebufferDelegate(uint target, uint framebuffer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void FramebufferTexture2DDelegate(uint target, uint attachment, uint textarget, uint texture, int level);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void DrawBuffersDelegate(int n, ref uint bufs);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate uint CheckFramebufferStatusDelegate(uint target);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void GenRenderbuffersDelegate(int n, out uint renderbuffers);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void BindRenderbufferDelegate(uint target, uint renderbuffer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void RenderbufferStorageDelegate(uint target, uint internalformat, int width, int height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void FramebufferRenderbufferDelegate(uint target, uint attachment, uint renderbuffertarget, uint renderbuffer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void DeleteFramebuffersDelegate(int n, ref uint framebuffers);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void DeleteRenderbuffersDelegate(int n, ref uint renderbuffers);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr FenceSyncDelegate(uint condition, uint flags);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void DeleteSyncDelegate(IntPtr sync);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void WaitSyncDelegate(IntPtr sync, uint flags, ulong timeout);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate uint ClientWaitSyncDelegate(IntPtr sync, uint flags, ulong timeout);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void BindBufferDelegate(uint target, uint buffer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void GenBuffersDelegate(int n, uint* buffers);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void DeleteBuffersDelegate(int n, uint* buffers);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void BufferStorageDelegate(uint target, long size, IntPtr data, uint flags);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr MapBufferRangeDelegate(uint target, long offset, long length, uint access);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void UnmapBufferDelegate(uint target);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void BlitFramebufferDelegate(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, uint mask, uint filter);
    }
}
