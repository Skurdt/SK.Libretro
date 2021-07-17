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
    internal sealed class LibretroSerialization
    {
        private readonly LibretroWrapper _wrapper;

        private ulong _quirks;

        public LibretroSerialization(LibretroWrapper wrapper) => _wrapper = wrapper;

        public void SetQuirks(ulong quirks) => _quirks = quirks;

        public unsafe bool SaveState(int index, out string outPath)
        {
            outPath = null;

            ulong stateSize = _wrapper.Core.retro_serialize_size();
            Logger.Instance.LogWarning($"Save: {stateSize}");
            if (stateSize == 0)
                return false;

            byte[] data = new byte[stateSize];
            fixed (byte* p = data)
            {
                if (!_wrapper.Core.retro_serialize(p, stateSize))
                    return false;

                string coreDirectory = Path.Combine(LibretroWrapper.SavesDirectory, _wrapper.Core.Name);
                if (!Directory.Exists(coreDirectory))
                    _ = Directory.CreateDirectory(coreDirectory);

                string gameDirectory = !(_wrapper.Game.Name is null) ? Path.Combine(coreDirectory, Path.GetFileNameWithoutExtension(_wrapper.Game.Name)) : null;
                if (!(gameDirectory is null) && !Directory.Exists(gameDirectory))
                    _ = Directory.CreateDirectory(gameDirectory);

                outPath = !(gameDirectory is null) ? Path.Combine(gameDirectory, $"save_{index}.state") : Path.Combine(coreDirectory, $"save_{index}.state");
                File.WriteAllBytes(outPath, data);
            }

            return true;
        }

        public unsafe bool LoadState(int index)
        {
            string coreDirectory = Path.Combine(LibretroWrapper.SavesDirectory, _wrapper.Core.Name);
            if (!Directory.Exists(coreDirectory))
                return false;

            string gameDirectory = !(_wrapper.Game.Name is null) ? Path.Combine(coreDirectory, Path.GetFileNameWithoutExtension(_wrapper.Game.Name)) : null;
            if (!(gameDirectory is null) && !Directory.Exists(gameDirectory))
                return false;

            string savePath = !(gameDirectory is null) ? Path.Combine(gameDirectory, $"save_{index}.state") : Path.Combine(coreDirectory, $"save_{index}.state");

            if (!File.Exists(savePath))
                return false;

            ulong stateSize = _wrapper.Core.retro_serialize_size();
            Logger.Instance.LogWarning($"Load: {stateSize}");
            if (stateSize == 0)
                return false;

            byte[] data = File.ReadAllBytes(savePath);
            fixed (byte* p = data)
                Logger.Instance.LogError(_wrapper.Core.retro_unserialize(p, stateSize)); // FIXME: This returns false, not sure why

            return true;
        }

        public bool SaveSRAM()
        {
            int saveSize = (int)_wrapper.Core.retro_get_memory_size(RETRO_MEMORY_SAVE_RAM);
            if (saveSize == 0)
                return false;

            IntPtr saveData = _wrapper.Core.retro_get_memory_data(RETRO_MEMORY_SAVE_RAM);
            if (saveData == IntPtr.Zero)
                return false;

            byte[] data = new byte[saveSize];
            Marshal.Copy(saveData, data, 0, saveSize);

            string coreDirectory = Path.Combine(LibretroWrapper.SavesDirectory, _wrapper.Core.Name);
            if (!Directory.Exists(coreDirectory))
                _ = Directory.CreateDirectory(coreDirectory);

            string path = Path.Combine(coreDirectory, $"{_wrapper.Game.Name}.srm");
            File.WriteAllBytes(path, data);

            return true;
        }

        public bool LoadSRAM()
        {
            int saveSize = (int)_wrapper.Core.retro_get_memory_size(RETRO_MEMORY_SAVE_RAM);
            if (saveSize == 0)
                return false;

            IntPtr saveData = _wrapper.Core.retro_get_memory_data(RETRO_MEMORY_SAVE_RAM);
            if (saveData == IntPtr.Zero)
                return false;

            string coreDirectory = Path.Combine(LibretroWrapper.SavesDirectory, _wrapper.Core.Name);
            if (!Directory.Exists(coreDirectory))
                _ = Directory.CreateDirectory(coreDirectory);

            string path = Path.Combine(coreDirectory, $"{_wrapper.Game.Name}.srm");
            byte[] data = File.ReadAllBytes(path);
            Marshal.Copy(data, 0, saveData, saveSize);

            return true;
        }
    }
}
