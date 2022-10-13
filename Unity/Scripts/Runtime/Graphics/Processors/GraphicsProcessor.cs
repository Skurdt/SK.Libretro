/* MIT License

 * Copyright (c) 2022 Skurdt
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
using Unity.Jobs;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal class GraphicsProcessor : IGraphicsProcessor
    {
        private readonly Action<Texture> _onTextureRecreated;

        private Texture2D _texture;
        private JobHandle _jobHandle;

        public GraphicsProcessor(int width, int height, Action<Texture> textureRecreatedCallback, FilterMode filterMode = FilterMode.Point)
        {
            _onTextureRecreated = textureRecreatedCallback;
            Construct(width, height, filterMode);
        }

        public virtual void Construct(int width, int height, FilterMode filterMode = FilterMode.Point) => CreateTexture(width, height, filterMode);

        public virtual void Dispose()
        {
            if (!_jobHandle.IsCompleted)
                _jobHandle.Complete();

            UnityEngine.Object.Destroy(_texture);
        }

        public unsafe virtual void ProcessFrame0RGB1555(ushort* data, int width, int height, int pitch)
        {
            CreateTexture(width, height);

            _jobHandle = new Frame0RGB1555Job
            {
                SourceData = data,
                Width = width,
                Height = height,
                PitchPixels = pitch / sizeof(ushort),
                TextureData = _texture.GetRawTextureData<uint>()
            }.Schedule();

            _jobHandle.Complete();
            _texture.Apply();
        }

        public unsafe virtual void ProcessFrameXRGB8888(uint* data, int width, int height, int pitch)
        {
            CreateTexture(width, height);

            _jobHandle = new FrameXRGB8888Job
            {
                SourceData = data,
                Width = width,
                Height = height,
                PitchPixels = pitch / sizeof(uint),
                TextureData = _texture.GetRawTextureData<uint>()
            }.Schedule();

            _jobHandle.Complete();
            _texture.Apply();
        }

        public unsafe virtual void ProcessFrameXRGB8888VFlip(uint* data, int width, int height, int pitch)
        {
            CreateTexture(width, height);

            _jobHandle = new FrameXRGB8888VFlipJob
            {
                SourceData = data,
                Width = width,
                Height = height,
                PitchPixels = pitch / sizeof(uint),
                TextureData = _texture.GetRawTextureData<uint>()
            }.Schedule();

            _jobHandle.Complete();
            _texture.Apply();
        }

        public unsafe virtual void ProcessFrameRGB565(ushort* data, int width, int height, int pitch)
        {
            CreateTexture(width, height);

            _jobHandle = new FrameRGB565Job
            {
                SourceData = data,
                Width = width,
                Height = height,
                PitchPixels = pitch / sizeof(ushort),
                TextureData = _texture.GetRawTextureData<uint>()
            }.Schedule();

            _jobHandle.Complete();
            _texture.Apply();
        }

        private void CreateTexture(int width, int height, FilterMode filterMode = FilterMode.Point)
        {
            if (!_texture || _texture.width != width || _texture.height != height)
            {
                UnityEngine.Object.Destroy(_texture);
                _texture = new Texture2D(width, height, TextureFormat.BGRA32, false)
                {
                    filterMode = filterMode
                };

                _onTextureRecreated(_texture);
            }
        }
    }
}
