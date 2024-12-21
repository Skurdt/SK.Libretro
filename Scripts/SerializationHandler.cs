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
using System.IO;
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal sealed class SerializationHandler
    {
        private const int DISK_NUM_MAX_STATES = 999999;

        //private const int REWIND_NUM_MAX_STATES = 256;

        //private readonly Deque<byte[]> _rewindStates;
        private ulong _quirks;
        private string _coreDirectory;
        private string _gameDirectory;
        private int _currentStateSlot;

        public SerializationHandler()
        {
            //_rewindStates = new Deque<byte[]>(REWIND_NUM_MAX_STATES);
        }

        public void Init()
        {
            _coreDirectory = FileSystem.GetOrCreateDirectory($"{Wrapper.StatesDirectory}/{Wrapper.Instance.Core.Name}");
            _gameDirectory = !string.IsNullOrWhiteSpace(Wrapper.Instance.Game.Name)
                           ? $"{_coreDirectory}/{Wrapper.Instance.Game.Name}"
                           : null;
        }

        public bool GetSaveDirectory(IntPtr data)
        {
            if (data.IsNull())
                return false;

            string path = FileSystem.GetOrCreateDirectory($"{Wrapper.SavesDirectory}/{Wrapper.Instance.Core.Name}");
            IntPtr stringPtr = Wrapper.Instance.GetUnsafeString(path);
            data.Write(stringPtr);
            return true;
        }

        public void SetStateSlot(int slot) => _currentStateSlot = slot.Clamp(0, DISK_NUM_MAX_STATES);

        public bool SaveStateToDisk()
        {
            try
            {
                if (!TrySaveState(out byte[] data))
                    return false;

                if (_gameDirectory is not null && !Directory.Exists(_gameDirectory))
                    _ = Directory.CreateDirectory(_gameDirectory);

                string path = $"{_gameDirectory ?? _coreDirectory}/save_{_currentStateSlot}.state";
                File.WriteAllBytes(path, data);
                return true;
            }
            catch (Exception e)
            {
                Wrapper.Instance.LogHandler.LogException(e);
                return false;
            }
        }

        public bool SaveStateToDisk(out string path)
        {
            try
            {
                if (!TrySaveState(out byte[] data))
                {
                    path = null;
                    return false;
                }

                if (_gameDirectory is not null && !Directory.Exists(_gameDirectory))
                    _ = Directory.CreateDirectory(_gameDirectory);

                path = $"{_gameDirectory ?? _coreDirectory}/save_{_currentStateSlot}.state";
                File.WriteAllBytes(path, data);
                return true;
            }
            catch (Exception e)
            {
                Wrapper.Instance.LogHandler.LogException(e);
                path = null;
                return false;
            }
        }

        public bool LoadStateFromDisk()
        {
            GCHandle handle = default;
            try
            {
                string coreDirectory = $"{Wrapper.StatesDirectory}/{Wrapper.Instance.Core.Name}";
                if (!Directory.Exists(coreDirectory))
                    return false;

                if (_gameDirectory is not null && !Directory.Exists(_gameDirectory))
                    return false;

                string savePath = $"{_gameDirectory ?? coreDirectory}/save_{_currentStateSlot}.state";
                if (!FileSystem.FileExists(savePath))
                    return false;

                long stateSize = Wrapper.Instance.Core.SerializeSize();
                if (stateSize == 0)
                    return false;

                byte[] data = File.ReadAllBytes(savePath);
                handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
                bool result = Wrapper.Instance.Core.Unserialize(ptr, stateSize);
                if (result)
                    Wrapper.Instance.AudioHandler.Init(true);
                return result;
            }
            catch (Exception e)
            {
                Wrapper.Instance.LogHandler.LogException(e);
                return false;
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        public void RewindSaveState()
        {
            //try
            //{
            //    if (_stateSize == 0)
            //        return;

            //    //ulong size = Wrapper.Instance.Core.retro_serialize_size();
            //    //if (size != _stateSize)
            //    //    _stateSize = size;

            //    //if (_rewindStates.Count == REWIND_NUM_MAX_STATES)
            //    //    _rewindStates.AddToBack(_rewindStates.RemoveFromFront());
            //    //else
            //    //    _rewindStates.AddToBack(new byte[_stateSize]);

            //    //GCHandle handle = GCHandle.Alloc(_rewindStates[_rewindStates.Count - 1], GCHandleType.Pinned);
            //    //_ = Wrapper.Instance.Core.retro_serialize(handle.AddrOfPinnedObject(), _stateSize);
            //    //handle.Free();
            //}
            //catch (Exception e)
            //{
            //    Wrapper.Instance.LogHandler.LogException(e);
            //}
        }

        public void RewindLoadState()
        {
            //try
            //{
            //    if (_stateSize == 0 || _rewindStates.Count == 0)
            //        return;

            //    //ulong size = Wrapper.Instance.Core.retro_serialize_size();
            //    //if (size != _stateSize)
            //    //    return;

            //    GCHandle handle = GCHandle.Alloc(_rewindStates.RemoveFromBack(), GCHandleType.Pinned);
            //    _ = Wrapper.Instance.Core.retro_unserialize(handle.AddrOfPinnedObject(), _stateSize);
            //    handle.Free();
            //}
            //catch (Exception e)
            //{
            //    Wrapper.Instance.LogHandler.LogException(e);
            //}
        }

        public bool SaveSRAM()
        {
            try
            {
                int saveSize = (int)Wrapper.Instance.Core.GetMemorySize(RETRO_MEMORY.SAVE_RAM);
                if (saveSize == 0)
                    return false;

                IntPtr saveData = Wrapper.Instance.Core.GetMemoryData(RETRO_MEMORY.SAVE_RAM);
                if (saveData.IsNull())
                    return false;

                byte[] data = new byte[saveSize];
                Marshal.Copy(saveData, data, 0, saveSize);

                string coreDirectory = FileSystem.GetOrCreateDirectory($"{Wrapper.SavesDirectory}/{Wrapper.Instance.Core.Name}");
                string path          = $"{coreDirectory}/{Wrapper.Instance.Game.Name}.srm";
                File.WriteAllBytes(path, data);

                return true;
            }
            catch (Exception e)
            {
                Wrapper.Instance.LogHandler.LogException(e);
                return false;
            }
        }

        public bool LoadSRAM()
        {
            try
            {
                int saveSize = (int)Wrapper.Instance.Core.GetMemorySize(RETRO_MEMORY.SAVE_RAM);
                if (saveSize == 0)
                    return false;

                IntPtr saveData = Wrapper.Instance.Core.GetMemoryData(RETRO_MEMORY.SAVE_RAM);
                if (saveData.IsNull())
                    return false;

                string coreDirectory = $"{Wrapper.SavesDirectory}/{Wrapper.Instance.Core.Name}";
                if (!Directory.Exists(coreDirectory))
                    return false;

                string path = $"{coreDirectory}/{Wrapper.Instance.Game.Name}.srm";
                if (!FileSystem.FileExists(path))
                    return false;

                byte[] data = File.ReadAllBytes(path);
                if (data is null || data.Length == 0)
                    return false;

                Marshal.Copy(data, 0, saveData, saveSize);
                return true;
            }
            catch (Exception e)
            {
                Wrapper.Instance.LogHandler.LogException(e);
                return false;
            }
        }

        public bool SetSerializationQuirks(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _quirks = data.ReadUInt64();
            // _quirks |= Header.RETRO_SERIALIZATION_QUIRK_FRONT_VARIABLE_SIZE;
            return true;
        }

        private bool TrySaveState(out byte[] data)
        {
            GCHandle handle = default;
            try
            {
                long stateSize = Wrapper.Instance.Core.SerializeSize();
                if (stateSize == 0)
                {
                    data = Array.Empty<byte>();
                    return false;
                }

                data = new byte[stateSize];
                handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
                return Wrapper.Instance.Core.Serialize(ptr, stateSize);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }
    }
}
