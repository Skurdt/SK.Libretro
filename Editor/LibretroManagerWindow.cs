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

using HtmlAgilityPack;
using SK.Libretro.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SK.Libretro.UnityEditor
{
    public sealed class LibretroManagerWindow : EditorWindow
    {
        [Serializable]
        private sealed class Core
        {
            public bool Latest => CurrentDate.Equals(LatestDate, StringComparison.OrdinalIgnoreCase);
            public bool Processing { get; set; } = false;
            public bool TaskRunning { get; set; } = false;
            public CancellationTokenSource CancellationTokenSource { get; set; } = null;

            public string FullName    = "";
            public string DisplayName = "";
            public string CurrentDate = "";
            public string LatestDate  = "";
            public bool Available     = false;
        }

        [Serializable]
        private sealed class CoreList
        {
            public List<Core> Cores = new List<Core>();
        }

        private static string CurrentPlatform
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxEditor:
                        return "linux";
                    case RuntimePlatform.OSXEditor:
                        return "apple/osx";
                    case RuntimePlatform.WindowsEditor:
                        return "windows";
                    default:
                    {
                        Debug.LogError($"[LibretroManagerWindow] Invalid/unsupported platform detected: {Application.platform}");
                        return null;
                    }
                }
            }
        }

        private static readonly string _buildbotUrl       = $"https://buildbot.libretro.com/nightly/{CurrentPlatform}/x86_64/latest/";
        private static readonly string _libretroDirectory = Path.Combine(Application.streamingAssetsPath, "libretro~");
        private static readonly string _coresDirectory    = Path.Combine(_libretroDirectory, "cores");
        private static readonly string _coresStatusFile   = Path.Combine(_libretroDirectory, "cores.json");
        private static readonly Color _greenColor         = Color.green;
        private static readonly Color _orangeColor        = new Color(1.0f, 0.5f, 0f, 1f);
        private static readonly Color _redColor           = Color.red;
        private static readonly Color _grayColor          = Color.gray;

        private CoreList _coreList;

        private Vector2 _scrollPos;
        private string _statusText = "";

        [MenuItem("Libretro/Manage Cores"), SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity Editor")]
        private static void ShowWindow()
        {
            if (CurrentPlatform == null)
                return;

            if (!Directory.Exists(_libretroDirectory))
                _ = Directory.CreateDirectory(_libretroDirectory);

            if (!Directory.Exists(_coresDirectory))
                _ = Directory.CreateDirectory(_coresDirectory);


            GetCustomWindow(true).minSize = new Vector2(311f, 200f);
        }

        private void OnEnable()
        {
            if (File.Exists(_coresStatusFile))
                _coreList = FileSystem.DeserializeFromJson<CoreList>(_coresStatusFile);

            Refresh();
        }

        private void OnGUI()
        {
            GUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                using EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos, EditorStyles.helpBox);
                _scrollPos = scrollView.scrollPosition;

                foreach (Core core in _coreList.Cores)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(core.DisplayName, GUILayout.Width(180f));

                        string buttonText;
                        if (core.Available && core.Latest)
                        {
                            GUI.backgroundColor = core.Processing ? _grayColor : _greenColor;
                            buttonText          = core.Processing ? "Busy..." : "OK";
                        }
                        else if (core.Available && !core.Latest)
                        {
                            GUI.backgroundColor = core.Processing ? _grayColor : _orangeColor;
                            buttonText          = core.Processing ? "Busy..." : "Update";
                        }
                        else
                        {
                            GUI.backgroundColor = core.Processing ? _grayColor : _redColor;
                            buttonText          = core.Processing ? "Busy..." : "Download";
                        }

                        if (GUILayout.Button(new GUIContent(buttonText, null, core.DisplayName), GUILayout.Width(100f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        {
                            core.TaskRunning = true;

                            // Threaded processing taken from: https://ru.stackoverflow.com/questions/1088120
                            core.CancellationTokenSource = new CancellationTokenSource();
                            CancellationToken token = core.CancellationTokenSource.Token;
                            SynchronizationContext context = SynchronizationContext.Current;
                            _ = Task.Run(() => DownloadAndExtractTask(core, context, token), token)
                                               .ContinueWith(
                                                   t =>
                                                   {
                                                       HandleTaskException(t);
                                                       OnTaskFinishedOrCanceled(core);
                                                   },
                                                   token,
                                                   TaskContinuationOptions.OnlyOnFaulted,
                                                   TaskScheduler.FromCurrentSynchronizationContext()
                                               );
                        }

                        GUI.backgroundColor = Color.white;
                    }
                }
            }

            GUILayout.Space(8f);
            if (string.IsNullOrEmpty(_statusText))
                EditorGUILayout.HelpBox("Ready", MessageType.None);
            else
                EditorGUILayout.HelpBox(_statusText, MessageType.None);
            GUILayout.Space(8f);
        }

        private void OnTaskFinishedOrCanceled(Core core)
        {
            core.TaskRunning = false;
            core.CancellationTokenSource?.Dispose();
            core.CancellationTokenSource = null;
            core.Processing              = false;
        }

        private static void HandleTaskException(Task task)
        {
            if (task.IsFaulted)
            {
                Exception taskException = task.Exception;
                while (taskException is AggregateException && taskException.InnerException != null)
                    taskException = taskException.InnerException;
                _ = EditorUtility.DisplayDialog("Task chain terminated", $"Exception: {taskException.Message}", "Ok");
            }
        }

        private static LibretroManagerWindow GetCustomWindow(bool focus) => GetWindow<LibretroManagerWindow>("Libretro Core Manager", focus);

        private void Refresh()
        {
            _coreList ??= new CoreList();

            _statusText = "";

            foreach (Core core in _coreList.Cores)
            {
                bool fileExists = File.Exists(Path.GetFullPath(Path.Combine(_coresDirectory, core.FullName.Replace(".zip", ""))));
                if (!fileExists)
                {
                    core.CurrentDate = "";
                    core.LatestDate  = "";
                    core.Available   = false;
                }
            }

            HtmlWeb hw                 = new HtmlWeb();
            HtmlDocument doc           = hw.Load(new Uri(_buildbotUrl));
            HtmlNodeCollection trNodes = doc.DocumentNode.SelectNodes("//body/div/table/tr");

            foreach (HtmlNode trNode in trNodes)
            {
                HtmlNodeCollection tdNodes = trNode.ChildNodes;
                if (tdNodes.Count < 3)
                    continue;

                string fileName = tdNodes[1].InnerText;
                if (!fileName.Contains("_libretro"))
                    continue;

                string lastModified = tdNodes[2].InnerText;
                bool available      = File.Exists(Path.GetFullPath(Path.Combine(_coresDirectory, fileName.Replace(".zip", ""))));
                Core found          = _coreList.Cores.Find(x => x.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    found.LatestDate = lastModified;
                    found.Available  = available;
                }
                else
                {
                    _coreList.Cores.Add(new Core
                    {
                        FullName    = fileName,
                        DisplayName = fileName.Substring(0, fileName.IndexOf("_libretro")),
                        LatestDate  = lastModified,
                        Available   = available
                    });
                }
            }

            _coreList.Cores = _coreList.Cores.OrderBy(x => x.Available).ThenBy(x => x.Latest).ThenBy(x => x.DisplayName).ToList();
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

                _coreList.Cores = _coreList.Cores.OrderBy(x => x.Available).ThenBy(x => x.Latest).ThenBy(x => x.DisplayName).ToList();
                _ = FileSystem.SerializeToJson(_coreList, _coresStatusFile);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            context.Post(_ => GetCustomWindow(true).OnTaskFinishedOrCanceled(core), null);
        }

        private static string DownloadFile(string url)
        {
            using WebClient webClient = new WebClient();
            string fileName = Path.GetFileName(url);
            string filePath = Path.GetFullPath(Path.Combine(_coresDirectory, fileName));
            if (File.Exists(filePath))
                File.Delete(filePath);
            webClient.DownloadFile(url, filePath);
            return filePath;
        }

        private void ExtractFile(string zipPath)
        {
            if (!File.Exists(zipPath))
                return;

            try
            {
                using ZipArchive archive = ZipFile.OpenRead(zipPath);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.GetFullPath(Path.Combine(_coresDirectory, entry.FullName));
                    if (File.Exists(destinationPath))
                        File.Delete(destinationPath);
                    entry.ExtractToFile(destinationPath);
                }
            }
            finally
            {
                File.Delete(zipPath);
            }

            _statusText = $"Processed {Path.GetFileNameWithoutExtension(zipPath)}";
        }
    }
}
