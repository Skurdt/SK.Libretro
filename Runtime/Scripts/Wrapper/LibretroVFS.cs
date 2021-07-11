/* MIT License

 * Copyright (c) 2020 Skurdt
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
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroVFS
    {
        public const int SUPPORTED_VFS_VERSION = 3;
        public readonly IntPtr InterfacePtr;
        private readonly retro_vfs_interface _interface;

        public LibretroVFS()
        {
            _interface = new retro_vfs_interface
            {
                // v1
                get_path        = Marshal.GetFunctionPointerForDelegate<retro_vfs_get_path_t>(GetPath),
                open            = Marshal.GetFunctionPointerForDelegate<retro_vfs_open_t>(Open),
                close           = Marshal.GetFunctionPointerForDelegate<retro_vfs_close_t>(Close),
                size            = Marshal.GetFunctionPointerForDelegate<retro_vfs_size_t>(Size),
                tell            = Marshal.GetFunctionPointerForDelegate<retro_vfs_tell_t>(Tell),
                seek            = Marshal.GetFunctionPointerForDelegate<retro_vfs_seek_t>(Seek),
                read            = Marshal.GetFunctionPointerForDelegate<retro_vfs_read_t>(Read),
                write           = Marshal.GetFunctionPointerForDelegate<retro_vfs_write_t>(Write),
                flush           = Marshal.GetFunctionPointerForDelegate<retro_vfs_flush_t>(Flush),
                remove          = Marshal.GetFunctionPointerForDelegate<retro_vfs_remove_t>(Remove),
                rename          = Marshal.GetFunctionPointerForDelegate<retro_vfs_rename_t>(Rename),
                // v2
                truncate        = Marshal.GetFunctionPointerForDelegate<retro_vfs_truncate_t>(Truncate),
                // v3
                stat            = Marshal.GetFunctionPointerForDelegate<retro_vfs_stat_t>(Stat),
                mkdir           = Marshal.GetFunctionPointerForDelegate<retro_vfs_mkdir_t>(MkDir),
                opendir         = Marshal.GetFunctionPointerForDelegate<retro_vfs_opendir_t>(OpenDir),
                readdir         = Marshal.GetFunctionPointerForDelegate<retro_vfs_readdir_t>(ReadDir),
                dirent_get_name = Marshal.GetFunctionPointerForDelegate<retro_vfs_dirent_get_name_t>(DirentGeName),
                dirent_is_dir   = Marshal.GetFunctionPointerForDelegate<retro_vfs_dirent_is_dir_t>(DirentIsDir),
                closedir        = Marshal.GetFunctionPointerForDelegate<retro_vfs_closedir_t>(CloseDir)
            };

            InterfacePtr = Marshal.AllocHGlobal(Marshal.SizeOf<retro_vfs_interface>());
            Marshal.StructureToPtr(_interface, InterfacePtr, false);
        }

        public void DeInit() => Marshal.FreeHGlobal(InterfacePtr);

        private IntPtr GetPath(IntPtr stream) => IntPtr.Zero;
        private IntPtr Open(IntPtr path, uint mode, uint hints) => IntPtr.Zero;
        private int Close(IntPtr stream) => 0;
        private long Size(IntPtr stream) => 0;
        private long Tell(IntPtr stream) => 0;
        private long Seek(IntPtr stream, long offset, int seek_position) => 0;
        private long Read(IntPtr stream, IntPtr s, long len) => 0;
        private long Write(IntPtr stream, IntPtr s, long len) => 0;
        private int Flush(IntPtr stream) => 0;
        private int Remove(IntPtr path) => 0;
        private int Rename(IntPtr old_path, IntPtr new_path) => 0;
        private long Truncate(IntPtr stream, long length) => 0;
        private int Stat(IntPtr path, ref int size) => 0;
        private int MkDir(IntPtr dir) => 0;
        private IntPtr OpenDir(IntPtr dir, bool include_hidden) => IntPtr.Zero;
        private bool ReadDir(IntPtr dirstream) => false;
        private IntPtr DirentGeName(IntPtr dirstream) => IntPtr.Zero;
        private bool DirentIsDir(IntPtr dirstream) => false;
        private int CloseDir(IntPtr dirstream) => 0;
    }
}
