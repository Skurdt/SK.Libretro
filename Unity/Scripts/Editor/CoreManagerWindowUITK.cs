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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SK.Libretro.Unity.Editor
{
    internal sealed class CoreManagerWindowUITK : CoreManagerWindow
    {
        [SerializeField] private VisualTreeAsset _mainVisualTreeAsset;
        [SerializeField] private VisualTreeAsset _listItemVisualTreeAsset;

        private static readonly string _infoDirectory = $"{_libretroDirectory}/info";
        
        private ListView _coreListView;

        [MenuItem("Libretro/Manage Cores (UITK)")]
        public static void OpenWindow()
        {
            if (CurrentPlatform is null)
                return;

            _ = FileSystem.GetOrCreateDirectory(_libretroDirectory);
            _ = FileSystem.GetOrCreateDirectory(_coresDirectory);
            _ = FileSystem.GetOrCreateDirectory(_infoDirectory);

            GetWindow<CoreManagerWindowUITK>("Manage Cores", true).minSize = new(640f, 480f);
        }

        public void CreateGUI()
        {
            UpdateCoreListData();

            _mainVisualTreeAsset.CloneTree(rootVisualElement);

            TextField searchField = rootVisualElement.Q<TextField>("SearchField");
            _ = searchField.RegisterValueChangedCallback(evnt => {
                _coreListDisplay = !string.IsNullOrWhiteSpace(evnt.newValue)
                                 ? _coreList.Cores.Where(x => x.DisplayName.Contains(evnt.newValue, StringComparison.OrdinalIgnoreCase))
                                                  .ToList()
                                 : _coreList.Cores;

                _coreListView.ClearSelection();
                _coreListView.Clear();
                _coreListView.itemsSource = _coreListDisplay;
                _coreListView.RefreshItems();
            });

            _coreListView = rootVisualElement.Q<ListView>();

            _coreListView.itemsSource = _coreListDisplay;
            _coreListView.makeItem = () =>
            {
                VisualElement item = new();
                _listItemVisualTreeAsset.CloneTree(item);
                VisualElement root = item.Q("ItemRoot");
                Button button = root.Q<Button>();
                button.clickable = null;
                button.AddToClassList("core-list-item-processing");
                button.AddToClassList("core-list-item-not-available");
                button.AddToClassList("core-list-item-latest");
                button.AddToClassList("core-list-item-older");
                return item;
            };
            _coreListView.bindItem = (VisualElement item, int index) =>
            {
                Core core = _coreListDisplay[index];

                VisualElement root = item.Q("ItemRoot");
                Button button = root.Q<Button>();
                button.clickable = null;
                button.EnableInClassList("core-list-item-processing", false);
                button.EnableInClassList("core-list-item-not-available", false);
                button.EnableInClassList("core-list-item-latest", false);
                button.EnableInClassList("core-list-item-older", false);
                if (!core.Available)
                {
                    button.ToggleInClassList("core-list-item-not-available");
                    button.text = "Download";
                }
                else if (core.Latest)
                {
                    button.ToggleInClassList("core-list-item-latest");
                    button.text = "Re-Download";
                }
                else
                {
                    button.ToggleInClassList("core-list-item-older");
                    button.text = "Update";
                }
                button.clicked += async () =>
                {
                    button.EnableInClassList("core-list-item-not-available", false);
                    button.EnableInClassList("core-list-item-latest", false);
                    button.EnableInClassList("core-list-item-older", false);
                    button.ToggleInClassList("core-list-item-processing");
                    button.text = "Busy...";
                    button.SetEnabled(false);
                    using CancellationTokenSource cancellationTokenSource = new();
                    await ListItemButtonClickedCallback(core, cancellationTokenSource.Token);
                    _coreListView.RefreshItems();
                    button.SetEnabled(true);
                };

                Label label = root.Q<Label>();
                label.text = core.DisplayName;
            };

            Label infoLabel = rootVisualElement.Q<Label>("InfoText");
            _coreListView.onSelectionChange += async (items) =>
            {
                if (EditorPrefs.GetInt("CoreManagerWindowSelectedIndex") == _coreListView.selectedIndex)
                    return;

                infoLabel.text = "";

                if (_coreListView.selectedIndex == -1)
                    return;

                EditorPrefs.SetInt("CoreManagerWindowSelectedIndex", _coreListView.selectedIndex);

                Core core = items.First() as Core;
                string infoFilePath = $"{_infoDirectory}/{core.DisplayName}_libretro.info";
                if (!FileSystem.FileExists(infoFilePath))
                    return;

                using StreamReader stream = new(infoFilePath);
                StringBuilder stringBuilder = new();
                string line;
                while ((line = await stream.ReadLineAsync()) is not null)
                {
                    if (line.StartsWith('#'))
                    {
                        _ = stringBuilder.Append($"<size=18><b>\n{line.Trim()}:</b></size>\n\n");
                        continue;
                    }
                    
                    if (line.Contains('='))
                    {
                        int equalSignIndex = line.IndexOf('=');
                        string propertyValue = line[(equalSignIndex + 1)..].Trim().Trim('\"');
                        if (string.IsNullOrWhiteSpace(propertyValue))
                            continue;

                        string propertyName = line[..equalSignIndex].Trim();
                        _ = stringBuilder.Append($"<size=14><b>{propertyName}: </b></size>");
                        _ = stringBuilder.Append($"<size=12>{propertyValue}</size>\n");
                    }
                }

                infoLabel.text = stringBuilder.ToString();
            };

            _coreListView.SetSelection(EditorPrefs.GetInt("CoreManagerWindowSelectedIndex", -1));
        }
    }
}
