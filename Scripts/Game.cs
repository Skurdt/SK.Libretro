/* MIT License

 * Copyright (c) 2022 Skurdt
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
    internal sealed class Game
    {
        public bool Running { get; private set; }
        public string Name { get; private set; }
        public SystemAVInfo SystemAVInfo { get; private set; }

        public retro_game_info GameInfo = new();

        private readonly Wrapper _wrapper;
        private readonly ContentOverrides _contentOverrides = new();
        private readonly List<retro_system_content_info_override> _systemContentInfoOverrides = new();

        private string _path          = "";
        private string _extractedPath = "";

        private retro_game_info_ext _gameInfoExt = new();

        public Game(Wrapper wrapper) => _wrapper = wrapper;

        public bool Start(string gameDirectory, string gameName)
        {
            Name = gameName;

            try
            {
                if (!string.IsNullOrWhiteSpace(gameDirectory) && !string.IsNullOrWhiteSpace(gameName))
                {
                    _path = GetGamePath(gameDirectory, gameName);
                    if (_path == null)
                    {
                        // Try Zip archive
                        // TODO(Tom): Check for any file after extraction instead of exact game name (only the archive needs to match)
                        string archivePath = $"{gameDirectory}/{gameName}.zip";
                        if (File.Exists(archivePath))
                        {
                            string extractDirectory = $"{Wrapper.TempDirectory}/extracted/{gameName}_{Guid.NewGuid()}";
                            if (!Directory.Exists(extractDirectory))
                                _ = Directory.CreateDirectory(extractDirectory);
                            System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, extractDirectory);

                            _path = GetGamePath(extractDirectory, gameName);
                            _extractedPath = _path;
                        }
                    }
                }

                if (!GetGameInfo())
                {
                    Logger.Instance.LogError($"Game not set, core '{_wrapper.Core.Name}' needs a game to run.", "Libretro.LibretroGame.Start");
                    return false;
                }

                Running = LoadGame();

                FreeGameInfo();

                return Running;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
            }

            return false;
        }

        public void Stop()
        {
            if (Running)
            {
                //FIXME(Tom): This sometimes crashes
                _wrapper.Core.retro_unload_game();
                Running = false;
            }

            FreeGameInfo();

            if (!string.IsNullOrWhiteSpace(_extractedPath) && FileSystem.FileExists(_extractedPath))
                _ = FileSystem.DeleteFile(_extractedPath);
        }

        public void GetGameInfoExt(ref IntPtr data) => Marshal.StructureToPtr(_gameInfoExt, data, true);

        public void SetSystemAVInfo(retro_system_av_info info) => SystemAVInfo = new(ref info);

        public void SetGeometry(retro_game_geometry geometry)
        {
            if (SystemAVInfo.BaseWidth != geometry.base_width
             || SystemAVInfo.BaseHeight != geometry.base_height
             || SystemAVInfo.AspectRatio != geometry.aspect_ratio)
            {
                SystemAVInfo.SetGeometry(ref geometry);
                // TODO: Set video aspect ratio if needed
            }
        }

        public void SetContentInfoOverride(ref IntPtr data)
        {
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
            if (_wrapper.Core.ValidExtensions is null)
                return null;

            foreach (string extension in _wrapper.Core.ValidExtensions)
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
                return _wrapper.EnvironmentVariables.SupportNoGame;

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
            bool needFullPath = result ? contentOverride.NeedFullpath : _wrapper.Core.NeedFullPath;
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
                if (!_wrapper.Core.retro_load_game(ref GameInfo))
                    return false;

                _wrapper.Core.retro_get_system_av_info(out retro_system_av_info info);
                SystemAVInfo = new(ref info);
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
            }

            return false;
        }
    }
}
