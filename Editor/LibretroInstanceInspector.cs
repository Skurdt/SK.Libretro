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

using SK.Libretro.Unity;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SK.Libretro.UnityEditor
{
    [CustomEditor(typeof(LibretroInstance), true)]
    public sealed class LibretroInstanceInspector : Editor
    {
        private SerializedProperty _rendererProperty;
        private SerializedProperty _viewerProperty;
        private SerializedProperty _mainDirectoryProperty;
        private SerializedProperty _coreNameProperty;
        private SerializedProperty _gameDirectoryProperty;
        private SerializedProperty _gamesProperty;
        private SerializedProperty _editorGLProperty;

        private void OnEnable()
        {
            _rendererProperty      = serializedObject.FindProperty("_renderer");
            _viewerProperty        = serializedObject.FindProperty("_viewer");
            _mainDirectoryProperty = serializedObject.FindProperty("_settings").FindPropertyRelative("MainDirectory");
            _coreNameProperty      = serializedObject.FindProperty("_coreName");
            _gameDirectoryProperty = serializedObject.FindProperty("_gameDirectory");
            _gamesProperty         = serializedObject.FindProperty("_gameNames");
            _editorGLProperty      = serializedObject.FindProperty("_allowGLCoresInEditor");

            EditorGUI.FocusTextInControl(null);
        }

        public override void OnInspectorGUI()
        {
            _ = EditorGUILayout.PropertyField(_rendererProperty);
            _ = EditorGUILayout.PropertyField(_viewerProperty);
            _ = EditorGUILayout.PropertyField(_editorGLProperty);

            GUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Core", GUILayout.Width(100f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    ShowSelectCoreWindow();
                _coreNameProperty.stringValue = EditorGUILayout.TextField(_coreNameProperty.stringValue);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Directory", GUILayout.Width(100f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    ShowSelectRomDirectoryDialog();
                _gameDirectoryProperty.stringValue = EditorGUILayout.TextField(_gameDirectoryProperty.stringValue);
            }

            GUILayout.Space(8f);
            ShowGames();
            if (GUILayout.Button("+", GUILayout.Width(100f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                ++_gamesProperty.arraySize;
                ShowSelectRomDialog(_gamesProperty.arraySize - 1);
            }

            GUILayout.Space(8f);
            if (string.IsNullOrEmpty(_coreNameProperty.stringValue))
                EditorGUILayout.HelpBox("No core selected", MessageType.Error);

            _ = serializedObject.ApplyModifiedProperties();
        }

        private void ShowGames()
        {
            GUILayout.Label("Roms:");
            if (_gamesProperty.arraySize == 0)
            {
                ++_gamesProperty.arraySize;
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button($"Rom_0", GUILayout.Width(100f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        ShowSelectRomDialog(0);

                    SerializedProperty gameNameProperty = _gamesProperty.GetArrayElementAtIndex(0);
                    gameNameProperty.stringValue = EditorGUILayout.TextField("");
                }
                return;
            }

            for (int i = 0; i < _gamesProperty.arraySize; ++i)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button($"Rom_{i}", GUILayout.Width(100f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        ShowSelectRomDialog(i);

                    SerializedProperty gameNameProperty = _gamesProperty.GetArrayElementAtIndex(i);
                    gameNameProperty.stringValue = EditorGUILayout.TextField(gameNameProperty.stringValue);

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(25f)))
                    {
                        EditorGUI.FocusTextInControl(null);
                        if (_gamesProperty.arraySize == 0)
                            gameNameProperty.stringValue = "";
                        else
                            _gamesProperty.DeleteArrayElementAtIndex(i);
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
        }

        private void ShowSelectCoreWindow()
        {
            string coresDirectory = !string.IsNullOrEmpty(_mainDirectoryProperty.stringValue)
                                  ? $"{_mainDirectoryProperty.stringValue}/cores"
                                  : $"{Application.streamingAssetsPath}/libretro~/cores";

            if (!Directory.Exists(coresDirectory))
            {
                Debug.LogError($"[LibretroInstanceInspector] Cores directory not found: {coresDirectory}");
                return;
            }

            LibretroCoreListWindow.ShowWindow(coresDirectory, (string coreName) =>
            {
                if (!string.IsNullOrEmpty(coreName))
                {
                    _coreNameProperty.stringValue = coreName;
                    _ = serializedObject.ApplyModifiedProperties();
                }
            });
        }

        private void ShowSelectRomDirectoryDialog()
        {
            string startingDirectory = !string.IsNullOrEmpty(_gameDirectoryProperty.stringValue) ? _gameDirectoryProperty.stringValue : "";
            string directory         = EditorUtility.OpenFolderPanel("Select rom directory", startingDirectory, startingDirectory);
            if (!string.IsNullOrEmpty(directory))
                _gameDirectoryProperty.stringValue = directory.Replace(Path.DirectorySeparatorChar, '/');
        }

        private void ShowSelectRomDialog(int romIndex)
        {
            string startingDirectory = !string.IsNullOrEmpty(_gameDirectoryProperty.stringValue) ? _gameDirectoryProperty.stringValue : "";
            string filePath          = EditorUtility.OpenFilePanel("Select rom", startingDirectory, "");
            if (!string.IsNullOrEmpty(filePath))
            {
                _gameDirectoryProperty.stringValue = Path.GetDirectoryName(filePath).Replace(Path.DirectorySeparatorChar, '/');
                _gamesProperty.GetArrayElementAtIndex(romIndex).stringValue = Path.GetFileNameWithoutExtension(filePath);
            }
        }
    }
}
