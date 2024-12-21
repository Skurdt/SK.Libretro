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
    internal sealed class DiskHandler
    {
        public const int VERSION = 1;

        public bool Enabled { get; set; }

        private retro_set_eject_state_t _set_eject_state;
        private retro_get_eject_state_t _get_eject_state;
        private retro_get_image_index_t _get_image_index;
        private retro_set_image_index_t _set_image_index;
        private retro_get_num_images_t _get_num_images;
        private retro_replace_image_index_t _replace_image_index;
        private retro_add_image_index_t _add_image_index;
        private retro_set_initial_image_t _set_initial_image;
        private retro_get_image_path_t _get_image_path;
        private retro_get_image_label_t _get_image_label;

        public bool SetEjectState(bool ejected) => _set_eject_state(ejected);

        public bool GetEjectState() => _get_eject_state();

        public uint GetImageIndex() => _get_image_index();

        public bool SetImageIndex(uint index) => _set_image_index(index);

        public bool SetImageIndexAuto(uint index, string path)
        {
            if (_get_eject_state())
                return false;

            if (!_set_eject_state(true))
                return false;

            foreach (string extension in Wrapper.Instance.Core.SystemInfo.ValidExtensions)
            {
                try
                {
                    string filePath = $"{path}.{extension}";
                    if (!FileSystem.FileExists(filePath))
                        continue;

                    Game game = Wrapper.Instance.Game;
                    game.GameInfo.path = Wrapper.Instance.GetUnsafeString(filePath);
                    using FileStream stream = new(filePath, FileMode.Open);
                    byte[] data = new byte[stream.Length];
                    game.GameInfo.size = (nuint)data.Length;
                    game.GameInfo.data = PointerUtilities.Alloc(data.Length * Marshal.SizeOf<byte>());
                    _ = stream.Read(data, 0, (int)stream.Length);
                    Marshal.Copy(data, 0, game.GameInfo.data, data.Length);
                    if (!_replace_image_index(index, ref game.GameInfo))
                    {
                        game.FreeGameInfo();
                        return false;
                    }

                    game.FreeGameInfo();

                    return _set_image_index(index) && _set_eject_state(false);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        public uint GetNumImages() => _get_num_images();
        
        public bool ReplaceImageIndex(uint index, ref retro_game_info info) => _replace_image_index(index, ref info);
        
        public bool AddImageIndex() => _add_image_index();
        
        public bool SetInitialImage(uint index, string path) => _set_initial_image(index, path);
        
        public bool GetImagePath(uint index, ref string path, nuint len) => _get_image_path(index, ref path, len);
        
        public bool GetImageLabel(uint index, ref string label, nuint len) => _get_image_label(index, ref label, len);

        public bool GetDiskControlInterfaceVersion(IntPtr data)
        {
            if (data.IsNull())
                return false;

            data.Write(VERSION);
            return true;
        }

        public bool SetDiskControlInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_disk_control_callback callback = data.ToStructure<retro_disk_control_callback>();

            _set_eject_state     = callback.set_eject_state.GetDelegate<retro_set_eject_state_t>();
            _get_eject_state     = callback.get_eject_state.GetDelegate<retro_get_eject_state_t>();
            _get_image_index     = callback.get_image_index.GetDelegate<retro_get_image_index_t>();
            _set_image_index     = callback.set_image_index.GetDelegate<retro_set_image_index_t>();
            _get_num_images      = callback.get_num_images.GetDelegate<retro_get_num_images_t>();
            _replace_image_index = callback.replace_image_index.GetDelegate<retro_replace_image_index_t>();
            _add_image_index     = callback.add_image_index.GetDelegate<retro_add_image_index_t>();
            _set_initial_image   = (uint index, string path) => false;
            _get_image_path      = (uint index, ref string path, nuint len) => false;
            _get_image_label     = (uint index, ref string label, nuint len) => false;

            Enabled = true;
            return true;
        }

        public bool SetDiskControlExtInterface(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_disk_control_ext_callback callback = data.ToStructure<retro_disk_control_ext_callback>();

            _set_eject_state     = callback.set_eject_state.GetDelegate<retro_set_eject_state_t>();
            _get_eject_state     = callback.get_eject_state.GetDelegate<retro_get_eject_state_t>();
            _get_image_index     = callback.get_image_index.GetDelegate<retro_get_image_index_t>();
            _set_image_index     = callback.set_image_index.GetDelegate<retro_set_image_index_t>();
            _get_num_images      = callback.get_num_images.GetDelegate<retro_get_num_images_t>();
            _replace_image_index = callback.replace_image_index.GetDelegate<retro_replace_image_index_t>();
            _add_image_index     = callback.add_image_index.GetDelegate<retro_add_image_index_t>();
            _set_initial_image   = callback.set_initial_image.GetDelegate<retro_set_initial_image_t>();
            _get_image_path      = callback.get_image_path.GetDelegate<retro_get_image_path_t>();
            _get_image_label     = callback.get_image_label.GetDelegate<retro_get_image_label_t>();

            Enabled = true;
            return true;
        }
    }
}
