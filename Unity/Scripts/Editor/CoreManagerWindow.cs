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
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SK.Libretro.Unity.Editor
{
    internal sealed class CoreManagerWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _mainVisualTreeAsset;
        [SerializeField] private VisualTreeAsset _listItemVisualTreeAsset;

        [Serializable]
        private sealed class Core
        {
            public string FullName    = "";
            public string DisplayName = "";
            public DateTime CurrentDate;
            public DateTime LatestDate;

            [JsonIgnore] public bool Processing = false;
            [JsonIgnore] public bool Available  = false;
            [JsonIgnore] public bool Latest => CurrentDate == LatestDate;
        }

        [Serializable]
        private sealed class CoreList
        {
            public List<Core> Cores = new();
        }

        private string CurrentPlatform => Application.platform switch
        {
            UnityEngine.RuntimePlatform.LinuxEditor   => "linux",
            UnityEngine.RuntimePlatform.OSXEditor     => "apple/osx",
            UnityEngine.RuntimePlatform.WindowsEditor => "windows",
            _                                         => InvalidPlatformDetected()
        };

        private string _buildbotUrl;
        private string _libretroDirectory;
        private string _infoDirectory;
        private string _coresDirectory;
        private string _coresStatusFile;

        private CoreList _coreList;
        private List<Core> _coreListDisplay;

        private ListView _coreListView;
        private Label _infoLabel;

        [MenuItem("Libretro/Manage Cores")]
        public static void OpenWindow() => GetWindow<CoreManagerWindow>("Manage Cores", true).minSize = new(640f, 480f);

        public void CreateGUI()
        {
            if (CurrentPlatform is null)
                return;

            _buildbotUrl       = $"https://buildbot.libretro.com/nightly/{CurrentPlatform}/x86_64/latest/";
            _libretroDirectory = $"{Application.persistentDataPath}/Libretro";
            _infoDirectory     = $"{_libretroDirectory}/info";
            _coresDirectory    = $"{_libretroDirectory}/cores";
            _coresStatusFile   = $"{_libretroDirectory}/cores.json";

            _ = FileSystem.GetOrCreateDirectory(_libretroDirectory);
            _ = FileSystem.GetOrCreateDirectory(_coresDirectory);
            _ = FileSystem.GetOrCreateDirectory(_infoDirectory);

            UpdateCoreListData();

            _mainVisualTreeAsset.CloneTree(rootVisualElement);

            SetupToolbar();
            SetupInfoPanel();
            SetupCoreListView();
        }

        private void SetupToolbar()
        {
            ToolbarSearchField searchField = rootVisualElement.Q<ToolbarSearchField>("ToolbarSearchField");
            _ = searchField.RegisterValueChangedCallback(ToolbarSearchFieldValueChangedCallback);

            ToolbarButton downloadInfoFilesButton = rootVisualElement.Q<ToolbarButton>("ToolbarButton");
            downloadInfoFilesButton.clickable = null;
            downloadInfoFilesButton.clicked += async () => await ToolbarDownloadInfoFilesButtonClickedCallback(downloadInfoFilesButton);
        }

        private void ToolbarSearchFieldValueChangedCallback(ChangeEvent<string> evnt)
        {
            _coreListDisplay = !string.IsNullOrWhiteSpace(evnt.newValue)
                             ? _coreList.Cores.Where(x => x.DisplayName.Contains(evnt.newValue, StringComparison.OrdinalIgnoreCase))
                                              .ToList()
                             : _coreList.Cores;

            _coreListView.itemsSource = _coreListDisplay;
            _coreListView.Rebuild();
        }

        private async UniTask ToolbarDownloadInfoFilesButtonClickedCallback(ToolbarButton downloadInfoFilesButton)
        {
            downloadInfoFilesButton.SetEnabled(false);
            using CancellationTokenSource tokenSource = new();
            string infoRepositoryPath = await DownloadFile("https://github.com/libretro/libretro-core-info/archive/refs/heads/master.zip", _infoDirectory, tokenSource.Token);
            ExtractFile(infoRepositoryPath, _infoDirectory);
            FileSystem.DeleteFile(infoRepositoryPath);

            string[] infoFiles = FileSystem.GetFilesInDirectory($"{_infoDirectory}/libretro-core-info-master", "*.info");
            foreach (string infoFile in infoFiles)
            {
                FileSystem.MoveFile(infoFile, $"{_infoDirectory}/{Path.GetFileName(infoFile)}", true);
            }
            Directory.Delete($"{_infoDirectory}/libretro-core-info-master", true);
            downloadInfoFilesButton.SetEnabled(true);
        }

        private void SetupCoreListView()
        {
            _coreListView = rootVisualElement.Q<ListView>();
            _coreListView.makeItem = CoreListViewMakeItemCallback;
            _coreListView.bindItem = CoreListViewBindItemCallback;
            _coreListView.itemsSource = _coreListDisplay;
            _coreListView.selectionChanged += CoreListViewOnSelectionChangeCallback;
            _coreListView.SetSelection(EditorPrefs.GetInt("CoreManagerWindowSelectedIndex", -1));
        }

        private VisualElement CoreListViewMakeItemCallback()
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
        }

        private void CoreListViewBindItemCallback(VisualElement item, int index)
        {
            Core core = _coreListDisplay[index];

            VisualElement root = item.Q("ItemRoot");

            Button button = root.Q<Button>();
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
            button.clickable = null;
            button.clicked += async () => await CoreListItemButtonClickedCallback(core, button);

            Label label = root.Q<Label>();
            label.text = core.DisplayName;
        }

        private async UniTask CoreListItemButtonClickedCallback(Core core, Button button)
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
        }

        private async void CoreListViewOnSelectionChangeCallback(IEnumerable<object> items)
        {
            _infoLabel.text = "";

            EditorPrefs.SetInt("CoreManagerWindowSelectedIndex", _coreListView.selectedIndex);

            if (_coreListView.selectedIndex == -1)
                return;

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

            _infoLabel.text = stringBuilder.ToString();
        }

        private void SetupInfoPanel() => _infoLabel = rootVisualElement.Q<Label>("InfoText");

        private void UpdateCoreListData()
        {
            _coreList = FileSystem.FileExists(_coresStatusFile) ? FileSystem.DeserializeFromJson<CoreList>(_coresStatusFile) : new();

            HtmlWeb hw = new();
            HtmlDocument doc = hw.Load(new Uri(_buildbotUrl));
            HtmlNodeCollection trNodes = doc.DocumentNode.SelectNodes("//body/div/table/tr");

            foreach (HtmlNode trNode in trNodes)
            {
                HtmlNodeCollection tdNodes = trNode.ChildNodes;
                if (tdNodes.Count < 3)
                    continue;

                string fileName = tdNodes[1].InnerText;
                if (!fileName.Contains("_libretro"))
                    continue;

                string lastModifiedString = tdNodes[2].InnerText;
                _ = DateTime.TryParse(lastModifiedString, out DateTime lastModifiedDate);
                bool available = FileSystem.FileExists($"{_coresDirectory}/{fileName.Replace(".zip", "", StringComparison.OrdinalIgnoreCase)}");
                Core found = _coreList.Cores.Find(x => x.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                if (found is null)
                {
                    _coreList.Cores.Add(new Core
                    {
                        FullName    = fileName,
                        DisplayName = fileName[..fileName.IndexOf("_libretro")],
                        LatestDate  = lastModifiedDate,
                        Available   = available
                    });
                }
                else
                {
                    found.LatestDate = lastModifiedDate;
                    found.Available = available;
                }
            }

            _coreList.Cores = _coreList.Cores.OrderBy(x => x.DisplayName).ToList();
            FileSystem.SerializeToJson(_coreList, _coresStatusFile);

            _coreListDisplay = _coreList.Cores;
        }

        private async UniTask ListItemButtonClickedCallback(Core core, CancellationToken cancellationToken)
        {
            core.Processing = true;
            await DownloadAndExtractTask(core, cancellationToken);
            core.Processing = false;
        }

        private async UniTask DownloadAndExtractTask(Core core, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                string zipPath = await DownloadFile($"{_buildbotUrl}{core.FullName}", _coresDirectory, cancellationToken);
                ExtractFile(zipPath, _coresDirectory);
                await UniTask.Delay(500, delayType: DelayType.Realtime, cancellationToken: cancellationToken);
                FileSystem.DeleteFile(zipPath);

                core.CurrentDate = core.LatestDate;
                core.Available   = true;

                _coreList.Cores = _coreList.Cores.OrderBy(x => x.DisplayName).ToList();
                FileSystem.SerializeToJson(_coreList, _coresStatusFile);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Debug.LogException(e);
            }
        }

        private static async UniTask<string> DownloadFile(string url, string directory, CancellationToken cancellationToken)
        {
            try
            {
                string fileName = Path.GetFileName(url);
                string filePath = $"{directory}/{fileName}";
                FileSystem.DeleteFile(filePath);
                await UniTask.Delay(200, delayType: DelayType.Realtime, cancellationToken: cancellationToken);

                using WebClient webClient = new();
                await webClient.DownloadFileTaskAsync(url, filePath);
                return filePath;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private static void ExtractFile(string zipPath, string directory)
        {
            try
            {
                if (!FileSystem.FileExists(zipPath))
                    return;

                using ZipArchive archive = ZipFile.OpenRead(zipPath);
                archive.ExtractToDirectory(directory, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static string InvalidPlatformDetected()
        {
            Debug.LogError($"[LibretroManagerWindow] Invalid/Unsupported platform detected: {Application.platform}");
            return null;
        }
    }
}
