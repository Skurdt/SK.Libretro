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

using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SK.Libretro.Unity.Editor
{
    internal sealed class CoreManagerWindow : EditorWindow
    {
        [Serializable]
        private sealed class Core
        {
            public string FullName    = "";
            public string DisplayName = "";
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

        private static readonly string _buildbotUrl       = $"https://buildbot.libretro.com/nightly/{CurrentPlatform}/x86_64/latest/";
        private static readonly string _libretroDirectory = $"{Application.streamingAssetsPath}/libretro~";
        private static readonly string _coresDirectory    = $"{_libretroDirectory}/cores";
        private static readonly string _coresStatusFile   = $"{_libretroDirectory}/cores.json";
        private static readonly Color _greenColor         = Color.green;
        private static readonly Color _orangeColor        = new(1.0f, 0.5f, 0f, 1f);
        private static readonly Color _redColor           = Color.red;
        private static readonly Color _grayColor          = Color.gray;

        private static string CurrentPlatform => Application.platform switch
        {
            UnityEngine.RuntimePlatform.LinuxEditor   => "linux",
            UnityEngine.RuntimePlatform.OSXEditor     => "apple/osx",
            UnityEngine.RuntimePlatform.WindowsEditor => "windows",
            _                                         => InvalidPlatformDetected()
        };

        private CoreList _coreList;

        private Vector2 _scrollPos;
        private string _statusText = "";

        [MenuItem("Libretro/Manage Cores")]
        public static void ShowWindow()
        {
            if (CurrentPlatform is null)
                return;

            _ = FileSystem.GetOrCreateDirectory(_libretroDirectory);
            _ = FileSystem.GetOrCreateDirectory(_coresDirectory);

            GetCustomWindow(true).minSize = new Vector2(311f, 200f);
        }

        private void OnEnable()
        {
            _coreList = FileSystem.FileExists(_coresStatusFile) ? FileSystem.DeserializeFromJson<CoreList>(_coresStatusFile) : new();
            UpdateCoreListData();
        }

        private void OnGUI()
        {
            GUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                using EditorGUILayout.ScrollViewScope scrollView = new(_scrollPos, EditorStyles.helpBox);
                _scrollPos = scrollView.scrollPosition;

                foreach (Core core in _coreList.Cores)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(core.DisplayName, GUILayout.Width(180f));

                        string buttonText;
                        if (core.Processing)
                        {
                            GUI.backgroundColor = _grayColor;
                            buttonText          = "Busy...";
                        }
                        else if (!core.Available)
                        {
                            GUI.backgroundColor = _redColor;
                            buttonText          = "Download";
                        }
                        else if (core.Latest)
                        {
                            GUI.backgroundColor = _greenColor;
                            buttonText          = "Re-Download";
                        }
                        else
                        {
                            GUI.backgroundColor = _orangeColor;
                            buttonText          = "Update";
                        }

                        if (GUILayout.Button(new GUIContent(buttonText, null, core.DisplayName), GUILayout.Width(100f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
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

                        GUI.backgroundColor = Color.white;
                    }
                }
            }

            GUILayout.Space(8f);
            if (string.IsNullOrWhiteSpace(_statusText))
                EditorGUILayout.HelpBox("Ready", MessageType.None);
            else
                EditorGUILayout.HelpBox(_statusText, MessageType.None);
            GUILayout.Space(8f);
        }

        private static CoreManagerWindow GetCustomWindow(bool focus) => GetWindow<CoreManagerWindow>("Libretro Core Manager", focus);

        private void UpdateCoreListData()
        {
            _coreList ??= new CoreList();

            _statusText = "";

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
                        FullName = fileName,
                        DisplayName = fileName[..fileName.IndexOf("_libretro")],
                        LatestDate = lastModifiedDate,
                        Available = available
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
                core.Available   = true;

                _coreList.Cores = _coreList.Cores.OrderBy(x => x.DisplayName).ToList();
                FileSystem.SerializeToJson(_coreList, _coresStatusFile);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            context.Post(_ => GetCustomWindow(true).OnTaskFinishedOrCanceled(core), null);
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

        private void ExtractFile(string zipPath)
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

            _statusText = $"Processed {Path.GetFileNameWithoutExtension(zipPath)}";
        }

        private static string InvalidPlatformDetected()
        {
            Debug.LogError($"[LibretroManagerWindow] Invalid/Unsupported platform detected: {Application.platform}");
            return null;
        }
    }
}
