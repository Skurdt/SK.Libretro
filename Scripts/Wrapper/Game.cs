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
using System.IO;
using System.Runtime.InteropServices;
using static SK.Libretro.Header;

namespace SK.Libretro
{
    internal sealed class Game
    {
        public double VideoFps => SystemAVInfo.timing.fps;
        public int VideoWidth => (int)SystemAVInfo.geometry.max_width;
        public int VideoHeight => (int)SystemAVInfo.geometry.max_height;

        public string Name { get; private set; }
        public bool Running { get; private set; }

        public retro_game_info GameInfo          = new();
        public retro_game_info_ext GameInfoExt   = new();
        public retro_system_av_info SystemAVInfo = new();

        public ContentOverrides ContentOverrides = new();

        private readonly Wrapper _wrapper;

        private string _path          = "";
        private string _extractedPath = "";

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
                _wrapper.Core.retro_unload_game();
                Running = false;
            }

            FreeGameInfo();

            if (!string.IsNullOrWhiteSpace(_extractedPath) && FileSystem.FileExists(_extractedPath))
                _ = FileSystem.DeleteFile(_extractedPath);
        }

        private string GetGamePath(string directory, string gameName)
        {
            if (_wrapper.Core.SystemInfo.ValidExtensions == null)
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

            GameInfo.path = Marshal.StringToHGlobalAnsi(_path);

            string directory = Path.GetDirectoryName(_path);
            string name      = Path.GetFileNameWithoutExtension(_path);
            string extension = Path.GetExtension(_path).TrimStart('.');

            GameInfoExt.full_path       = Marshal.StringToHGlobalAnsi(_path);
            GameInfoExt.dir             = Marshal.StringToHGlobalAnsi(directory);
            GameInfoExt.name            = Marshal.StringToHGlobalAnsi(name);
            GameInfoExt.ext             = Marshal.StringToHGlobalAnsi(extension);
            GameInfoExt.file_in_archive = false;
            GameInfoExt.persistent_data = false;

            bool needFullPath = ContentOverrides.Get(extension, out ContentOverride contentOverride)
                              ? contentOverride.NeedFullpath
                              : _wrapper.Core.SystemInfo.NeedFullpath;
            if (!needFullPath)
            {
                try
                {
                    using FileStream stream = new(_path, FileMode.Open);
                    byte[] data             = new byte[stream.Length];
                    GameInfo.data           = Marshal.AllocHGlobal(data.Length * Marshal.SizeOf<byte>());
                    GameInfoExt.data        = Marshal.AllocHGlobal(data.Length * Marshal.SizeOf<byte>());
                    GameInfo.size           = (ulong)data.Length;
                    GameInfoExt.size        = (ulong)data.Length;
                    _ = stream.Read(data, 0, (int)stream.Length);
                    Marshal.Copy(data, 0, GameInfo.data, data.Length);
                    Marshal.Copy(data, 0, GameInfoExt.data, data.Length);
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

                _wrapper.Core.retro_get_system_av_info(out SystemAVInfo);
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
            }

            return false;
        }

        public void FreeGameInfo()
        {
            if (GameInfo.data != IntPtr.Zero)
                Marshal.FreeHGlobal(GameInfo.data);
            if (GameInfo.path != IntPtr.Zero)
                Marshal.FreeHGlobal(GameInfo.path);
            GameInfo = default;

            if (GameInfoExt.full_path != IntPtr.Zero)
                Marshal.FreeHGlobal(GameInfoExt.full_path);
            if (GameInfoExt.dir != IntPtr.Zero)
                Marshal.FreeHGlobal(GameInfoExt.dir);
            if (GameInfoExt.name != IntPtr.Zero)
                Marshal.FreeHGlobal(GameInfoExt.name);
            if (GameInfoExt.ext != IntPtr.Zero)
                Marshal.FreeHGlobal(GameInfoExt.ext);
            if (GameInfoExt.data != IntPtr.Zero)
                Marshal.FreeHGlobal(GameInfoExt.data);
            GameInfoExt = default;
        }
    }
}
