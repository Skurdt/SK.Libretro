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
using System.IO;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class Serialization
    {
        //private const int REWIND_NUM_MAX_STATES = 256;

        private readonly Wrapper _wrapper;
        //private readonly Deque<byte[]> _rewindStates;
        private ulong _quirks;
        private ulong _stateSize;

        public Serialization(Wrapper wrapper)
        {
            _wrapper      = wrapper;
            //_rewindStates = new Deque<byte[]>(REWIND_NUM_MAX_STATES);
        }

        public void SetQuirks(ulong quirks) => _quirks = quirks;

        public void SetStateSize(ulong size) => _stateSize = size;

        public bool SaveState(int index)
        {
            try
            {
                nuint stateSize = _wrapper.Core.retro_serialize_size();
                if (stateSize == 0)
                    return false;

                byte[] data = new byte[stateSize];
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
                if (!_wrapper.Core.retro_serialize(ptr, stateSize))
                    return false;

                string coreDirectory = $"{Wrapper.StatesDirectory}/{_wrapper.Core.Name}";
                if (!Directory.Exists(coreDirectory))
                    _ = Directory.CreateDirectory(coreDirectory);

                string gameDirectory = _wrapper.Game.Name != null ? $"{coreDirectory}/{Path.GetFileNameWithoutExtension(_wrapper.Game.Name)}" : null;
                if (gameDirectory != null && !Directory.Exists(gameDirectory))
                    _ = Directory.CreateDirectory(gameDirectory);

                string path = $"{gameDirectory ?? coreDirectory}/save_{index}.state";
                File.WriteAllBytes(path, data);

                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                return false;
            }
        }

        public bool SaveState(int index, out string outPath)
        {
            outPath = null;
            try
            {
                nuint stateSize = _wrapper.Core.retro_serialize_size();
                if (stateSize == 0)
                    return false;

                if (stateSize != _stateSize)
                    _stateSize = stateSize;

                byte[] data = new byte[stateSize];
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                if (!_wrapper.Core.retro_serialize(handle.AddrOfPinnedObject(), stateSize))
                    return false;

                string coreDirectory = $"{Wrapper.StatesDirectory}/{_wrapper.Core.Name}";
                if (!Directory.Exists(coreDirectory))
                    _ = Directory.CreateDirectory(coreDirectory);

                string gameDirectory = _wrapper.Game.Name != null ? $"{coreDirectory}/{Path.GetFileNameWithoutExtension(_wrapper.Game.Name)}" : null;
                if (gameDirectory != null && !Directory.Exists(gameDirectory))
                    _ = Directory.CreateDirectory(gameDirectory);

                outPath = $"{gameDirectory ?? coreDirectory}/save_{index}.state";
                File.WriteAllBytes(outPath, data);

                handle.Free();
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                return false;
            }
        }

        public bool LoadState(int index)
        {
            try
            {
                string coreDirectory = $"{Wrapper.StatesDirectory}/{_wrapper.Core.Name}";
                if (!Directory.Exists(coreDirectory))
                    return false;

                string gameDirectory = _wrapper.Game.Name != null ? $"{coreDirectory}/{Path.GetFileNameWithoutExtension(_wrapper.Game.Name)}" : null;
                if (gameDirectory != null && !Directory.Exists(gameDirectory))
                    return false;

                string savePath = $"{gameDirectory ?? coreDirectory}/save_{index}.state";

                if (!File.Exists(savePath))
                    return false;

                nuint stateSize = _wrapper.Core.retro_serialize_size();
                if (stateSize == 0 || stateSize != _stateSize)
                    return false;

                byte[] data = File.ReadAllBytes(savePath);
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                _ = _wrapper.Core.retro_unserialize(handle.AddrOfPinnedObject(), stateSize); // FIXME: This returns false, not sure why
                handle.Free();
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                return false;
            }
        }

        public void RewindSaveState()
        {
            //try
            //{
            //    if (_stateSize == 0)
            //        return;

            //    //ulong size = _wrapper.Core.retro_serialize_size();
            //    //if (size != _stateSize)
            //    //    _stateSize = size;

            //    //if (_rewindStates.Count == REWIND_NUM_MAX_STATES)
            //    //    _rewindStates.AddToBack(_rewindStates.RemoveFromFront());
            //    //else
            //    //    _rewindStates.AddToBack(new byte[_stateSize]);

            //    //GCHandle handle = GCHandle.Alloc(_rewindStates[_rewindStates.Count - 1], GCHandleType.Pinned);
            //    //_ = _wrapper.Core.retro_serialize(handle.AddrOfPinnedObject(), _stateSize);
            //    //handle.Free();
            //}
            //catch (Exception e)
            //{
            //    Logger.Instance.LogException(e);
            //}
        }

        public void RewindLoadState()
        {
            //try
            //{
            //    if (_stateSize == 0 || _rewindStates.Count == 0)
            //        return;

            //    //ulong size = _wrapper.Core.retro_serialize_size();
            //    //if (size != _stateSize)
            //    //    return;

            //    GCHandle handle = GCHandle.Alloc(_rewindStates.RemoveFromBack(), GCHandleType.Pinned);
            //    _ = _wrapper.Core.retro_unserialize(handle.AddrOfPinnedObject(), _stateSize);
            //    handle.Free();
            //}
            //catch (Exception e)
            //{
            //    Logger.Instance.LogException(e);
            //}
        }

        public bool SaveSRAM()
        {
            try
            {
                int saveSize = (int)_wrapper.Core.retro_get_memory_size(RETRO_MEMORY.SAVE_RAM);
                if (saveSize == 0)
                    return false;

                IntPtr saveData = _wrapper.Core.retro_get_memory_data(RETRO_MEMORY.SAVE_RAM);
                if (saveData.IsNull())
                    return false;

                byte[] data = new byte[saveSize];
                Marshal.Copy(saveData, data, 0, saveSize);

                string coreDirectory = $"{Wrapper.SavesDirectory}/{_wrapper.Core.Name}";
                if (!Directory.Exists(coreDirectory))
                    _ = Directory.CreateDirectory(coreDirectory);

                string path = $"{coreDirectory}/{_wrapper.Game.Name}.srm";
                File.WriteAllBytes(path, data);

                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                return false;
            }
        }

        public bool LoadSRAM()
        {
            try
            {
                int saveSize = (int)_wrapper.Core.retro_get_memory_size(RETRO_MEMORY.SAVE_RAM);
                if (saveSize == 0)
                    return false;

                IntPtr saveData = _wrapper.Core.retro_get_memory_data(RETRO_MEMORY.SAVE_RAM);
                if (saveData.IsNull())
                    return false;

                string coreDirectory = $"{Wrapper.SavesDirectory}/{_wrapper.Core.Name}";
                if (!Directory.Exists(coreDirectory))
                    _ = Directory.CreateDirectory(coreDirectory);

                string path = $"{coreDirectory}/{_wrapper.Game.Name}.srm";
                if (!FileSystem.FileExists(path))
                    return false;

                byte[] data = File.ReadAllBytes(path);
                if (data == null || data.Length == 0)
                    return false;

                Marshal.Copy(data, 0, saveData, saveSize);
                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
                return false;
            }
        }
    }
}
