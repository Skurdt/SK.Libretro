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
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate IntPtr retro_vfs_get_path_t(IntPtr stream);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate IntPtr retro_vfs_open_t(IntPtr path, uint mode, uint hints);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate int retro_vfs_close_t(IntPtr stream);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate long retro_vfs_size_t(IntPtr stream);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate long retro_vfs_truncate_t(IntPtr stream, long length);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate long retro_vfs_tell_t(IntPtr stream);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate long retro_vfs_seek_t(IntPtr stream, long offset, int seek_position);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate long retro_vfs_read_t(IntPtr stream, IntPtr s, long len);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate long retro_vfs_write_t(IntPtr stream, IntPtr s, long len);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate int retro_vfs_flush_t(IntPtr stream);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate int retro_vfs_remove_t(IntPtr path);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate int retro_vfs_rename_t(IntPtr old_path, IntPtr new_path);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate int retro_vfs_stat_t(IntPtr path, ref int size);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate int retro_vfs_mkdir_t(IntPtr dir);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate IntPtr retro_vfs_opendir_t(IntPtr dir, [MarshalAs(UnmanagedType.I1)] bool include_hidden);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_vfs_readdir_t(IntPtr dirstream);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate IntPtr retro_vfs_dirent_get_name_t(IntPtr dirstream);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_vfs_dirent_is_dir_t(IntPtr dirstream);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate int retro_vfs_closedir_t(IntPtr dirstream);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct retro_vfs_interface
    {
        public IntPtr get_path;        // retro_vfs_get_path_t
        public IntPtr open;            // retro_vfs_open_t
        public IntPtr close;           // retro_vfs_close_t
        public IntPtr size;            // retro_vfs_size_t
        public IntPtr tell;            // retro_vfs_tell_t
        public IntPtr seek;            // retro_vfs_seek_t
        public IntPtr read;            // retro_vfs_read_t
        public IntPtr write;           // retro_vfs_write_t
        public IntPtr flush;           // retro_vfs_flush_t
        public IntPtr remove;          // retro_vfs_remove_t
        public IntPtr rename;          // retro_vfs_rename_t
        public IntPtr truncate;        // retro_vfs_truncate_t
        public IntPtr stat;            // retro_vfs_stat_t
        public IntPtr mkdir;           // retro_vfs_mkdir_t
        public IntPtr opendir;         // retro_vfs_opendir_t
        public IntPtr readdir;         // retro_vfs_readdir_t
        public IntPtr dirent_get_name; // retro_vfs_dirent_get_name_t
        public IntPtr dirent_is_dir;   // retro_vfs_dirent_is_dir_t
        public IntPtr closedir;        // retro_vfs_closedir_t
    }
}
