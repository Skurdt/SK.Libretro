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
    internal sealed class Game : IDisposable
    {
        public bool Running              { get; private set; }
        public SystemAVInfo SystemAVInfo { get; } = new();
        public int Rotation              { get; private set; }

        public readonly string[] Names;
        public readonly string Name;

        public retro_game_info GameInfo = new();

        private readonly Wrapper _wrapper;
        private readonly string _gameDirectory;
        private readonly ContentOverrides _contentOverrides = new();
        private readonly List<retro_system_content_info_override> _systemContentInfoOverrides = new();

        private string _path          = "";
        private string _extractedPath = "";

        private IntPtr _dataPtr;
        private nuint _dataLength;
        private bool _needFullPath;
        private bool _persistentData;
        private IntPtr _gameInfoExtPtr;

        public Game(Wrapper wrapper, string gameDirectory, string[] gameNames)
        {
            _wrapper       = wrapper;
            _gameDirectory = gameDirectory;
            Names          = gameNames;
            Name           = gameNames[0];
        }

        public bool Start()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_gameDirectory) && !string.IsNullOrWhiteSpace(Name))
                {
                    _path = GetGamePath(_gameDirectory, Name);
                    if (_path is null)
                    {
                        // Try Zip archive
                        string archivePath = $"{_gameDirectory}/{Name}.zip";
                        if (FileSystem.FileExists(archivePath))
                        {
                            string extractDirectory = FileSystem.GetOrCreateDirectory($"{Wrapper.TempDirectory}/extracted/{Name}_{Guid.NewGuid()}");
                            System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, extractDirectory);

                            _path = GetExtractedGamePath(extractDirectory);
                            _extractedPath = _path;
                        }
                    }
                }

                if (!GetGameInfo())
                {
                    _wrapper.LogHandler.LogError($"Game not set, core '{_wrapper.Core.Name}' needs a game to run.", "SK.Libretro.Game.Start");
                    return false;
                }

                Running = LoadGame();

                //FreeGameInfo();

                return Running;
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
            }

            return false;
        }

        public void Dispose()
        {
            if (Running)
            {
                Running = false;
                _wrapper.Core.UnloadGame();
            }

            try
            {
                FreeGameInfo();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (_dataPtr.IsNotNull())
                    PointerUtilities.Free(ref _dataPtr);
                if (_gameInfoExtPtr.IsNotNull())
                    PointerUtilities.Free(ref _gameInfoExtPtr);
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_extractedPath) && FileSystem.FileExists(_extractedPath))
                    FileSystem.DeleteFile(_extractedPath);
            }
            catch
            {

                throw;
            }
        }

        public bool GetGameInfoExt(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _gameInfoExtPtr = PointerUtilities.Alloc<retro_game_info_ext>();

            string directory = Path.GetDirectoryName(_path).Replace(Path.DirectorySeparatorChar, '/');
            string name      = Path.GetFileNameWithoutExtension(_path);
            string extension = Path.GetExtension(_path).TrimStart('.');

            retro_game_info_ext gameInfoExt = new()
            {
                full_path       = _wrapper.GetUnsafeString(_path),
                dir             = _wrapper.GetUnsafeString(directory),
                name            = _wrapper.GetUnsafeString(name),
                ext             = _wrapper.GetUnsafeString(extension),
                file_in_archive = false,
                archive_path    = IntPtr.Zero,
                archive_file    = IntPtr.Zero,
                meta            = IntPtr.Zero
            };

            if (!_needFullPath)
            {
                gameInfoExt.persistent_data = _persistentData;
                gameInfoExt.data            = _dataPtr;
                gameInfoExt.size            = _dataLength;
            }

            Marshal.StructureToPtr(gameInfoExt, _gameInfoExtPtr, false);
            data.Write(_gameInfoExtPtr);
            return true;
        }

        public bool SetRotation(IntPtr data)
        {
            if (data.IsNull())
                return false;

            // ???????????????????
            // ? Input ? Degrees ?
            // ???????????????????
            // ?     0 ?       0 ?
            // ?     1 ?      90 ?
            // ?     2 ?     180 ?
            // ?     3 ?     270 ?
            // ???????????????????

            Rotation = (int)data.ReadUInt32() * 90;
            return _wrapper.Settings.UseCoreRotation;
        }

        public bool SetGeometry(retro_game_geometry geometry)
        {
            if (SystemAVInfo.BaseWidth != geometry.base_width
             || SystemAVInfo.BaseHeight != geometry.base_height
             || SystemAVInfo.AspectRatio != geometry.aspect_ratio)
            {
                SystemAVInfo.SetGeometry(geometry);
                return true;
            }

            return false;
        }

        public bool SetSystemAvInfo(IntPtr data)
        {
            if (data.IsNull())
                return false;

            SystemAVInfo.Init(data.ToStructure<retro_system_av_info>());
            return true;
        }

        public bool SetContentInfoOverride(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _systemContentInfoOverrides.Clear();

            retro_system_content_info_override infoOverride = data.ToStructure<retro_system_content_info_override>();
            while (infoOverride is not null && infoOverride.extensions.IsNotNull())
            {
                _systemContentInfoOverrides.Add(infoOverride);

                string extensionsString = infoOverride.extensions.AsString();
                string[] extensions = extensionsString.Split('|');
                foreach (string extension in extensions)
                    _contentOverrides.Add(extension, infoOverride.need_fullpath, infoOverride.persistent_data);

                data += Marshal.SizeOf(infoOverride);
                data.ToStructure(infoOverride);
            }

            return true;
        }

        public void FreeGameInfo()
        {
            GameInfo = default;

            PointerUtilities.Free(ref _gameInfoExtPtr);
        }

        private string GetGamePath(string directory, string gameName)
        {
            if (_wrapper.Core.SystemInfo.ValidExtensions is null)
                return null;

            foreach (string extension in _wrapper.Core.SystemInfo.ValidExtensions)
            {
                string filePath = $"{directory}/{gameName}.{extension}";
                if (FileSystem.FileExists(filePath))
                    return filePath.Replace(Path.DirectorySeparatorChar, '/');
            }

            return null;
        }

        private string GetExtractedGamePath(string directory)
        {
            if (_wrapper.Core.SystemInfo.ValidExtensions is null)
                return null;

            foreach (string extension in _wrapper.Core.SystemInfo.ValidExtensions)
                foreach (string filePath in Directory.EnumerateFiles(directory, $"*.{extension}", SearchOption.AllDirectories))
                    return filePath.Replace(Path.DirectorySeparatorChar, '/');

            return null;
        }

        private bool GetGameInfo()
        {
            if (string.IsNullOrWhiteSpace(_path))
                return _wrapper.Core.SupportNoGame;

            GameInfo.path = _wrapper.GetUnsafeString(_path);

            (bool result, ContentOverride contentOverride) = _contentOverrides.TryGet(Path.GetExtension(_path).TrimStart('.'));
            _needFullPath   = result ? contentOverride.NeedFullpath : _wrapper.Core.SystemInfo.NeedFullPath;
            _persistentData = result && contentOverride.PersistentData;
            if (_needFullPath)
                return true;

            try
            {
                using FileStream stream = new(_path, FileMode.Open);
                byte[] data = new byte[stream.Length];
                _ = stream.Read(data, 0, (int)stream.Length);

                _dataPtr = PointerUtilities.Alloc(data.Length);
                Marshal.Copy(data, 0, _dataPtr, data.Length);
                _dataLength = (nuint)data.Length;

                GameInfo.data = _dataPtr;
                GameInfo.size = _dataLength;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool LoadGame()
        {
            try
            {
                if (!_wrapper.Core.LoadGame(ref GameInfo))
                    return false;

                _wrapper.Core.GetSystemAVInfo(out retro_system_av_info info);
                SystemAVInfo.Init(info);
                return true;
            }
            catch (Exception e)
            {
                _wrapper.LogHandler.LogException(e);
            }

            return false;
        }
    }
}
