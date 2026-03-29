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

namespace SK.Libretro.Header
{

    /*
    
#ifdef VFS_FRONTEND
    struct retro_vfs_dir_handle
#else
    struct libretro_vfs_implementation_dir
#endif
    {
        char* orig_path;
#if defined(_WIN32)
#if defined(LEGACY_WIN32)
        WIN32_FIND_DATA entry;
#else
        WIN32_FIND_DATAW entry;
#endif
        HANDLE directory;
        bool next;
        char path[PATH_MAX_LENGTH];
#elif defined(VITA)
        SceUID directory;
        SceIoDirent entry;
#elif defined(__PSL1GHT__) || defined(__PS3__)
        int error;
        int directory;
        sysFSDirent entry;
#else
        DIR *directory;
        const struct dirent *entry;
#endif
#if defined(ANDROID) && defined(HAVE_SAF)
        libretro_vfs_implementation_saf_dir *saf_directory;
#endif
    };
    
    */

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal unsafe struct retro_vfs_dir_handle
    {
        public IntPtr orig_path;
        public IntPtr entry;
        public IntPtr directory;
        [MarshalAs(UnmanagedType.I1)] public bool next;
        public fixed byte path[4096];
    }
}
