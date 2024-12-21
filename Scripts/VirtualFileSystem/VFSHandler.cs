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

        private static readonly retro_vfs_get_path_t _getPath              = GetPath;
        private static readonly retro_vfs_open_t _open                     = Open;
        private static readonly retro_vfs_close_t _close                   = Close;
        private static readonly retro_vfs_size_t _size                     = Size;
        private static readonly retro_vfs_tell_t _tell                     = Tell;
        private static readonly retro_vfs_seek_t _seek                     = Seek;
        private static readonly retro_vfs_read_t _read                     = Read;
        private static readonly retro_vfs_write_t _write                   = Write;
        private static readonly retro_vfs_flush_t _flush                   = Flush;
        private static readonly retro_vfs_remove_t _remove                 = Remove;  
        private static readonly retro_vfs_rename_t _rename                 = Rename;  
        private static readonly retro_vfs_truncate_t _truncate             = Truncate;      
        //private static readonly retro_vfs_stat_t _stat                     = Stat;
        //private static readonly retro_vfs_mkdir_t _mkDir                   = MkDir;
        //private static readonly retro_vfs_opendir_t _openDir               = OpenDir;    
        //private static readonly retro_vfs_readdir_t _readDir               = ReadDir;    
        //private static readonly retro_vfs_dirent_get_name_t _direntGetName = DirentGetName;                  
        //private static readonly retro_vfs_dirent_is_dir_t _direntIsDir     = DirentIsDir;              
        //private static readonly retro_vfs_closedir_t _closeDir             = CloseDir;      

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

            _interfacePtr = PointerUtilities.Alloc<retro_vfs_interface>();
            Marshal.StructureToPtr(_interface, _interfacePtr, false);
            interfaceInfo.iface = _interfacePtr;

            data.Write(_interfacePtr);
            return true;
        }

        [MonoPInvokeCallback(typeof(retro_vfs_get_path_t))]
        private static string GetPath(ref retro_vfs_file_handle stream)
            => Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream) ? fileStream.Name : "";

        [MonoPInvokeCallback(typeof(retro_vfs_open_t))]
        private static IntPtr Open(string path, uint mode, uint hints)
        {
            try
            {
                FileMode fileMode = (mode & (uint)RETRO_VFS_FILE_ACCESS.UPDATE_EXISTING) == (uint)RETRO_VFS_FILE_ACCESS.UPDATE_EXISTING
                                  ? FileMode.Append
                                  : (mode & (uint)RETRO_VFS_FILE_ACCESS.READ_WRITE) == (uint)RETRO_VFS_FILE_ACCESS.READ_WRITE ? FileMode.Create : FileMode.Open;

                FileStream stream = File.Open(path, fileMode);
                IntPtr handle = stream.SafeFileHandle.DangerousGetHandle();
                Wrapper.Instance.VFSHandler._files.Add(handle, stream);
                return handle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        [MonoPInvokeCallback(typeof(retro_vfs_close_t))]
        private static int Close(ref retro_vfs_file_handle stream)
        {
            try
            {
                if (!Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                _ = Wrapper.Instance.VFSHandler._files.Remove(stream.handle);
                fileStream.Close();
                fileStream.Dispose();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        [MonoPInvokeCallback(typeof(retro_vfs_size_t))]
        private static long Size(ref retro_vfs_file_handle stream)
            => Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream) ? fileStream.Length : -1;

        [MonoPInvokeCallback(typeof(retro_vfs_tell_t))]
        private static long Tell(ref retro_vfs_file_handle stream)
            => Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream) ? fileStream.Position : -1;

        [MonoPInvokeCallback(typeof(retro_vfs_seek_t))]
        private static long Seek(ref retro_vfs_file_handle stream, long offset, int seek_position)
        {
            try
            {
                if (seek_position is < (int)RETRO_VFS_SEEK_POSITION.START or > (int)RETRO_VFS_SEEK_POSITION.END)
                    return -1;

                if (!Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream))
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

        [MonoPInvokeCallback(typeof(retro_vfs_read_t))]
        private static long Read(ref retro_vfs_file_handle stream, IntPtr s, long len)
        {
            try
            {
                if (len > int.MaxValue)
                    return -1;

                if (!Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream))
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

        [MonoPInvokeCallback(typeof(retro_vfs_write_t))]
        private static long Write(ref retro_vfs_file_handle stream, IntPtr s, long len)
        {
            try
            {
                if (len > int.MaxValue)
                    return -1;

                if (!Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream))
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

        [MonoPInvokeCallback(typeof(retro_vfs_flush_t))]
        private static int Flush(ref retro_vfs_file_handle stream)
        {
            try
            {
                if (!Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                fileStream.Flush();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        [MonoPInvokeCallback(typeof(retro_vfs_remove_t))]
        private static int Remove(string path)
        {
            try
            {
                FileSystem.DeleteFile(path);
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        [MonoPInvokeCallback(typeof(retro_vfs_rename_t))]
        private static int Rename(string old_path, string new_path)
        {
            try
            {
                FileSystem.MoveFile(old_path, new_path, true);
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        [MonoPInvokeCallback(typeof(retro_vfs_truncate_t))]
        private static long Truncate(ref retro_vfs_file_handle stream, long length)
        {
            try
            {
                if (!Wrapper.Instance.VFSHandler._files.TryGetValue(stream.handle, out FileStream fileStream))
                    return -1;

                fileStream.SetLength(length);
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        //[MonoPInvokeCallback(typeof(retro_vfs_stat_t))]
        //private static int Stat(string path, ref int size) => 0;

        //[MonoPInvokeCallback(typeof(retro_vfs_mkdir_t))]
        //private static int MkDir(string dir) => 0;

        //[MonoPInvokeCallback(typeof(retro_vfs_opendir_t))]
        //private static IntPtr OpenDir(string dir, bool include_hidden) => IntPtr.Zero;

        //[MonoPInvokeCallback(typeof(retro_vfs_readdir_t))]
        //private static bool ReadDir(ref retro_vfs_dir_handle dirstream) => false;

        //[MonoPInvokeCallback(typeof(retro_vfs_dirent_get_name_t))]
        //private static string DirentGetName(ref retro_vfs_dir_handle dirstream) => "";

        //[MonoPInvokeCallback(typeof(retro_vfs_dirent_is_dir_t))]
        //private static bool DirentIsDir(ref retro_vfs_dir_handle dirstream) => false;

        //[MonoPInvokeCallback(typeof(retro_vfs_closedir_t))]
        //private static int CloseDir(ref retro_vfs_dir_handle dirstream) => 0;
    }
}
