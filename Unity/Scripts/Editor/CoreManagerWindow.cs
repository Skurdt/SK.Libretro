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
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace SK.Libretro.Unity.Editor
{
    internal abstract class CoreManagerWindow : EditorWindow
    {
        [Serializable]
        protected sealed class Core
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
        protected sealed class CoreList
        {
            public List<Core> Cores = new();
        }

        protected static string _buildbotUrl;
        protected static string _libretroDirectory;
        protected static string _coresDirectory;
        protected static string _coresStatusFile;

        protected static string CurrentPlatform => Application.platform switch
        {
            UnityEngine.RuntimePlatform.LinuxEditor   => "linux",
            UnityEngine.RuntimePlatform.OSXEditor     => "apple/osx",
            UnityEngine.RuntimePlatform.WindowsEditor => "windows",
            _                                         => InvalidPlatformDetected()
        };

        protected CoreList _coreList;
        protected List<Core> _coreListDisplay;

        protected static void InitializePaths()
        {
            _buildbotUrl       ??= $"https://buildbot.libretro.com/nightly/{CurrentPlatform}/x86_64/latest/";
            _libretroDirectory ??= $"{Application.persistentDataPath}/Libretro";
            _coresDirectory    ??= $"{_libretroDirectory}/cores";
            _coresStatusFile   ??= $"{_libretroDirectory}/cores.json";
        }

        protected void UpdateCoreListData()
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

        protected async UniTask ListItemButtonClickedCallback(Core core, CancellationToken cancellationToken)
        {
            core.Processing = true;
            await DownloadAndExtractTask(core, cancellationToken);
            core.Processing = false;
        }

        protected async UniTask DownloadAndExtractTask(Core core, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                string zipPath = await DownloadFile($"{_buildbotUrl}{core.FullName}", _coresDirectory, cancellationToken);
                ExtractFile(zipPath, _coresDirectory);
                await UniTask.Delay(500, DelayType.Realtime, cancellationToken: cancellationToken);
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

        protected static async UniTask<string> DownloadFile(string url, string directory, CancellationToken cancellationToken)
        {
            try
            {
                string fileName = Path.GetFileName(url);
                string filePath = $"{directory}/{fileName}";
                FileSystem.DeleteFile(filePath);
                await UniTask.Delay(200, DelayType.Realtime, cancellationToken: cancellationToken);

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

        protected static void ExtractFile(string zipPath, string directory)
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
