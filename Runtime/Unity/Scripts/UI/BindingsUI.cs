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

using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.Libretro.Unity
{
    public sealed class BindingsUI : MainMenuElement
    {
        private static readonly string _saveDirectory = $"{Application.streamingAssetsPath}/libretro~/bindings";
        private static readonly string _savePath      = $"{_saveDirectory}/default.json";

        protected override void OnShow() => LoadFromDisk();

        protected override void OnHide() => SaveToDisk();

        private void LoadFromDisk()
        {
            if (!File.Exists(_savePath))
                return;

            string json = File.ReadAllText(_savePath);
            if (!string.IsNullOrEmpty(json))
                _inputActions.actionMaps[0].LoadBindingOverridesFromJson(json);
        }

        private void SaveToDisk()
        {
            if (!Directory.Exists(_saveDirectory))
                _ = Directory.CreateDirectory(_saveDirectory);

            string json = _inputActions.actionMaps[0].SaveBindingOverridesAsJson();
            if (!string.IsNullOrEmpty(json))
                File.WriteAllText(_savePath, json);
        }
    }
}
