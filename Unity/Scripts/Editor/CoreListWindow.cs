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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SK.Libretro.Unity.Editor
{
    internal sealed class CoreListWindow : EditorWindow
    {
        private static string _coresDirectory = "";
        private static Action<string> _coreSelectedCallback;

        private IEnumerable<string> _coreNames  = Array.Empty<string>();
        private List<string> _filteredCoreNames = new();
        private ListView _listView;

        public static void ShowWindow(string coresDirectory, Action<string> coreSelectedCallback)
        {
            if (!Directory.Exists(coresDirectory))
                return;

            _coresDirectory       = coresDirectory;
            _coreSelectedCallback = coreSelectedCallback;

            GetWindow<CoreListWindow>("Available Cores").minSize = new Vector2(311f, 200f);
        }

        private void CreateGUI()
        {
            Toolbar toolbar = new();
            toolbar.style.marginTop    = 4f;
            toolbar.style.marginBottom = 4f;
            ToolbarSearchField searchField = new();
            searchField.style.flexShrink = 1f;
            searchField.style.flexGrow   = 1f;
            toolbar.Add(searchField);
            rootVisualElement.Add(toolbar);

            if (string.IsNullOrWhiteSpace(_coresDirectory))
                return;

            _ = searchField.RegisterValueChangedCallback(SearchFieldValueChangedCallback);

            string filter = Application.platform switch
            {
                UnityEngine.RuntimePlatform.OSXEditor     => "*.dylib",
                UnityEngine.RuntimePlatform.LinuxEditor   => "*.so",
                UnityEngine.RuntimePlatform.WindowsEditor => "*.dll",
                _ => "*.*"
            };

            _coreNames = Directory.EnumerateFiles(_coresDirectory, filter, SearchOption.TopDirectoryOnly)
                                  .Select(x => Path.GetFileNameWithoutExtension(x).Replace("_libretro", ""));
            _filteredCoreNames = _coreNames.ToList();

            _listView = new()
            {
                makeItem        = ListViewMakeItemCallback,
                bindItem        = ListViewBindItemCallback,
                itemsSource     = _filteredCoreNames,
                fixedItemHeight = 22f
            };
            _listView.style.flexGrow = 1f;
            rootVisualElement.Add(_listView);
        }

        private void SearchFieldValueChangedCallback(ChangeEvent<string> evnt)
        {
            _filteredCoreNames = string.IsNullOrWhiteSpace(evnt.newValue)
                               ? _coreNames.ToList()
                               : _coreNames.Where(x => x.Contains(evnt.newValue, StringComparison.OrdinalIgnoreCase)).ToList();
            _listView.itemsSource = _filteredCoreNames;
            _listView.Rebuild();
        }

        private VisualElement ListViewMakeItemCallback() => new Button();

        private void ListViewBindItemCallback(VisualElement element, int index)
        {
            Button button = element as Button;
            button.text = _filteredCoreNames[index];
            button.clickable = null;
            button.clicked += () => {
                _coreSelectedCallback?.Invoke(button.text);
                _coreSelectedCallback = null;
                GetWindow<CoreListWindow>().Close();
            };
        }
    }
}
