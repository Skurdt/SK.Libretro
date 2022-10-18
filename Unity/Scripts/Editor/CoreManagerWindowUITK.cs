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
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SK.Libretro.Unity.Editor
{
    internal sealed class CoreManagerWindowUITK : EditorWindow
    {
        [Serializable]
        private sealed class Core
        {
            public string FullName;
            public string DisplayName;
            public DateTime CurrentDate;
            public DateTime LatestDate;

            [JsonIgnore] public bool Available = false;
            [JsonIgnore] public bool Latest => CurrentDate == LatestDate;
            [JsonIgnore] public bool Processing { get; set; } = false;
            [JsonIgnore] public bool TaskRunning { get; set; } = false;
            [JsonIgnore] public CancellationTokenSource CancellationTokenSource { get; set; } = null;
        }

        [Serializable]
        private sealed class CoreList
        {
            public List<Core> Cores = new();
        }

        [SerializeField] private VisualTreeAsset _mainVisualTreeAsset;
        [SerializeField] private VisualTreeAsset _listItemVisualTreeAsset;

        private static readonly string _buildbotUrl       = $"https://buildbot.libretro.com/nightly/{CurrentPlatform}/x86_64/latest/";
        private static readonly string _libretroDirectory = $"{Application.streamingAssetsPath}/libretro~";
        private static readonly string _coresDirectory    = $"{_libretroDirectory}/cores";
        private static readonly string _infoDirectory     = $"{_libretroDirectory}/info";
        private static readonly string _coresStatusFile   = $"{_libretroDirectory}/cores.json";
        
        private static string CurrentPlatform => Application.platform switch
        {
            UnityEngine.RuntimePlatform.LinuxEditor   => "linux",
            UnityEngine.RuntimePlatform.OSXEditor     => "apple/osx",
            UnityEngine.RuntimePlatform.WindowsEditor => "windows",
            _                                         => InvalidPlatformDetected()
        };

        private CoreList _coreList;
        private List<Core> _coreListDisplay;

        private ListView _coreListView;

        [MenuItem("Libretro/Manage Cores (UITK)")]
        public static void OpenWindow() => GetCustomWindow().minSize = new(640f, 480f);

        public void CreateGUI()
        {
            if (CurrentPlatform is null)
                return;

            _ = FileSystem.GetOrCreateDirectory(_libretroDirectory);
            _ = FileSystem.GetOrCreateDirectory(_coresDirectory);
            _ = FileSystem.GetOrCreateDirectory(_infoDirectory);

            UpdateCoreList();

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
                button.EnableInClassList("core-list-item-processing", false);
                button.EnableInClassList("core-list-item-not-available", false);
                button.EnableInClassList("core-list-item-latest", false);
                button.EnableInClassList("core-list-item-older", false);
                if (core.Processing)
                {
                    button.ToggleInClassList("core-list-item-processing");
                    button.text = "Busy...";
                }
                else if (!core.Available)
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
                button.clicked += () => ListItemButtonClickedCallback(core);

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

        private static CoreManagerWindowUITK GetCustomWindow() => GetWindow<CoreManagerWindowUITK>("Manage Cores", true);

        private void UpdateCoreList()
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
                    found.Available  = available;
                }
            }

            _coreList.Cores = _coreList.Cores.OrderBy(x => x.DisplayName).ToList();
            FileSystem.SerializeToJson(_coreList, _coresStatusFile);

            _coreListDisplay = _coreList.Cores;
        }

        private void ListItemButtonClickedCallback(Core core)
        {
            core.TaskRunning = true;

            core.CancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = core.CancellationTokenSource.Token;
            SynchronizationContext context = SynchronizationContext.Current;
            _ = Task.Run(() => DownloadAndExtractTask(core, context, token), token)
                    .ContinueWith(t =>
                    {
                        HandleTaskException(t);
                        OnTaskFinishedOrCanceled(core);
                    },
                    token,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DownloadAndExtractTask(Core core, SynchronizationContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                core.Processing = true;

                string zipPath = DownloadFile($"{_buildbotUrl}{core.FullName}");
                ExtractFile(zipPath);

                core.CurrentDate = core.LatestDate;
                core.Available = true;

                _coreList.Cores = _coreList.Cores.OrderBy(x => x.DisplayName).ToList();
                FileSystem.SerializeToJson(_coreList, _coresStatusFile);
            }
            catch
            {
            }

            context.Post(_ => OnTaskFinishedOrCanceled(core), null);
        }

        private static void HandleTaskException(Task task)
        {
            if (!task.IsFaulted)
                return;

            Exception taskException = task.Exception;
            while (taskException is AggregateException && taskException.InnerException is not null)
                taskException = taskException.InnerException;
            _ = EditorUtility.DisplayDialog("Task chain terminated", $"Exception: {taskException.Message}", "Ok");
        }

        private void OnTaskFinishedOrCanceled(Core core)
        {
            core.TaskRunning = false;
            core.CancellationTokenSource?.Dispose();
            core.CancellationTokenSource = null;
            core.Processing = false;
            _coreListView.RefreshItems();
        }

        private static string DownloadFile(string url)
        {
            using WebClient webClient = new();
            string fileName = Path.GetFileName(url);
            string filePath = $"{_coresDirectory}/{fileName}";
            FileSystem.DeleteFile(filePath);
            webClient.DownloadFile(url, filePath);
            return filePath;
        }

        private static void ExtractFile(string zipPath)
        {
            if (!FileSystem.FileExists(zipPath))
                return;

            using ZipArchive archive = ZipFile.OpenRead(zipPath);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string destinationPath = $"{_coresDirectory}/{entry.FullName}";
                FileSystem.DeleteFile(destinationPath);
                entry.ExtractToFile(destinationPath);
            }

            FileSystem.DeleteFile(zipPath);
        }

        private static string InvalidPlatformDetected()
        {
            Debug.LogError($"[LibretroManagerWindow] Invalid/Unsupported platform detected: {Application.platform}");
            return null;
        }
    }
}
