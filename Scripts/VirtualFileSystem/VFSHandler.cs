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
    internal sealed class VFSHandler
    {
        // Logging helper for diagnostics
        private static void LogVfsException(string method, Exception ex)
        {
            var msg = $"[VFSHandler] Exception in {method}: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            try
            {
                File.AppendAllText("VFSHandler.log", msg + "\n");
            }
            catch { }
        }

        private const int SUPPORTED_VERSION = 3;
        private const int RFILE_HINT_UNBUFFERED = 1 << 8;

        private static Wrapper _wrapper;

        private static readonly retro_vfs_get_path_t _getPath = GetPath;
        private static readonly retro_vfs_open_t _open = Open;
        private static readonly retro_vfs_close_t _close = Close;
        private static readonly retro_vfs_size_t _size = Size;
        private static readonly retro_vfs_tell_t _tell = Tell;
        private static readonly retro_vfs_seek_t _seek = Seek;
        private static readonly retro_vfs_read_t _read = Read;
        private static readonly retro_vfs_write_t _write = Write;
        private static readonly retro_vfs_flush_t _flush = Flush;
        private static readonly retro_vfs_remove_t _remove = Remove;
        private static readonly retro_vfs_rename_t _rename = Rename;
        private static readonly retro_vfs_truncate_t _truncate = Truncate;
        private static readonly retro_vfs_stat_t _stat = Stat;
        private static readonly retro_vfs_mkdir_t _mkDir = MkDir;
        private static readonly retro_vfs_opendir_t _openDir = OpenDir;
        private static readonly retro_vfs_readdir_t _readDir = ReadDir;
        private static readonly retro_vfs_dirent_get_name_t _direntGetName = DirentGetName;
        private static readonly retro_vfs_dirent_is_dir_t _direntIsDir = DirentIsDir;
        private static readonly retro_vfs_closedir_t _closeDir = CloseDir;

        private static readonly retro_vfs_interface _interface;
        private static readonly IntPtr _interfacePtr;
        // Keep GCHandles to prevent GC moving or collecting pinned objects
        private struct FileHandleInfo
        {
            public FileHandle Handle;
            public GCHandle PinnedStruct;
            public GCHandle PinnedPath;
        }

        private struct DirHandleInfo
        {
            public DirHandle Handle;
            public GCHandle PinnedStruct;
            public List<GCHandle> PinnedEntries;
        }

        private static readonly Dictionary<IntPtr, DirHandleInfo> _openDirectories = new();
        private static readonly HashSet<string> _openDirPaths = new();

        // Managed file handle wrapper
        private sealed class FileHandle
        {
            public FileStream Stream { get; }
            public string Path { get; }
            public FileHandle(FileStream stream, string path)
            {
                Stream = stream;
                Path = path;
            }
        }

        // Managed directory handle wrapper
        private sealed class DirHandle
        {
            public string Path { get; }
            public string[] Entries { get; }
            public int Index { get; set; }
            public DirHandle(string path, string[] entries)
            {
                Path = path;
                Entries = entries;
                Index = -1;
            }
        }

        static VFSHandler()
        {
            _interface = new()
            {
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
                truncate        = _truncate.GetFunctionPointer(),
                stat            = _stat.GetFunctionPointer(),
                mkdir           = _mkDir.GetFunctionPointer(),
                opendir         = _openDir.GetFunctionPointer(),
                readdir         = _readDir.GetFunctionPointer(),
                dirent_get_name = _direntGetName.GetFunctionPointer(),
                dirent_is_dir   = _direntIsDir.GetFunctionPointer(),
                closedir        = _closeDir.GetFunctionPointer()
            };
            _interfacePtr = PointerUtilities.Alloc<retro_vfs_interface>();
            _interface.ToPointer(_interfacePtr);
        }

        public static void Init(Wrapper wrapper) => _wrapper = wrapper;

        public bool GetVfsInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;
            var interfaceInfo = data.ToStructure<retro_vfs_interface_info>();
            if (interfaceInfo.required_interface_version > SUPPORTED_VERSION)
                return false;
            interfaceInfo.iface = _interfacePtr;
            interfaceInfo.ToPointer(data);
            return true;
        }

        // --- VFS function implementations ---

        private static IntPtr GetPath(IntPtr stream)
        {
            if (stream == IntPtr.Zero)
                return IntPtr.Zero;

            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                return _wrapper.GetUnsafeString(fileStream.Name);
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(GetPath), ex);
                return IntPtr.Zero;
            }
        }

        private static IntPtr Open(IntPtr path, uint mode, uint hints)
        {
            if (path == IntPtr.Zero)
                return IntPtr.Zero;

            try
            {
                var stream = new retro_vfs_file_handle
                {
                    size = 0,
                    mappos = 0,
                    mapsize = 0,
                    fp = IntPtr.Zero,
                    fh = IntPtr.Zero,
                    buf = IntPtr.Zero,
                    orig_path = IntPtr.Zero,
                    mapped = IntPtr.Zero,
                    fd = -1,
                    hints = hints,
                    scheme = RETRO.vfs_scheme.VFS_SCHEME_NONE
                };

                string pathStr = null;
                {
                    pathStr = path.AsString();
                    if (pathStr.Length > "vfsonly://".Length -1
                     && pathStr[0] == 'v'
                     && pathStr[1] == 'f'
                     && pathStr[2] == 's'
                     && pathStr[3] == 'o'
                     && pathStr[4] == 'n'
                     && pathStr[5] == 'l'
                     && pathStr[6] == 'y'
                     && pathStr[7] == ':'
                     && pathStr[8] == '/'
                     && pathStr[9] == '/')
                        path += "vfsonly://".Length -1;
                    pathStr = path.AsString();

                    stream.orig_path = path;
                }

                stream.hints &= ~(uint)RETRO_VFS_FILE_ACCESS_HINT.FREQUENT_ACCESS;

                FileMode fileMode;
                FileAccess fileAccess;
                switch (mode)
                {
                    case (uint)RETRO_VFS_FILE_ACCESS.READ:
                        fileMode = FileMode.Open;
                        fileAccess = FileAccess.Read;
                        break;
                    case (uint)RETRO_VFS_FILE_ACCESS.WRITE:
                        fileMode = FileMode.Create;
                        fileAccess = FileAccess.Write;
                        break;
                    case (uint)RETRO_VFS_FILE_ACCESS.READ_WRITE:
                        fileMode = FileMode.OpenOrCreate;
                        fileAccess = FileAccess.ReadWrite;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), "Invalid access mode");
                }
                
                var fileStream = new FileStream(pathStr, fileMode, fileAccess, FileShare.ReadWrite);
                var fileStreamHandle = fileStream.SafeFileHandle.DangerousGetHandle();
                if ((stream.hints & RFILE_HINT_UNBUFFERED) == 0)
                    stream.fp = fileStreamHandle;

                stream.fd = (int)fileStreamHandle.ToInt64();

                if (stream.fd == -1)
                {
                    fileStream.Dispose();
                    return IntPtr.Zero;
                }

                stream.size = fileStream.Length;

                var streamPtr = PointerUtilities.Alloc<retro_vfs_file_handle>();
                stream.ToPointer(streamPtr);
                return streamPtr;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Open), ex);
                return IntPtr.Zero;
            }
        }

        private static int Close(IntPtr stream)
        {
            if (stream == IntPtr.Zero)
                return -1;
            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                if ((handle.hints & RFILE_HINT_UNBUFFERED) == 0 && handle.fp != IntPtr.Zero)
                {
                    var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(handle.fp, false), FileAccess.ReadWrite);
                    fileStream.Dispose();
                }

                if (handle.fd > 0)
                {
                    var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                    fileStream.Dispose();
                }
                
                PointerUtilities.Free(ref handle.buf);
                PointerUtilities.Free(ref handle.orig_path);
                PointerUtilities.Free(ref stream);

                return 0;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Close), ex);
                return -1;
            }
        }

        private static long Size(IntPtr stream)
        {
            if (stream == IntPtr.Zero)
                return -1;

            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                return fileStream.Length;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Size), ex);
                return -1;
            }
        }

        private static long Tell(IntPtr stream)
        {
            if (stream == IntPtr.Zero)
                return -1;

            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                return fileStream.Position;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Tell), ex);
                return -1;
            }
        }

        private static long Seek(IntPtr stream, long offset, int seek_position)
        {
            if (stream == IntPtr.Zero)
                return -1;

            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                var origin = seek_position switch
                {
                    0 => SeekOrigin.Begin,
                    1 => SeekOrigin.Current,
                    2 => SeekOrigin.End,
                    _ => throw new ArgumentOutOfRangeException(nameof(seek_position), "Invalid seek position")
                };
                return fileStream.Seek(offset, origin);
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Seek), ex);
                return -1;
            }
        }

        private static long Read(IntPtr stream, IntPtr s, long len)
        {
            if (stream == IntPtr.Zero || s == IntPtr.Zero || len > int.MaxValue)
                return -1;

            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                var buffer = new byte[len];
                var bytesRead = fileStream.Read(buffer, 0, (int)len);
                Marshal.Copy(buffer, 0, s, bytesRead);
                return bytesRead;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Read), ex);
                return -1;
            }
        }

        private static long Write(IntPtr stream, IntPtr s, long len)
        {
            if (stream == IntPtr.Zero || s == IntPtr.Zero || len > int.MaxValue)
                return -1;

            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                var buffer = new byte[len];
                Marshal.Copy(s, buffer, 0, (int)len);
                fileStream.Write(buffer, 0, (int)len);
                return len;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Write), ex);
                return -1;
            }
        }

        private static int Flush(IntPtr stream)
        {
            if (stream == IntPtr.Zero)
                return -1;

            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                fileStream.Flush();
                return 0;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Flush), ex);
                return -1;
            }
        }

        private static int Remove(IntPtr path)
        {
            if (path == IntPtr.Zero)
                return -1;

            try
            {
                var pathStr = path.AsString();
                File.Delete(pathStr);
                return 0;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Remove), ex);
                return -1;
            }
        }

        private static int Rename(IntPtr old_path, IntPtr new_path)
        {
            if (old_path == IntPtr.Zero || new_path == IntPtr.Zero)
                return -1;

            try
            {
                var oldPathStr = old_path.AsString();
                var newPathStr = new_path.AsString();
                File.Move(oldPathStr, newPathStr);
                return 0;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Rename), ex);
                return -1;
            }
        }

        private static long Truncate(IntPtr stream, long length)
        {
            if (stream == IntPtr.Zero)
                return -1;

            try
            {
                var handle = stream.ToStructure<retro_vfs_file_handle>();
                var fileStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(new IntPtr(handle.fd), false), FileAccess.ReadWrite);
                fileStream.SetLength(length);
                handle.size = length;
                handle.ToPointer(stream);
                return 0;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Truncate), ex);
                return -1;
            }
        }

        private static int Stat(IntPtr path, ref int size)
        {
            if (path == IntPtr.Zero)
                return 0;

            try
            {
                var pathStr = path.AsString();
                if (Directory.Exists(pathStr))
                {
                    size = 0;
                    return (int)(RETRO_VFS_STAT.IS_VALID | RETRO_VFS_STAT.IS_DIRECTORY);
                }
                var fileInfo = new FileInfo(pathStr);
                if (fileInfo.Exists)
                {
                    size = (int)fileInfo.Length;
                    return (int)RETRO_VFS_STAT.IS_VALID;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(Stat), ex);
                return 0;
            }
        }

        private static int MkDir(IntPtr dir)
        {
            if (dir == IntPtr.Zero)
                return -1;

            try
            {
                var dirStr = dir.AsString();
                if (Directory.Exists(dirStr))
                    return -2;
                _ = Directory.CreateDirectory(dirStr);
                return 0;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(MkDir), ex);
                return -1;
            }
        }

        private static IntPtr OpenDir(IntPtr path, bool includeHidden)
        {
            if (path == IntPtr.Zero)
                return IntPtr.Zero;

            try
            {
                var pathStr = path.AsString();
                if (_openDirPaths.Contains(pathStr))
                {
                    // Already open, do not allow duplicate
                    return IntPtr.Zero;
                }
                var entries = Directory.Exists(pathStr)
                    ? Directory.GetFileSystemEntries(pathStr)
                    : Array.Empty<string>();
                var dirHandle = new DirHandle(pathStr, entries);
                // Pin the path string for orig_path
                var pinnedPath = GCHandle.Alloc(System.Text.Encoding.UTF8.GetBytes(pathStr + "\0"), GCHandleType.Pinned);
                var retroHandle = new retro_vfs_dir_handle { orig_path = pinnedPath.AddrOfPinnedObject() };
                // Pin the struct itself
                var pinnedHandle = GCHandle.Alloc(retroHandle, GCHandleType.Pinned);
                var ptr = pinnedHandle.AddrOfPinnedObject();
                // Pin all entry strings for the lifetime of the dir handle
                var entryHandles = new List<GCHandle>();
                foreach (var entry in entries)
                    entryHandles.Add(GCHandle.Alloc(System.Text.Encoding.UTF8.GetBytes(entry + "\0"), GCHandleType.Pinned));
                var info = new DirHandleInfo
                {
                    Handle = dirHandle,
                    PinnedStruct = pinnedHandle,
                    PinnedEntries = entryHandles
                };
                _openDirectories.Add(ptr, info);
                _ = _openDirPaths.Add(pathStr);
                return ptr;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(OpenDir), ex);
                return IntPtr.Zero;
            }
        }

        private static bool ReadDir(IntPtr dirstream)
        {
            if (dirstream == IntPtr.Zero)
                return false;

            try
            {
                if (!_openDirectories.TryGetValue(dirstream, out var info))
                    return false;
                info.Handle.Index++;
                if (info.Handle.Index >= 0 && info.Handle.Index < info.Handle.Entries.Length)
                {
                    // Update the retro_vfs_dir_handle's orig_path to the current entry
                    var retroHandle = dirstream.ToStructure<retro_vfs_dir_handle>();
                    retroHandle.orig_path = _wrapper.GetUnsafeString(info.Handle.Entries[info.Handle.Index]);
                    retroHandle.ToPointer(dirstream);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(ReadDir), ex);
                return false;
            }
        }

        private static IntPtr DirentGetName(IntPtr dirstream)
        {
            if (dirstream == IntPtr.Zero)
                return IntPtr.Zero;

            try
            {
                return !_openDirectories.TryGetValue(dirstream, out var info)
                    ? IntPtr.Zero
                    : info.Handle.Index >= 0 && info.Handle.Index < info.Handle.Entries.Length
                    ? _wrapper.GetUnsafeString(Path.GetFileName(info.Handle.Entries[info.Handle.Index]))
                    : IntPtr.Zero;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(DirentGetName), ex);
                return IntPtr.Zero;
            }
        }

        private static bool DirentIsDir(IntPtr dirstream)
        {
            if (dirstream == IntPtr.Zero)
                return false;

            try
            {
                return _openDirectories.TryGetValue(dirstream, out var info) && info.Handle.Index >= 0 && info.Handle.Index < info.Handle.Entries.Length && Directory.Exists(info.Handle.Entries[info.Handle.Index]);
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(DirentIsDir), ex);
                return false;
            }
        }

        private static int CloseDir(IntPtr dirstream)
        {
            if (dirstream == IntPtr.Zero)
                return -1;
                
            try
            {
                if (!_openDirectories.TryGetValue(dirstream, out var info))
                    return -1;
                _ = _openDirectories.Remove(dirstream);
                _ = _openDirPaths.Remove(info.Handle.Path);
                info.PinnedStruct.Free(); // pinned struct
                foreach (var h in info.PinnedEntries) h.Free(); // pinned entry strings
                return 0;
            }
            catch (Exception ex)
            {
                LogVfsException(nameof(CloseDir), ex);
                return -1;
            }
        }
    }
}
