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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SK.Libretro.Unity.Editor
{
    internal sealed class CoreListWindow : EditorWindow
    {
        private static System.Action<string> _coreSelectedCallback;
        private static IEnumerable<string> _coreNames;
        private Vector2 _scrollPos;
        private string _searchQuery = "";

        public static void ShowWindow(string coresDirectory, System.Action<string> coreSelectedCallback)
        {
            UnityEngine.RuntimePlatform runtimePlatform;
            try
            {
                runtimePlatform = GetCurrentPlatform();
            }
            catch (System.NotSupportedException e)
            {
                Debug.LogError(e.Message);
                return;
            }

            if (!Directory.Exists(coresDirectory))
                return;

            _coreSelectedCallback = coreSelectedCallback;

            string filter = runtimePlatform switch
            {
                UnityEngine.RuntimePlatform.OSXEditor     => "*.dylib",
                UnityEngine.RuntimePlatform.LinuxEditor   => "*.so",
                UnityEngine.RuntimePlatform.WindowsEditor => "*.dll",
                _ => "*.*"
            };

            _coreNames = Directory.EnumerateFiles(coresDirectory, filter, SearchOption.TopDirectoryOnly)
                                  .Select(x =>  Path.GetFileNameWithoutExtension(x).Replace("_libretro", ""));
            GetWindow<CoreListWindow>("Available Cores").minSize = new Vector2(311f, 200f);
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _searchQuery = EditorGUILayout.TextField("Search", _searchQuery);
                if (GUILayout.Button("x", GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    EditorGUI.FocusTextInControl(null);
                    _searchQuery = "";
                }
            }

            IEnumerable<string> filteredCoreNames = _coreNames;
            if (!string.IsNullOrWhiteSpace(_searchQuery))
                filteredCoreNames = _coreNames.Where(x => x.Contains(_searchQuery));

            using EditorGUILayout.ScrollViewScope scrollView = new(_scrollPos, EditorStyles.helpBox);
            _scrollPos = scrollView.scrollPosition;

            foreach (string coreName in filteredCoreNames)
            {
                if (GUILayout.Button(coreName))
                {
                    _coreSelectedCallback?.Invoke(coreName);
                    _coreSelectedCallback = null;
                    GetWindow<CoreListWindow>().Close();
                }
            }
        }

        private static UnityEngine.RuntimePlatform GetCurrentPlatform()
        {
            switch (Application.platform)
            {
                case UnityEngine.RuntimePlatform.LinuxEditor:
                case UnityEngine.RuntimePlatform.OSXEditor:
                case UnityEngine.RuntimePlatform.WindowsEditor:
                    return Application.platform;
                default:
                {
                    throw new System.NotSupportedException($"[LibretroCoreListWindow] Invalid/Unsupported platform detected: {Application.platform}");
                }
            }
        }
    }
}
