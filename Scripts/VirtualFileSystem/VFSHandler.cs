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

using SK.Libretro.Header;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class VFSHandler : IDisposable
    {
        private const int SUPPORTED_VERSION = 2;

        private readonly Dictionary<IntPtr, FileStream> _files = new();

        private retro_vfs_get_path_t _getPath;
        private retro_vfs_open_t _open;
        private retro_vfs_close_t _close;
        private retro_vfs_size_t _size;
        private retro_vfs_tell_t _tell;
        private retro_vfs_seek_t _seek;
        private retro_vfs_read_t _read;
        private retro_vfs_write_t _write;
        private retro_vfs_flush_t _flush;
        private retro_vfs_remove_t _remove;
        private retro_vfs_rename_t _rename;
        private retro_vfs_truncate_t _truncate;
        //private retro_vfs_stat_t _stat;
        //private retro_vfs_mkdir_t _mkDir;
        //private retro_vfs_opendir_t _openDir;
        //private retro_vfs_readdir_t _readDir;
        //private retro_vfs_dirent_get_name_t _direntGetName;
        //private retro_vfs_dirent_is_dir_t _direntIsDir;
        //private retro_vfs_closedir_t _closeDir;

        private retro_vfs_interface _interface;
        private IntPtr _interfacePtr;

        public void Dispose()
        {
            foreach (FileStream stream in _files.Values)
            {
                try
                {
                    stream.Dispose();
                }
                catch
                {
                }
            }

            _files.Clear();
            PointerUtilities.Free(ref _interfacePtr);
        }

        public bool GetVfsInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_vfs_interface_info interfaceInfo = data.ToStructure<retro_vfs_interface_info>();
            if (interfaceInfo.required_interface_version > SUPPORTED_VERSION)
                return false;

            _getPath       = GetPath;
            _open          = Open;
            _close         = Close;
            _size          = Size;
            _tell          = Tell;
            _seek          = Seek;
            _read          = Read;
            _write         = Write;
            _flush         = Flush;
            _remove        = Remove;
            _rename        = Rename;
            _truncate      = Truncate;
            //_stat          = Stat;
            //_mkDir         = MkDir;
            //_openDir       = OpenDir;
            //_readDir       = ReadDir;
            //_direntGetName = DirentGetName;
            //_direntIsDir   = DirentIsDir;
            //_closeDir      = CloseDir;

            _interface = new()
            {
                // v1
                get_path        = _getPath.GetFunctionPointer(),
                open            = _open.GetFunctionPointer(),
                close           = _close.GetFunctionPointer(),
                size            = _size.GetFunctionPointer(),
                tell            = _tell.GetFunctionPointer(),
                seek            = _seek.GetFunctionPointer(),
                read            = _read.GetFunctionPointer(),
                write           = _write.GetFunctionPointer(),
                flush           = _flush.GetFunctionPointer(),
                remove          = _remove.GetFunctionPointer(),
                rename          = _rename.GetFunctionPointer(),
                // v2
                truncate        = _truncate.GetFunctionPointer(),
                //// v3
                //stat            = _stat.GetFunctionPointer(),
                //mkdir           = _mkDir.GetFunctionPointer(),
                //opendir         = _openDir.GetFunctionPointer(),
                //readdir         = _readDir.GetFunctionPointer(),
                //dirent_get_name = _direntGetName.GetFunctionPointer(),
                //dirent_is_dir   = _direntIsDir.GetFunctionPointer(),
                //closedir        = _closeDir.GetFunctionPointer()
            };

            _interfacePtr = Marshal.AllocHGlobal(Marshal.SizeOf<retro_vfs_interface>());
            Marshal.StructureToPtr(_interface, _interfacePtr, true);
            interfaceInfo.iface = _interfacePtr;

            Marshal.StructureToPtr(interfaceInfo, data, true);
            return true;
        }

        private string GetPath(ref retro_vfs_file_handle stream) =>
            _files.TryGetValue(stream.handle, out FileStream fileStream) ? fileStream.Name : "";

        private IntPtr Open(string path, uint mode, uint hints)
        {
            try
            {
                FileMode fileMode = (mode & (uint)RETRO_VFS_FILE_ACCESS.UPDATE_EXISTING) == (uint)RETRO_VFS_FILE_ACCESS.UPDATE_EXISTING
                                  ? FileMode.Append
                                  : (mode & (uint)RETRO_VFS_FILE_ACCESS.READ_WRITE) == (uint)RETRO_VFS_FILE_ACCESS.READ_WRITE ? FileMode.Create : FileMode.Open;

                FileStream stream = File.Open(path, fileMode);
                IntPtr handle = stream.SafeFileHandle.DangerousGetHandle();
                _files.Add(handle, stream);
                return handle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private int Close(ref retro_vfs_file_handle stream)
        {
            try
            {
                if (!_files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                _ = _files.Remove(stream.handle);
                fileStream.Close();
                fileStream.Dispose();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        private long Size(ref retro_vfs_file_handle stream) =>
            _files.TryGetValue(stream.handle, out FileStream fileStream) ? fileStream.Length : -1;

        private long Tell(ref retro_vfs_file_handle stream) =>
            _files.TryGetValue(stream.handle, out FileStream fileStream) ? fileStream.Position : -1;

        private long Seek(ref retro_vfs_file_handle stream, long offset, int seek_position)
        {
            try
            {
                if (seek_position is < (int)RETRO_VFS_SEEK_POSITION.START or > (int)RETRO_VFS_SEEK_POSITION.END)
                    return -1;

                if (!_files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                SeekOrigin seekOrigin = seek_position switch
                {
                    (int)RETRO_VFS_SEEK_POSITION.START   => SeekOrigin.Begin,
                    (int)RETRO_VFS_SEEK_POSITION.CURRENT => SeekOrigin.Current,
                    (int)RETRO_VFS_SEEK_POSITION.END     => SeekOrigin.End,
                    _ => throw new ArgumentOutOfRangeException($"seek_position: {seek_position}")
                };

                return fileStream.Seek(offset, seekOrigin);
            }
            catch
            {
                return -1;
            }
        }

        private long Read(ref retro_vfs_file_handle stream, IntPtr s, long len)
        {
            try
            {
                if (len > int.MaxValue)
                    return -1;

                if (!_files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                byte[] data = new byte[len];
                int result = fileStream.Read(data, 0, (int)len);
                Marshal.Copy(data, 0, s, data.Length);
                return result;
            }
            catch
            {
                return -1;
            }
        }

        private long Write(ref retro_vfs_file_handle stream, IntPtr s, long len)
        {
            try
            {
                if (len > int.MaxValue)
                    return -1;

                if (!_files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                byte[] data = new byte[len];
                Marshal.Copy(s, data, 0, (int)len);
                fileStream.Write(data, 0, (int)len);
                return len;
            }
            catch
            {
                return -1;
            }
        }

        private int Flush(ref retro_vfs_file_handle stream)
        {
            try
            {
                if (!_files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                fileStream.Flush();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        private int Remove(string path) =>
            FileSystem.DeleteFile(path) ? 0 : -1;

        private int Rename(string old_path, string new_path) =>
            FileSystem.MoveFile(old_path, new_path) ? 0 : -1;

        private long Truncate(ref retro_vfs_file_handle stream, long length)
        {
            try
            {
                if (!_files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                fileStream.SetLength(length);
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        //private int Stat(string path, ref int size) => 0;
        //private int MkDir(string dir) => 0;
        //private IntPtr OpenDir(string dir, bool include_hidden) => IntPtr.Zero;
        //private bool ReadDir(ref retro_vfs_dir_handle dirstream) => false;
        //private string DirentGetName(ref retro_vfs_dir_handle dirstream) => "";
        //private bool DirentIsDir(ref retro_vfs_dir_handle dirstream) => false;
        //private int CloseDir(ref retro_vfs_dir_handle dirstream) => 0;
    }
}
