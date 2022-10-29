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

namespace SK.Libretro
{
    internal abstract class DynamicLibrary : IDisposable
    {
        public readonly string Extension;

        public string Path { get; private set; }

        protected string Name { get; private set; }

        protected IntPtr _nativeHandle;

        private readonly bool _deleteFileOnDispose;

        private bool _disposedValue;

        protected DynamicLibrary(string extension, bool deleteFileOnDispose) => (Extension, _deleteFileOnDispose) = (extension, deleteFileOnDispose);

        ~DynamicLibrary() => DisposeImpl();

        public void Dispose()
        {
            DisposeImpl();
            GC.SuppressFinalize(this);
        }

        public void Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("Library path is null or empty.");

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
            if (_nativeHandle.IsNull())
                throw new Exception($"Library '{Name}' at path '{Path}' not loaded, cannot get function '{functionName}'");

            try
            {
                return GetProcAddress(functionName).GetDelegate<T>();
            }
            catch (Exception e)
            {
                throw new Exception($"Function '{functionName}' not found in library '{Name}' at path '{Path}' ({e.Message})");
            }
        }

        protected abstract void LoadLibrary();

        protected abstract IntPtr GetProcAddress(string functionName);

        protected abstract void FreeLibrary();

        private void DisposeImpl()
        {
            if (!_disposedValue)
            {
                try
                {
                    if (_nativeHandle.IsNotNull())
                        FreeLibrary();
                    _nativeHandle = IntPtr.Zero;
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to free library '{Name}' at path '{Path}' ({e.Message})");
                }

                try
                {
                    if (_deleteFileOnDispose)
                        FileSystem.DeleteFile(Path);
                }
                catch/* (Exception e)*/
                {
                    //throw new Exception($"Failed to delete file '{Name}' at path '{Path}' ({e.Message})");
                }

                _disposedValue = true;
            }
        }
    }
}
