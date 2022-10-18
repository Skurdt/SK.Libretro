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
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class Wrapper
    {
        public static readonly retro_log_level LogLevel = retro_log_level.RETRO_LOG_WARN;

        public static string MainDirectory       { get; private set; } = null;
        public static string CoresDirectory      { get; private set; } = null;
        public static string SystemDirectory     { get; private set; } = null;
        public static string CoreAssetsDirectory { get; private set; } = null;
        public static string OptionsDirectory    { get; private set; } = null;
        public static string SavesDirectory      { get; private set; } = null;
        public static string StatesDirectory     { get; private set; } = null;
        public static string TempDirectory       { get; private set; } = null;

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

        public bool RewindEnabled = false;
        public bool PerformRewind = false;

        public retro_frame_time_callback FrameTimeInterface;
        public retro_frame_time_callback_t FrameTimeInterfaceCallback;

        //private const int REWIND_FRAMES_INTERVAL = 10;

        private readonly List<IntPtr> _unsafeStrings = new();

        private long _frameTimeLast      = 0;
        //private uint _totalFrameCount    = 0;

        public unsafe Wrapper(WrapperSettings settings)
        {
            Settings = settings;

            if (MainDirectory is null)
            {
                MainDirectory       = FileSystem.GetOrCreateDirectory(!string.IsNullOrWhiteSpace(settings.MainDirectory) ? settings.MainDirectory : "libretro");
                CoresDirectory      = FileSystem.GetOrCreateDirectory($"{MainDirectory}/cores");
                SystemDirectory     = FileSystem.GetOrCreateDirectory($"{MainDirectory}/system");
                CoreAssetsDirectory = FileSystem.GetOrCreateDirectory($"{MainDirectory}/core_assets");
                OptionsDirectory    = FileSystem.GetOrCreateDirectory($"{MainDirectory}/core_options");
                SavesDirectory      = FileSystem.GetOrCreateDirectory($"{MainDirectory}/saves");
                StatesDirectory     = FileSystem.GetOrCreateDirectory($"{MainDirectory}/states");
                TempDirectory       = FileSystem.GetOrCreateDirectory($"{MainDirectory}/temp");
            }

            Core = new(this);
            Game = new(this);

            EnvironmentHandler       = new(this);
            GraphicsHandler          = new(this);
            AudioHandler             = new(this, settings.AudioProcessor);
            InputHandler             = new(settings.InputProcessor);
            LogHandler               = settings.Platform switch
            {
                Platform.Win => new LogHandlerWin(settings.LogProcessor),
                _            => new LogHandler(settings.LogProcessor),
            };
            OptionsHandler           = new(this);
            VFSHandler               = new();
            SerializationHandler     = new(this);
            DiskHandler              = new(this);
            PerfHandler              = new();
            LedHandler               = new(settings.LedProcessor);
            MessageHandler           = new(this, settings.MessageProcessor);
            MemoryHandler            = new();

            CoreInstances.Instance.Add(this);
        }

        public bool StartContent(string coreName, string gameDirectory, string gameName)
        {
            if (string.IsNullOrWhiteSpace(coreName))
                return false;

            if (!Core.Start(coreName))
            {
                StopContent();
                return false;
            }

            if (FrameTimeInterface.callback.IsNotNull())
                FrameTimeInterfaceCallback = FrameTimeInterface.callback.GetDelegate<retro_frame_time_callback_t>();

            if (!Game.Start(gameDirectory, gameName))
            {
                StopContent();
                return false;
            }

            FrameTimeRestart();

            ulong size = Core.SerializeSize();
            if (size > 0)
                SerializationHandler.SetStateSize(size);

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
            CoreInstances.Instance.Remove(this);

            Game.Dispose();
            Core.Dispose();

            GraphicsHandler.Dispose();
            AudioHandler.Dispose();
            LedHandler.Dispose();
            VFSHandler.Dispose();

            PointerUtilities.Free(_unsafeStrings);
        }

        public void RunFrame()
        {
            if (!Game.Running || !Core.Initialized)
                return;

            if (Core.HwAccelerated)
                GLFW.PollEvents();

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

        public void InitGraphics(GraphicsFrameHandlerBase graphicsFrameHandler, bool enabled) =>
            GraphicsHandler.Init(graphicsFrameHandler, enabled);

        public void InitAudio(bool enabled) =>
            AudioHandler.Init(enabled);

        public bool GetSystemDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{MainDirectory}/system");

            IntPtr stringPtr = GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        public bool GetLibretroPath(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory(Core.Path);
            IntPtr stringPtr = GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        public bool GetCoreAssetsDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{CoreAssetsDirectory}/{Core.Name}");
            IntPtr stringPtr = GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        public bool GetSaveDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{SavesDirectory}/{Core.Name}");
            IntPtr stringPtr = GetUnsafeString(path);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        public bool GetUsername(IntPtr data)
        {
            if (data.IsNull())
                return false;

            IntPtr stringPtr = GetUnsafeString(Settings.UserName);
            Marshal.StructureToPtr(stringPtr, data, true);
            return true;
        }

        public bool GetLanguage(IntPtr data)
        {
            if (data.IsNull())
                return false;

            IntPtr stringPtr = GetUnsafeString(Settings.Language.ToString());
            Marshal.StructureToPtr(stringPtr, data, true);
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
            if (FrameTimeInterfaceCallback == null)
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
