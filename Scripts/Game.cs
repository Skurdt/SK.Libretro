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
        public bool Running { get; private set; }
        public string Name { get; private set; }
        public SystemAVInfo SystemAVInfo { get; private set; }
        public int Rotation { get; private set; }

        public retro_game_info GameInfo = new();

        private readonly Wrapper _wrapper;
        private readonly ContentOverrides _contentOverrides = new();
        private readonly List<retro_system_content_info_override> _systemContentInfoOverrides = new();

        private string _path          = "";
        private string _extractedPath = "";

        private retro_game_info_ext _gameInfoExt = new();

        public Game(Wrapper wrapper) => _wrapper = wrapper;

        public bool Start(string gameDirectory, string[] gameNames)
        {
            Name = gameNames is not null ? gameNames[0] : null;

            try
            {
                if (!string.IsNullOrWhiteSpace(gameDirectory) && !string.IsNullOrWhiteSpace(Name))
                {
                    _path = GetGamePath(gameDirectory, Name);
                    if (_path is null)
                    {
                        // Try Zip archive
                        // TODO(Tom): Check for any file after extraction instead of exact game name (only the archive needs to match)
                        string archivePath = $"{gameDirectory}/{Name}.zip";
                        if (FileSystem.FileExists(archivePath))
                        {
                            string extractDirectory = FileSystem.GetOrCreateDirectory($"{Wrapper.TempDirectory}/extracted/{Name}_{Guid.NewGuid()}");
                            System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, extractDirectory);

                            _path = GetGamePath(extractDirectory, Name);
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

                FreeGameInfo();

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

            Marshal.StructureToPtr(_gameInfoExt, data, true);
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
                SystemAVInfo.SetGeometry(ref geometry);
                return true;
            }

            return false;
        }

        public bool SetSystemAvInfo(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_system_av_info info = data.ToStructure<retro_system_av_info>();
            SystemAVInfo = new(ref info);
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
            PointerUtilities.Free(ref GameInfo.path);
            PointerUtilities.Free(ref GameInfo.data);
            PointerUtilities.Free(ref GameInfo.meta);
            GameInfo = default;

            PointerUtilities.Free(ref _gameInfoExt.full_path);
            PointerUtilities.Free(ref _gameInfoExt.archive_path);
            PointerUtilities.Free(ref _gameInfoExt.archive_file);
            PointerUtilities.Free(ref _gameInfoExt.dir);
            PointerUtilities.Free(ref _gameInfoExt.name);
            PointerUtilities.Free(ref _gameInfoExt.ext);
            PointerUtilities.Free(ref _gameInfoExt.meta);
            PointerUtilities.Free(ref _gameInfoExt.data);
            _gameInfoExt = default;
        }

        private string GetGamePath(string directory, string gameName)
        {
            if (_wrapper.Core.SystemInfo.ValidExtensions is null)
                return null;

            foreach (string extension in _wrapper.Core.SystemInfo.ValidExtensions)
            {
                string filePath = $"{directory}/{gameName}.{extension}";
                if (FileSystem.FileExists(filePath))
                    return filePath;
            }

            return null;
        }

        private bool GetGameInfo()
        {
            if (string.IsNullOrWhiteSpace(_path))
                return _wrapper.Core.SupportNoGame;

            GameInfo.path = _path.AsAllocatedPtr();

            string directory = Path.GetDirectoryName(_path);
            string name      = Path.GetFileNameWithoutExtension(_path);
            string extension = Path.GetExtension(_path).TrimStart('.');

            _gameInfoExt.full_path       = _path.AsAllocatedPtr();
            _gameInfoExt.dir             = directory.AsAllocatedPtr();
            _gameInfoExt.name            = name.AsAllocatedPtr();
            _gameInfoExt.ext             = extension.AsAllocatedPtr();
            _gameInfoExt.file_in_archive = false;
            _gameInfoExt.persistent_data = false;

            (bool result, ContentOverride contentOverride) = _contentOverrides.TryGet(extension);
            bool needFullPath = result ? contentOverride.NeedFullpath : _wrapper.Core.SystemInfo.NeedFullPath;
            if (!needFullPath)
            {
                try
                {
                    using FileStream stream = new(_path, FileMode.Open);
                    byte[] data             = new byte[stream.Length];
                    GameInfo.data           = Marshal.AllocHGlobal(data.Length * Marshal.SizeOf<byte>());
                    _gameInfoExt.data        = Marshal.AllocHGlobal(data.Length * Marshal.SizeOf<byte>());
                    GameInfo.size           = (nuint)data.Length;
                    _gameInfoExt.size        = (nuint)data.Length;
                    _ = stream.Read(data, 0, (int)stream.Length);
                    Marshal.Copy(data, 0, GameInfo.data, data.Length);
                    Marshal.Copy(data, 0, _gameInfoExt.data, data.Length);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        private bool LoadGame()
        {
            try
            {
                if (!_wrapper.Core.LoadGame(ref GameInfo))
                    return false;

                _wrapper.Core.GetSystemAVInfo(out retro_system_av_info info);
                SystemAVInfo = new(ref info);
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
