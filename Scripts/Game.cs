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
        public retro_game_info_ext GameInfoExt = new();

        private readonly Wrapper _wrapper;
        private readonly string _gameDirectory;
        private readonly ContentOverrides _contentOverrides = new();
        private readonly List<retro_system_content_info_override> _systemContentInfoOverrides = new();

        private string _path          = "";
        private string _extractedPath = "";
        private bool _needFullPath    = false;
        private bool _persistentData  = false;

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
                        var archivePath = $"{_gameDirectory}/{Name}.zip";
                        if (FileSystem.FileExists(archivePath))
                        {
                            var extractDirectory = FileSystem.GetOrCreateDirectory($"{Wrapper.TempDirectory}/extracted/{Name}_{Guid.NewGuid()}");
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

            var directory = Path.GetDirectoryName(_path).Replace(Path.DirectorySeparatorChar, '/');
            var name      = Path.GetFileNameWithoutExtension(_path);
            var extension = Path.GetExtension(_path).TrimStart('.');

            GameInfoExt.full_path       = _wrapper.GetUnsafeString(_path);
            GameInfoExt.archive_path    = IntPtr.Zero;
            GameInfoExt.archive_file    = IntPtr.Zero;
            GameInfoExt.dir             = _wrapper.GetUnsafeString(directory);
            GameInfoExt.name            = _wrapper.GetUnsafeString(name);
            GameInfoExt.ext             = _wrapper.GetUnsafeString(extension);
            GameInfoExt.meta            = IntPtr.Zero;

            if (!_needFullPath)
            {
                try
                {
                    using FileStream stream = new(_path, FileMode.Open);
                    var gameData = new byte[stream.Length];
                    _ = stream.Read(gameData, 0, (int)stream.Length);
                    GameInfoExt.data = PointerUtilities.Alloc(gameData.Length);
                    Marshal.Copy(gameData, 0, GameInfoExt.data, gameData.Length);
                    GameInfoExt.size = (uint)gameData.Length;
                    GameInfoExt.file_in_archive = false;
                    GameInfoExt.persistent_data = _persistentData;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            GameInfoExt.ToPointer(data);
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
            _wrapper.GraphicsHandler.ResizeHwRender(SystemAVInfo.BaseWidth, SystemAVInfo.BaseHeight);
            return true;
        }

        public bool SetContentInfoOverride(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _systemContentInfoOverrides.Clear();

            var infoOverride = data.ToStructure<retro_system_content_info_override>();
            while (infoOverride.extensions != IntPtr.Zero && !string.IsNullOrWhiteSpace(infoOverride.extensions.AsString()))
            {
                _systemContentInfoOverrides.Add(infoOverride);

                var extensionsString = infoOverride.extensions.AsString();
                var extensions = extensionsString.Split('|');
                foreach (var extension in extensions)
                    _contentOverrides.Add(extension, infoOverride.need_fullpath, infoOverride.persistent_data);

                data += Marshal.SizeOf<retro_system_content_info_override>();
                infoOverride = data.ToStructure<retro_system_content_info_override>();
            }

            return true;
        }

        public void FreeGameInfo()
        {
            PointerUtilities.Free(ref GameInfo.data);
            GameInfo = default;

            PointerUtilities.Free(ref GameInfoExt.data);
            GameInfoExt = default;
        }

        private string GetGamePath(string directory, string gameName)
        {
            if (_wrapper.Core.SystemInfo.ValidExtensions is null)
                return null;

            foreach (var extension in _wrapper.Core.SystemInfo.ValidExtensions)
            {
                var filePath = $"{directory}/{gameName}.{extension}";
                if (FileSystem.FileExists(filePath))
                    return filePath.Replace(Path.DirectorySeparatorChar, '/');
            }

            return null;
        }

        private string GetExtractedGamePath(string directory)
        {
            if (_wrapper.Core.SystemInfo.ValidExtensions is null)
                return null;

            foreach (var extension in _wrapper.Core.SystemInfo.ValidExtensions)
                foreach (var filePath in Directory.EnumerateFiles(directory, $"*.{extension}", SearchOption.AllDirectories))
                    return filePath.Replace(Path.DirectorySeparatorChar, '/');

            return null;
        }

        private bool GetGameInfo()
        {
            if (string.IsNullOrWhiteSpace(_path))
                return _wrapper.Core.SupportNoGame;

            GameInfo.path = _wrapper.GetUnsafeString(_path);

            (var result, var contentOverride) = _contentOverrides.TryGet(Path.GetExtension(_path).TrimStart('.'));
            _needFullPath   = result ? contentOverride.NeedFullpath : _wrapper.Core.SystemInfo.NeedFullPath;
            _persistentData = result && contentOverride.PersistentData;
            if (_needFullPath)
                return true;

            try
            {
                using FileStream stream = new(_path, FileMode.Open);
                var data = new byte[stream.Length];
                _ = stream.Read(data, 0, (int)stream.Length);
                GameInfo.data = PointerUtilities.Alloc(data.Length);
                Marshal.Copy(data, 0, GameInfo.data, data.Length);
                GameInfo.size = (uint)data.Length;
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

                _wrapper.Core.GetSystemAVInfo(out var info);
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
