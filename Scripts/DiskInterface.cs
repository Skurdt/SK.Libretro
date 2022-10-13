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
    internal sealed class DiskInterface
    {
        public const int VERSION = 1;

        private readonly Wrapper _wrapper;
        private readonly retro_disk_control_ext_callback _callback;

        public DiskInterface(Wrapper wrapper, retro_disk_control_callback callback)
        {
            _wrapper  = wrapper;
            _callback = new()
            {
                set_eject_state     = callback.set_eject_state     ?? ((bool ejected) => false),
                get_eject_state     = callback.get_eject_state     ?? (() => false),
                get_image_index     = callback.get_image_index     ?? (() => 0),
                set_image_index     = callback.set_image_index     ?? ((uint index) => false),
                get_num_images      = callback.get_num_images      ?? (() => 0),
                replace_image_index = callback.replace_image_index ?? ((uint index, ref retro_game_info info) => false),
                add_image_index     = callback.add_image_index     ?? (() => false),
                set_initial_image   = (uint index, string path) => false,
                get_image_path      = (uint index, ref string path, nuint len) => false,
                get_image_label     = (uint index, ref string label, nuint len) => false
            };
        }

        public DiskInterface(Wrapper wrapper, retro_disk_control_ext_callback callback)
        {
            _wrapper  = wrapper;
            _callback = new()
            {
                set_eject_state     = callback.set_eject_state     ?? ((bool ejected) => false),
                get_eject_state     = callback.get_eject_state     ?? (() => false),
                get_image_index     = callback.get_image_index     ?? (() => 0),
                set_image_index     = callback.set_image_index     ?? ((uint index) => false),
                get_num_images      = callback.get_num_images      ?? (() => 0),
                replace_image_index = callback.replace_image_index ?? ((uint index, ref retro_game_info info) => false),
                add_image_index     = callback.add_image_index     ?? (() => false),
                set_initial_image   = callback.set_initial_image   ?? ((uint index, string path) => false),
                get_image_path      = callback.get_image_path      ?? ((uint index, ref string path, nuint len) => false),
                get_image_label     = callback.get_image_label     ?? ((uint index, ref string label, nuint len) => false)
            };
        }

        public bool SetEjectState(bool ejected) =>
            _callback.set_eject_state(ejected);

        public bool GetEjectState() =>
            _callback.get_eject_state();

        public uint GetImageIndex() =>
            _callback.get_image_index();

        public bool SetImageIndex(uint index) =>
            _callback.set_image_index(index);

        public bool SetImageIndexAuto(uint index, string path)
        {
            if (_callback.get_eject_state())
                return false;

            if (!_callback.set_eject_state(true))
                return false;

            foreach (string extension in _wrapper.Core.ValidExtensions)
            {
                string filePath = $"{path}.{extension}";
                if (!FileSystem.FileExists(filePath))
                    continue;

                _wrapper.Game.GameInfo.path = filePath.AsAllocatedPtr();
                try
                {
                    using FileStream stream = new(filePath, FileMode.Open);
                    byte[] data = new byte[stream.Length];
                    _wrapper.Game.GameInfo.size = (nuint)data.Length;
                    _wrapper.Game.GameInfo.data = Marshal.AllocHGlobal(data.Length * Marshal.SizeOf<byte>());
                    _ = stream.Read(data, 0, (int)stream.Length);
                    Marshal.Copy(data, 0, _wrapper.Game.GameInfo.data, data.Length);
                }
                catch (Exception)
                {
                    return false;
                }

                if (!_callback.replace_image_index(index, ref _wrapper.Game.GameInfo))
                {
                    _wrapper.Game.FreeGameInfo();
                    return false;
                }

                _wrapper.Game.FreeGameInfo();

                return _callback.set_image_index(index) && _callback.set_eject_state(false);
            }

            return false;
        }

        public uint GetNumImages() =>
            _callback.get_num_images();
        
        public bool ReplaceImageIndex(uint index, ref retro_game_info info) =>
            _callback.replace_image_index(index, ref info);
        
        public bool AddImageIndex() =>
            _callback.add_image_index();
        
        public bool SetInitialImage(uint index, string path) =>
            _callback.set_initial_image(index, path);
        
        public bool GetImagePath(uint index, ref string path, nuint len) =>
            _callback.get_image_path(index, ref path, len);
        
        public bool GetImageLabel(uint index, ref string label, nuint len) =>
            _callback.get_image_label(index, ref label, len);
    }
}
