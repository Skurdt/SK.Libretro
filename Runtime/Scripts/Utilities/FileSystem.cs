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

using System;
using System.IO;

namespace SK.Libretro.Utilities
{
    internal static class FileSystem
    {
        public static bool FileExists(string path) => File.Exists(path);

        public static bool CreateFile(string path)
        {
            try
            {
                using (_ = File.Create(GetAbsolutePath(path)))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e, "FileSystem.CreateFile");
            }

            return false;
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(GetAbsolutePath(path));
                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e, "FileSystem.DeleteFile");
            }

            return false;
        }

        public static string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            try
            {
                if (path.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                    return Path.GetFullPath(Path.Combine(UnityEngine.Application.streamingAssetsPath, path.Remove(0, 1)));
            }
            catch (Exception e)
            {
                Logger.LogException(e, "FileSystem.GetAbsolutePath");
            }

            return Path.GetFullPath(path);
        }

        public static string GetRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            try
            {
                string fullPath = GetAbsolutePath(path);
                string formattedStreamingAssetsPath = UnityEngine.Application.streamingAssetsPath.Replace('/', Path.DirectorySeparatorChar);
                if (fullPath.Contains(formattedStreamingAssetsPath))
                    return $"@{fullPath.Replace($"{formattedStreamingAssetsPath}", "").Remove(0, 1)}";
            }
            catch (Exception e)
            {
                Logger.LogException(e, "FileSystem.GetRelativePath");
            }

            return path;
        }

        public static string[] GetFilesInDirectory(string path, string searchPattern, bool includeSubFolders = false)
        {
            try
            {
                return Directory.GetFiles(GetAbsolutePath(path), searchPattern, includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                Logger.LogException(e, "FileSystem.GetFilesInDirectory");
            }

            return null;
        }

        public static bool SerializeToJson<T>(T sourceObject, string targetPath)
        {
            try
            {
                string jsonString = UnityEngine.JsonUtility.ToJson(sourceObject, true);
                File.WriteAllText(GetAbsolutePath(targetPath), jsonString);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogException(e, $"FileSystem.SerializeToJson<{typeof(T).Name}>");
            }

            return false;
        }

        public static T DeserializeFromJson<T>(string sourcePath) where T : class
        {
            try
            {
                string jsonString = File.ReadAllText(GetAbsolutePath(sourcePath));
                return UnityEngine.JsonUtility.FromJson<T>(jsonString);
            }
            catch (Exception e)
            {
                Logger.LogException(e, $"FileSystem.DeserializeFromJson<{typeof(T).Name}>");
            }

            return null;
        }
    }
}
