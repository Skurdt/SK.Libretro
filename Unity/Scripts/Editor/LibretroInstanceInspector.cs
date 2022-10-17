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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace SK.Libretro.Unity.Editor
{
    [CustomEditor(typeof(LibretroInstance)), CanEditMultipleObjects]
    internal sealed class LibretroInstanceInspector : UnityEditor.Editor
    {
        private SerializedProperty _useSeparateThreadProperty;
        private SerializedProperty _cameraProperty;
        private SerializedProperty _raycastLayerProperty;
        private SerializedProperty _rendererProperty;
        private SerializedProperty _colliderProperty;
        private SerializedProperty _viewerProperty;
        private SerializedProperty _settingsProperty;
        private SerializedProperty _mainDirectoryProperty;
        private SerializedProperty _editorGLProperty;
        private SerializedProperty _coreNameProperty;
        private SerializedProperty _gameDirectoryProperty;
        private SerializedProperty _gamesProperty;

        private void OnEnable()
        {
            _useSeparateThreadProperty = serializedObject.FindProperty($"<{nameof(LibretroInstance.UseSeparateThread)}>k__BackingField");
            _cameraProperty            = serializedObject.FindProperty($"<{nameof(LibretroInstance.Camera)}>k__BackingField");
            _raycastLayerProperty      = serializedObject.FindProperty($"<{nameof(LibretroInstance.LightgunRaycastLayer)}>k__BackingField");
            _rendererProperty          = serializedObject.FindProperty($"<{nameof(LibretroInstance.Renderer)}>k__BackingField");
            _colliderProperty          = serializedObject.FindProperty($"<{nameof(LibretroInstance.Collider)}>k__BackingField");
            _viewerProperty            = serializedObject.FindProperty($"<{nameof(LibretroInstance.Viewer)}>k__BackingField");
            _settingsProperty          = serializedObject.FindProperty($"<{nameof(LibretroInstance.Settings)}>k__BackingField");
            _mainDirectoryProperty     = _settingsProperty.FindPropertyRelative(nameof(LibretroInstance.Settings.MainDirectory));
            _editorGLProperty          = serializedObject.FindProperty($"<{nameof(LibretroInstance.AllowGLCoreInEditor)}>k__BackingField");
            _coreNameProperty          = serializedObject.FindProperty($"<{nameof(LibretroInstance.CoreName)}>k__BackingField");
            _gameDirectoryProperty     = serializedObject.FindProperty($"<{nameof(LibretroInstance.GamesDirectory)}>k__BackingField");
            _gamesProperty             = serializedObject.FindProperty($"<{nameof(LibretroInstance.GameNames)}>k__BackingField");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _ = EditorGUILayout.PropertyField(_useSeparateThreadProperty);
            _ = EditorGUILayout.PropertyField(_cameraProperty);
            _ = EditorGUILayout.PropertyField(_raycastLayerProperty);
            _ = EditorGUILayout.PropertyField(_rendererProperty);
            _ = EditorGUILayout.PropertyField(_colliderProperty);
            _ = EditorGUILayout.PropertyField(_viewerProperty);

            GUILayout.Space(8f);
            _ = EditorGUILayout.PropertyField(_settingsProperty);

            GUILayout.Space(8f);
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
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+", GUILayout.Width(48f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    ++_gamesProperty.arraySize;
                    _gamesProperty.GetArrayElementAtIndex(_gamesProperty.arraySize - 1).stringValue = "";
                }
                if (GUILayout.Button("...", GUILayout.Width(48f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    ++_gamesProperty.arraySize;
                    ShowSelectRomDialog(_gamesProperty.arraySize - 1);
                }
            }

            GUILayout.Space(8f);
            if (string.IsNullOrWhiteSpace(_coreNameProperty.stringValue))
                EditorGUILayout.HelpBox("No core selected", MessageType.Error);

            _ = serializedObject.ApplyModifiedProperties();
        }

        private void ShowGames()
        {
            GUILayout.Label("Roms:");
            _gamesProperty.arraySize = Mathf.Max(1, _gamesProperty.arraySize);
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
            string coresDirectory = !string.IsNullOrWhiteSpace(_mainDirectoryProperty.stringValue)
                                  ? $"{_mainDirectoryProperty.stringValue}/cores"
                                  : $"{Application.streamingAssetsPath}/libretro~/cores";

            if (!Directory.Exists(coresDirectory))
            {
                Debug.LogError($"[LibretroInstanceInspector] Cores directory not found: {coresDirectory}");
                return;
            }

            CoreListWindow.ShowWindow(coresDirectory, (string coreName) =>
            {
                if (!string.IsNullOrWhiteSpace(coreName))
                {
                    EditorGUI.FocusTextInControl(null);
                    _coreNameProperty.stringValue = coreName;
                    _ = serializedObject.ApplyModifiedProperties();
                }
            });
        }

        private void ShowSelectRomDirectoryDialog()
        {
            string startingDirectory = !string.IsNullOrWhiteSpace(_gameDirectoryProperty.stringValue) ? _gameDirectoryProperty.stringValue : "";
            string directory         = EditorUtility.OpenFolderPanel("Select rom directory", startingDirectory, startingDirectory);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                EditorGUI.FocusTextInControl(null);
                _gameDirectoryProperty.stringValue = directory.Replace(Path.DirectorySeparatorChar, '/');
            }
        }

        private void ShowSelectRomDialog(int romIndex)
        {
            string startingDirectory = !string.IsNullOrWhiteSpace(_gameDirectoryProperty.stringValue) ? _gameDirectoryProperty.stringValue : "";
            string filePath          = EditorUtility.OpenFilePanel("Select rom", startingDirectory, "");
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                EditorGUI.FocusTextInControl(null);
                _gameDirectoryProperty.stringValue = Path.GetDirectoryName(filePath).Replace(Path.DirectorySeparatorChar, '/');
                _gamesProperty.GetArrayElementAtIndex(romIndex).stringValue = Path.GetFileNameWithoutExtension(filePath);
            }
        }
    }
}
