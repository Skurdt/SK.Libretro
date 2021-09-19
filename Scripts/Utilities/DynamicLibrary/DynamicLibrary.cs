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
using System.Runtime.InteropServices;

namespace SK.Libretro
{
    internal abstract class DynamicLibrary
    {
        public readonly string Extension;

        protected string Path { get; private set; }
        protected string Name { get; private set; }

        protected IntPtr _nativeHandle;

        protected DynamicLibrary(string extension) => Extension = extension;

        public void Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("Library path == null or empty.");

            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);

            try
            {
                LoadLibrary();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load library '{Name}' at path '{Path}' ({e.Message})");
            }
        }

        public T GetFunction<T>(string functionName) where T : Delegate
        {
            if (_nativeHandle == IntPtr.Zero)
                throw new Exception($"Library '{Name}' at path '{Path}' not loaded, cannot get function '{functionName}'");

            try
            {
                return Marshal.GetDelegateForFunctionPointer<T>(GetProcAddress(functionName));
            }
            catch (Exception e)
            {
                throw new Exception($"Function '{functionName}' not found in library '{Name}' at path '{Path}' ({e.Message})");
            }
        }

        public void Free(bool deleteFile = false)
        {
            try
            {
                if (_nativeHandle != IntPtr.Zero)
                    FreeLibrary();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to free library '{Name}' at path '{Path}' ({e.Message})");
            }
            finally
            {
                if (deleteFile && File.Exists(Path))
                    File.Delete(Path);
            }
        }

        protected abstract void LoadLibrary();

        protected abstract IntPtr GetProcAddress(string functionName);

        protected abstract void FreeLibrary();
    }
}
