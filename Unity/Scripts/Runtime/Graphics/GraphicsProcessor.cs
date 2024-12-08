﻿/* MIT License

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
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Jobs;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class GraphicsProcessor : IGraphicsProcessor
    {
        public IntPtr NativeWindow { get; }

        private readonly Action<Texture> _onTextureRecreated;
        private readonly ManualResetEventSlim _manualResetEvent;

        private FilterMode _filterMode;
        private Texture2D _texture;
        private JobHandle _jobHandle;

        public GraphicsProcessor(Action<Texture> textureRecreatedCallback, FilterMode filterMode)
        {
            _onTextureRecreated = textureRecreatedCallback;
            _filterMode         = filterMode;
            _manualResetEvent   = new(false);
        }

        public void Dispose() => MainThreadDispatcher.Enqueue(() =>
        {
            if (!_jobHandle.IsCompleted)
                _jobHandle.Complete();

            if (Application.isPlaying && _texture)
                UnityEngine.Object.Destroy(_texture);
        });

        public void SetFilterMode(FilterMode filterMode) => MainThreadDispatcher.Enqueue(() =>
        {
            _filterMode = filterMode;

            if (_texture)
                _texture.filterMode = filterMode;
        });

        public IntPtr GetCurrentSoftwareFramebuffer(int width, int height)
        {
            IntPtr result = IntPtr.Zero;

            _manualResetEvent.Reset();
            using CancellationTokenSource tokenSource = new();
            MainThreadDispatcher.Enqueue(() =>
            {
                try
                {
                    CreateTexture(width, height, TextureFormat.RGB565);
                    if (!_texture)
                        throw new NullReferenceException("Texture not created");

                    GCHandle handle = GCHandle.Alloc(_texture.GetRawTextureData(), GCHandleType.Pinned);
                    result = handle.AddrOfPinnedObject();
                    handle.Free();
                }
                finally
                {
                    _manualResetEvent.Set();
                }
            });
            _manualResetEvent.Wait(tokenSource.Token);
            tokenSource.Dispose();

            return result;
        }

        public unsafe void ProcessFrameSoftwareFramebuffer(IntPtr data, int pitch, int height) => MainThreadDispatcher.Enqueue(() =>
        {
            _texture.LoadRawTextureData(data, pitch * height * sizeof(short));
            _texture.Apply();
        });

        public unsafe void ProcessFrame0RGB1555(IntPtr data, int width, int height, int pitch) => MainThreadDispatcher.Enqueue(() =>
        {
            CreateTexture(width, height);
            if (!_texture)
                return;

            _jobHandle = new Frame0RGB1555Job
            {
                SourceData  = (ushort*)data,
                Width       = width,
                Height      = height,
                PitchPixels = pitch / sizeof(ushort),
                TextureData = _texture.GetRawTextureData<uint>()
            }.Schedule(width * height, 64);
            _jobHandle.Complete();
            _texture.Apply();
        });

        public unsafe void ProcessFrameXRGB8888(IntPtr data, int width, int height, int pitch) => MainThreadDispatcher.Enqueue(() =>
        {
            CreateTexture(width, height);
            if (!_texture)
                return;

            _jobHandle = new FrameXRGB8888Job
            {
                SourceData  = (uint*)data,
                Width       = width,
                Height      = height,
                PitchPixels = pitch / sizeof(uint),
                TextureData = _texture.GetRawTextureData<uint>()
            }.Schedule(width * height, 64);
            _jobHandle.Complete();
            _texture.Apply();
        });

        public unsafe void ProcessFrameXRGB8888VFlip(IntPtr data, int width, int height, int pitch) => MainThreadDispatcher.Enqueue(() =>
        {
            CreateTexture(width, height);
            if (!_texture)
                return;

            _jobHandle = new FrameXRGB8888VFlipJob
            {
                SourceData  = (uint*)data,
                Width       = width,
                Height      = height,
                PitchPixels = pitch / sizeof(uint),
                TextureData = _texture.GetRawTextureData<uint>()
            }.Schedule(width * height, 64);
            _jobHandle.Complete();
            _texture.Apply();
        });

        public unsafe void ProcessFrameRGB565(IntPtr data, int width, int height, int pitch) => MainThreadDispatcher.Enqueue(() =>
        {
            CreateTexture(width, height);
            if (!_texture)
                return;

            _jobHandle = new FrameRGB565Job
            {
                SourceData  = (ushort*)data,
                Width       = width,
                Height      = height,
                PitchPixels = pitch / sizeof(ushort),
                TextureData = _texture.GetRawTextureData<uint>()
            }.Schedule(width * height, 64);
            _jobHandle.Complete();
            _texture.Apply();
        });

        private void CreateTexture(int width, int height, TextureFormat textureFormat = TextureFormat.BGRA32)
        {
            if (!Application.isPlaying)
                return;

            if (!_texture || _texture.width != width || _texture.height != height)
            {
                UnityEngine.Object.Destroy(_texture);
                _texture = new Texture2D(width, height, textureFormat, false, false, false)
                {
                    filterMode = _filterMode
                };
                _onTextureRecreated(_texture);
            }
        }
    }
}
