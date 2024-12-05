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
using System.Runtime.InteropServices;
using Unity.Jobs;
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class GraphicsProcessor : IGraphicsProcessor
    {
        public IntPtr NativeWindow { get; }

        private readonly Action<Texture> _onTextureRecreated;

        private FilterMode _filterMode;
        private Texture2D _texture;
        private JobHandle _jobHandle;

        public GraphicsProcessor(Action<Texture> textureRecreatedCallback, FilterMode filterMode)
        {
            _onTextureRecreated = textureRecreatedCallback;
            _filterMode         = filterMode;

            if (Application.platform == UnityEngine.RuntimePlatform.Android)
                NativeWindow = GetNativeWindowHandle();

            Debug.LogWarning($"NativeWindow: {NativeWindow}");
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

        private void CreateTexture(int width, int height)
        {
            if (!Application.isPlaying)
                return;

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

        private IntPtr GetNativeWindowHandle()
        {
            // Get the current Android Activity
            using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // Get the Surface from the Activity
            AndroidJavaObject surfaceHolder = activity.Call<AndroidJavaObject>("getSurfaceHolder");
            if (surfaceHolder == null)
                return IntPtr.Zero;

            AndroidJavaObject surface = surfaceHolder.Call<AndroidJavaObject>("getSurface");
            if (surface == null)
                return IntPtr.Zero;

            // Get the native window from the Surface
            IntPtr surfaceHandle = surface.GetRawObject();
            IntPtr jniEnv = (IntPtr)AndroidJNI.AttachCurrentThread();

            IntPtr nativeWindow = ANativeWindow_fromSurface(jniEnv, surfaceHandle);
            return nativeWindow;
        }

        [DllImport("android")] private static extern IntPtr ANativeWindow_fromSurface(IntPtr jniEnv, IntPtr surface);
    }
}
