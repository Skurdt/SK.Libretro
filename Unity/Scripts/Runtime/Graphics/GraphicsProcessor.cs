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
using System;
using System.Threading;
using Unity.Jobs;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class GraphicsProcessor : IGraphicsProcessor
    {
        private readonly Action<Texture> _onTextureRecreated;
        private readonly CancellationToken _cancellationToken;

        private FilterMode _filterMode;
        private Texture2D _texture;
        private JobHandle _jobHandle;

        public GraphicsProcessor(Action<Texture> textureRecreatedCallback, FilterMode filterMode, CancellationToken cancellationToken)
        {
            _onTextureRecreated = textureRecreatedCallback;
            _filterMode         = filterMode;
            _cancellationToken  = cancellationToken;
        }

        public async void Dispose()
        {
            await UniTask.SwitchToMainThread(_cancellationToken);
            if (!_jobHandle.IsCompleted)
                _jobHandle.Complete();
            UnityEngine.Object.Destroy(_texture);
            await UniTask.SwitchToThreadPool();
        }

        public void SetFilterMode(FilterMode filterMode)
        {
            _filterMode = filterMode;
            _texture.filterMode = filterMode;
        }

        public async void ProcessFrame0RGB1555(IntPtr data, int width, int height, int pitch)
        {
            await UniTask.SwitchToMainThread(_cancellationToken);
            CreateTexture(width, height);
            unsafe
            {
                _jobHandle = new Frame0RGB1555Job
                {
                    SourceData = (ushort*)data,
                    Width = width,
                    Height = height,
                    PitchPixels = pitch / sizeof(ushort),
                    TextureData = _texture.GetRawTextureData<uint>()
                }.Schedule();
            }
            await UniTask.SwitchToThreadPool();
        }

        public async void ProcessFrameXRGB8888(IntPtr data, int width, int height, int pitch)
        {
            await UniTask.SwitchToMainThread(_cancellationToken);
            CreateTexture(width, height);
            unsafe
            {
                _jobHandle = new FrameXRGB8888Job
                {
                    SourceData = (uint*)data,
                    Width = width,
                    Height = height,
                    PitchPixels = pitch / sizeof(uint),
                    TextureData = _texture.GetRawTextureData<uint>()
                }.Schedule();
            }
            await UniTask.SwitchToThreadPool();
        }

        public async void ProcessFrameXRGB8888VFlip(IntPtr data, int width, int height, int pitch)
        {
            await UniTask.SwitchToMainThread(_cancellationToken);
            CreateTexture(width, height);
            unsafe
            {
                _jobHandle = new FrameXRGB8888VFlipJob
                {
                    SourceData = (uint*)data,
                    Width = width,
                    Height = height,
                    PitchPixels = pitch / sizeof(uint),
                    TextureData = _texture.GetRawTextureData<uint>()
                }.Schedule();
            }
            await UniTask.SwitchToThreadPool();
        }

        public async void ProcessFrameRGB565(IntPtr data, int width, int height, int pitch)
        {
            await UniTask.SwitchToMainThread(_cancellationToken);
            CreateTexture(width, height);
            unsafe
            {
                _jobHandle = new FrameRGB565Job
                {
                    SourceData = (ushort*)data,
                    Width = width,
                    Height = height,
                    PitchPixels = pitch / sizeof(ushort),
                    TextureData = _texture.GetRawTextureData<uint>()
                }.Schedule();
            }
            await UniTask.SwitchToThreadPool();
        }

        public async void FinalizeFrame()
        {
            await UniTask.SwitchToMainThread(_cancellationToken);
            _jobHandle.Complete();
            _texture.Apply();
            await UniTask.SwitchToThreadPool();
        }

        private void CreateTexture(int width, int height)
        {
            if (!_texture || _texture.width != width || _texture.height != height)
            {
                UnityEngine.Object.Destroy(_texture);
                _texture = new Texture2D(width, height, TextureFormat.BGRA32, false, false, false)
                {
                    filterMode = _filterMode
                };
                _onTextureRecreated(_texture);
            }
        }
    }
}
