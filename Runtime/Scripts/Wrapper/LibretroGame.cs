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

using SK.Libretro.Utilities;
using System;
using System.IO;
using System.Runtime.InteropServices;
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroGame
    {
        public double VideoFps => SystemAVInfo.timing.fps;
        public int VideoWidth => (int)SystemAVInfo.geometry.max_width;
        public int VideoHeight => (int)SystemAVInfo.geometry.max_height;

        public string Name { get; private set; }
        public bool Running { get; private set; }

        public retro_game_info GameInfo          = new retro_game_info();
        public retro_system_av_info SystemAVInfo = new retro_system_av_info();
        public retro_pixel_format PixelFormat;

        public readonly string[,] ButtonDescriptions = new string[LibretroInput.MAX_USERS, LibretroInput.FIRST_META_KEY];
        public bool HasInputDescriptors;

        private readonly LibretroWrapper _wrapper;

        private string _path = "";
        private string _extractedPath = "";

        public LibretroGame(LibretroWrapper wrapper) => _wrapper = wrapper;

        public bool Start(string gameDirectory, string gameName)
        {
            Name = gameName;

            try
            {
                if (!string.IsNullOrEmpty(gameDirectory) && !string.IsNullOrEmpty(gameName))
                {
                    string directory = FileSystem.GetAbsolutePath(gameDirectory);
                    _path = GetGamePath(directory, gameName);
                    if (_path == null)
                    {
                        // Try Zip archive
                        // TODO(Tom): Check for any file after extraction instead of exact game name (only the archive needs to match)
                        string archivePath = FileSystem.GetAbsolutePath($"{directory}/{gameName}.zip");
                        if (File.Exists(archivePath))
                        {
                            string extractDirectory = FileSystem.GetAbsolutePath($"{LibretroWrapper.TempDirectory}/extracted/{gameName}_{Guid.NewGuid()}");
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
                    Logger.Instance.LogWarning($"Game not set, running '{_wrapper.Core.Name}' core only.", "Libretro.LibretroGame.Start");
                    return false;
                }

                Running = LoadGame();
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
                _wrapper.Core.retro_unload_game();
                Running = false;
            }

            if (GameInfo.data != IntPtr.Zero)
                Marshal.FreeHGlobal(GameInfo.data);

            if (!string.IsNullOrEmpty(_extractedPath) && FileSystem.FileExists(_extractedPath))
                _ = FileSystem.DeleteFile(_extractedPath);
        }

        private string GetGamePath(string directory, string gameName)
        {
            if (_wrapper.Core.ValidExtensions == null)
                return null;

            foreach (string extension in _wrapper.Core.ValidExtensions)
            {
                string filePath = FileSystem.GetAbsolutePath($"{directory}/{gameName}.{extension}");
                if (FileSystem.FileExists(filePath))
                    return filePath;
            }

            return null;
        }

        private bool GetGameInfo()
        {
            if (string.IsNullOrEmpty(_path))
            {
                if (!_wrapper.Core.SupportNoGame)
                {
                    Logger.Instance.LogError($"Game not set, core '{_wrapper.Core.Name}' needs a game to run.", "Libretro.LibretroGame.Start");
                    return false;
                }

                return true;
            }

            GameInfo.path = _path;

            if (!_wrapper.Core.NeedFullpath)
            {
                using FileStream stream = new FileStream(_path, FileMode.Open);
                byte[] data = new byte[stream.Length];

                GameInfo.size = (ulong)data.Length;
                GameInfo.data = Marshal.AllocHGlobal(data.Length * Marshal.SizeOf<byte>());

                _ = stream.Read(data, 0, (int)stream.Length);
                Marshal.Copy(data, 0, GameInfo.data, data.Length);
            }

            return true;
        }

        private bool LoadGame()
        {
            try
            {
                if (!_wrapper.Core.retro_load_game(ref GameInfo))
                    return false;

                _wrapper.Core.retro_get_system_av_info(out SystemAVInfo);
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
