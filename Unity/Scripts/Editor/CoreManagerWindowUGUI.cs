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

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace SK.Libretro.Unity.Editor
{
    internal sealed class CoreManagerWindowUGUI : CoreManagerWindow
    {
        private Vector2 _scrollPos;

        [MenuItem("Libretro/Manage Cores")]
        public static void OpenWindow()
        {
            if (CurrentPlatform is null)
                return;

            InitializePaths();

            _ = FileSystem.GetOrCreateDirectory(_libretroDirectory);
            _ = FileSystem.GetOrCreateDirectory(_coresDirectory);

            GetWindow<CoreManagerWindowUGUI>("Manage Cores", true).minSize = new Vector2(311f, 200f);
        }

        private void OnEnable() => UpdateCoreListData();

        private void OnGUI()
        {
            GUILayout.Space(8f);
            using EditorGUILayout.ScrollViewScope scrollViewScope = new(_scrollPos, EditorStyles.helpBox);
            _scrollPos = scrollViewScope.scrollPosition;

            foreach (Core core in _coreList.Cores)
            {
                using EditorGUILayout.HorizontalScope coreHorizontalScope = new();

                GUILayout.Label(core.DisplayName, GUILayout.Width(180f));

                string buttonText;
                if (core.Processing)
                {
                    GUI.backgroundColor = Color.gray;
                    buttonText          = "Busy...";
                }
                else if(!core.Available)
                {
                    GUI.backgroundColor = Color.red;
                    buttonText          = "Download";
                }
                else if (core.Latest)
                {
                    GUI.backgroundColor = Color.green;
                    buttonText          = "Re-Download";
                }
                else
                {
                    GUI.backgroundColor = new(1.0f, 0.5f, 0f, 1f);
                    buttonText          = "Update";
                }

                GUI.enabled = !core.Processing;
                if (GUILayout.Button(new GUIContent(buttonText, null, core.DisplayName), GUILayout.Width(100f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    using CancellationTokenSource cancellationTokenSource = new();
                    ListItemButtonClickedCallback(core, cancellationTokenSource.Token).Forget();
                }
                GUI.enabled = true;

                GUI.backgroundColor = Color.white;
            }
        }
    }
}
