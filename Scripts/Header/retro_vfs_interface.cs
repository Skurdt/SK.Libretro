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
    // typedef const char *(RETRO_CALLCONV *retro_vfs_get_path_t)(struct retro_vfs_file_handle *stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    internal delegate string retro_vfs_get_path_t(ref retro_vfs_file_handle stream);
    // typedef struct retro_vfs_file_handle *(RETRO_CALLCONV *retro_vfs_open_t)(const char* path, unsigned mode, unsigned hints);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr retro_vfs_open_t([MarshalAs(UnmanagedType.LPStr)] string path, uint mode, uint hints);
    // typedef int (RETRO_CALLCONV *retro_vfs_close_t) (struct retro_vfs_file_handle *stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int retro_vfs_close_t(ref retro_vfs_file_handle stream);
    // typedef System.Int64 (RETRO_CALLCONV *retro_vfs_size_t)(struct retro_vfs_file_handle *stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long retro_vfs_size_t(ref retro_vfs_file_handle stream);
    // typedef System.Int64 (RETRO_CALLCONV *retro_vfs_truncate_t)(struct retro_vfs_file_handle *stream, System.Int64 length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long retro_vfs_truncate_t(ref retro_vfs_file_handle stream, long length);
    // typedef System.Int64 (RETRO_CALLCONV *retro_vfs_tell_t)(struct retro_vfs_file_handle *stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long retro_vfs_tell_t(ref retro_vfs_file_handle stream);
    // typedef System.Int64 (RETRO_CALLCONV *retro_vfs_seek_t)(struct retro_vfs_file_handle *stream, System.Int64 offset, int seek_position);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long retro_vfs_seek_t(ref retro_vfs_file_handle stream, long offset, int seek_position);
    // typedef System.Int64 (RETRO_CALLCONV *retro_vfs_read_t)(struct retro_vfs_file_handle *stream, void* s, uSystem.Int64 len);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long retro_vfs_read_t(ref retro_vfs_file_handle stream, IntPtr s, long len);
    // typedef System.Int64 (RETRO_CALLCONV *retro_vfs_write_t)(struct retro_vfs_file_handle *stream, const void* s, uSystem.Int64 len);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate long retro_vfs_write_t(ref retro_vfs_file_handle stream, IntPtr s, long len);
    // typedef int (RETRO_CALLCONV *retro_vfs_flush_t)(struct retro_vfs_file_handle *stream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int retro_vfs_flush_t(ref retro_vfs_file_handle stream);
    // typedef int (RETRO_CALLCONV *retro_vfs_remove_t)(const char* path);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int retro_vfs_remove_t([MarshalAs(UnmanagedType.LPStr)] string path);
    // typedef int (RETRO_CALLCONV *retro_vfs_rename_t)(const char* old_path, const char* new_path);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int retro_vfs_rename_t([MarshalAs(UnmanagedType.LPStr)] string old_path, [MarshalAs(UnmanagedType.LPStr)] string new_path);
    // typedef int (RETRO_CALLCONV *retro_vfs_stat_t)(const char* path, int32_t *size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int retro_vfs_stat_t([MarshalAs(UnmanagedType.LPStr)] string path, ref int size);
    // typedef int (RETRO_CALLCONV *retro_vfs_mkdir_t)(const char* dir);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int retro_vfs_mkdir_t([MarshalAs(UnmanagedType.LPStr)] string dir);
    // typedef struct retro_vfs_dir_handle *(RETRO_CALLCONV *retro_vfs_opendir_t)(const char* dir, bool include_hidden);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr retro_vfs_opendir_t([MarshalAs(UnmanagedType.LPStr)] string dir, [MarshalAs(UnmanagedType.I1)] bool include_hidden);
    // typedef bool (RETRO_CALLCONV *retro_vfs_readdir_t)(struct retro_vfs_dir_handle *dirstream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_vfs_readdir_t(ref retro_vfs_dir_handle dirstream);
    // typedef const char*(RETRO_CALLCONV *retro_vfs_dirent_get_name_t)(struct retro_vfs_dir_handle *dirstream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    internal delegate string retro_vfs_dirent_get_name_t(ref retro_vfs_dir_handle dirstream);
    // typedef bool (RETRO_CALLCONV *retro_vfs_dirent_is_dir_t)(struct retro_vfs_dir_handle *dirstream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal delegate bool retro_vfs_dirent_is_dir_t(ref retro_vfs_dir_handle dirstream);
    // typedef int (RETRO_CALLCONV *retro_vfs_closedir_t)(struct retro_vfs_dir_handle *dirstream);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int retro_vfs_closedir_t(ref retro_vfs_dir_handle dirstream);

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class retro_vfs_interface
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
        //public IntPtr stat;            // retro_vfs_stat_t
        //public IntPtr mkdir;           // retro_vfs_mkdir_t
        //public IntPtr opendir;         // retro_vfs_opendir_t
        //public IntPtr readdir;         // retro_vfs_readdir_t
        //public IntPtr dirent_get_name; // retro_vfs_dirent_get_name_t
        //public IntPtr dirent_is_dir;   // retro_vfs_dirent_is_dir_t
        //public IntPtr closedir;        // retro_vfs_closedir_t
    }
}
