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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SK.Libretro
{
    internal sealed class Wrapper
    {
        public static string CoresDirectory   { get; private set; } = null;
        public static string OptionsDirectory { get; private set; } = null;
        public static string SavesDirectory   { get; private set; } = null;
        public static string StatesDirectory  { get; private set; } = null;
        public static string TempDirectory    { get; private set; } = null;

        public bool RewindEnabled { get; private set; } = false;
        public bool PerformRewind { get; private set; } = false;

        public readonly WrapperSettings Settings;

        public readonly Core Core;
        public readonly Game Game;

        public readonly EnvironmentHandler EnvironmentHandler;
        public readonly GraphicsHandler GraphicsHandler;
        public readonly AudioHandler AudioHandler;
        public readonly InputHandler InputHandler;
        public readonly LogHandler LogHandler;
        public readonly OptionsHandler OptionsHandler;
        public readonly VFSHandler VFSHandler;
        public readonly SerializationHandler SerializationHandler;
        public readonly DiskHandler DiskHandler;
        public readonly PerfHandler PerfHandler;
        public readonly LedHandler LedHandler;
        public readonly MessageHandler MessageHandler;
        public readonly MemoryHandler MemoryHandler;

        public retro_frame_time_callback FrameTimeInterface;
        public retro_frame_time_callback_t FrameTimeInterfaceCallback;

        //private const int REWIND_FRAMES_INTERVAL = 10;

        //private static Wrapper _instance;
        private static string _mainDirectory;
        private static string _systemDirectory;
        private static string _coreAssetsDirectory;

        private static readonly ConcurrentDictionary<Thread, Wrapper> _instances = new();

        private readonly Thread _thread;
        private readonly List<IntPtr> _unsafeStrings = new();

        private long _frameTimeLast = 0;
        //private uint _totalFrameCount = 0;

        public Wrapper(WrapperSettings settings, string coreName, string gameDirectory, string[] gameNames)
        {
            _thread  = Thread.CurrentThread;
            Settings = settings;

            if (_mainDirectory is null)
            {
                _mainDirectory       = FileSystem.GetOrCreateDirectory(!string.IsNullOrWhiteSpace(settings.MainDirectory) ? settings.MainDirectory : "libretro");
                TempDirectory        = FileSystem.GetOrCreateDirectory(!string.IsNullOrWhiteSpace(settings.TempDirectory) ? settings.TempDirectory : $"{_mainDirectory}/temp");
                CoresDirectory       = FileSystem.GetOrCreateDirectory($"{_mainDirectory}/cores");
                _systemDirectory     = FileSystem.GetOrCreateDirectory($"{_mainDirectory}/system");
                _coreAssetsDirectory = FileSystem.GetOrCreateDirectory($"{_mainDirectory}/core_assets");
                OptionsDirectory     = FileSystem.GetOrCreateDirectory($"{_mainDirectory}/core_options");
                SavesDirectory       = FileSystem.GetOrCreateDirectory($"{_mainDirectory}/saves");
                StatesDirectory      = FileSystem.GetOrCreateDirectory($"{_mainDirectory}/states");
            }

            Core = new(this, coreName);
            Game = new(this, gameDirectory, gameNames);

            EnvironmentHandler       = new(this);
            GraphicsHandler          = new(this, settings.GraphicsProcessor);
            AudioHandler             = new(this, settings.AudioProcessor);
            InputHandler             = new(this, settings.InputProcessor);
            LogHandler               = settings.Platform switch
            {
                Platform.Win => new LogHandlerWin(this, settings.LogProcessor, settings.LogLevel),
                _            => new LogHandler(this, settings.LogProcessor, settings.LogLevel),
            };
            OptionsHandler           = new(this);
            VFSHandler               = new(this);
            SerializationHandler     = new(this);
            DiskHandler              = new(this);
            PerfHandler              = new();
            LedHandler               = new(this, settings.LedProcessor);
            MessageHandler           = new(this, settings.MessageProcessor);
            MemoryHandler            = new(this);
        }

#if ENABLE_IL2CPP
        public static bool TryGetInstance(Thread thread, out Wrapper wrapper) => _instances.TryGetValue(thread, out wrapper);
#endif
        public bool StartContent()
        {
            if (string.IsNullOrWhiteSpace(Core.Name))
                return false;

            if (!_instances.TryAdd(_thread, this))
                return false;

            if (!Core.Start())
            {
                StopContent();
                return false;
            }

            if (FrameTimeInterface.callback.IsNotNull())
                FrameTimeInterfaceCallback = FrameTimeInterface.callback.GetDelegate<retro_frame_time_callback_t>();

            if (!Game.Start())
            {
                StopContent();
                return false;
            }

            if (DiskHandler.Enabled)
                for (int i = 0; i < Game.Names.Length; ++i)
                    _ = DiskHandler.AddImageIndex();

            SerializationHandler.Init();

            FrameTimeRestart();

            return true;
        }

        public void ResetContent()
        {
            if (!Game.Running || !Core.Initialized)
                return;

            Core.Reset();
        }

        public void StopContent()
        {
            Game.Dispose();
            Core.Dispose();

            GraphicsHandler.Dispose();
            AudioHandler.Dispose();
            VFSHandler.Dispose();

            PointerUtilities.Free(_unsafeStrings);

            _ = _instances.Remove(_thread, out _);
        }

        public void RunFrame()
        {
            if (!Game.Running || !Core.Initialized)
                return;

            GraphicsHandler.PollEvents();
            //_totalFrameCount++;

            FrameTimeUpdate();

            //if (RewindEnabled)
            //{
            //    if (PerformRewind)
            //        Serialization.RewindLoadState();
            //    else if (_totalFrameCount % REWIND_FRAMES_INTERVAL == 0)
            //        Serialization.RewindSaveState();
            //}

            Core.Run();
        }

        public void InitGraphics(bool enabled = true) => GraphicsHandler.Init(enabled);

        public void InitAudio(bool enabled = true) => AudioHandler.Init(enabled);

        public bool GetSystemDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            IntPtr stringPtr = GetUnsafeString(_systemDirectory);
            data.Write(stringPtr);
            return true;
        }

        public bool GetLibretroPath(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory(Core.Path);
            IntPtr stringPtr = GetUnsafeString(path);
            data.Write(stringPtr);
            return true;
        }

        public bool GetCoreAssetsDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{_coreAssetsDirectory}/{Core.Name}");
            IntPtr stringPtr = GetUnsafeString(path);
            data.Write(stringPtr);
            return true;
        }

        public bool GetUsername(IntPtr data)
        {
            if (data.IsNull())
                return false;

            IntPtr stringPtr = GetUnsafeString(Settings.UserName);
            data.Write(stringPtr);
            return true;
        }

        public bool GetLanguage(IntPtr data)
        {
            if (data.IsNull())
                return false;

            data.Write((uint)Settings.Language);
            return true;
        }

        public bool Shutdown() => false;

        public IntPtr GetUnsafeString(string source)
        {
            IntPtr ptr = source.AsAllocatedPtr();
            _unsafeStrings.Add(ptr);
            return ptr;
        }

        private void FrameTimeRestart() => _frameTimeLast = System.Diagnostics.Stopwatch.GetTimestamp();

        private void FrameTimeUpdate()
        {
            if (FrameTimeInterfaceCallback is null)
                return;

            long current = System.Diagnostics.Stopwatch.GetTimestamp();
            long delta   = current - _frameTimeLast;

            if (_frameTimeLast <= 0)
                delta = FrameTimeInterface.reference;
            _frameTimeLast = current;
            FrameTimeInterfaceCallback(delta * 1000);
        }
    }
}
