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
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace SK.Libretro.Unity
{
    public sealed class GraphicsProcessorHardware : IGraphicsProcessor
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ContextData
        {
            public IntPtr TextureHandle;
            public int Width;
            public int Height;
            public bool Depth;
            public bool Stencil;

            public ContextData(Texture2D texture, bool depth, bool stencil)
            {
                TextureHandle = texture.GetNativeTexturePtr();
                Width         = texture.width;
                Height        = texture.height;
                Depth         = depth;
                Stencil       = stencil;
            }
        }

        public Action<Texture> OnTextureRecreated;

        public bool Initialized { get; private set; }
        public Texture2D Texture { get; private set; }

        private readonly CommandBuffer _retroRunCommandBuffer = new CommandBuffer();

        public unsafe GraphicsProcessorHardware(int width, int height, bool depth, bool stencil, FilterMode filterMode = FilterMode.Point)
        {
            Texture = new Texture2D(width, height, TextureFormat.RGB24, false)
            {
                filterMode = filterMode
            };

            ContextData initContextData = new ContextData(Texture, depth, stencil);
            CommandBuffer cb = new CommandBuffer();
            cb.IssuePluginEventAndData(LibretroPlugin.GetRenderEventFunc(), 0, (IntPtr)(&initContextData));
            Graphics.ExecuteCommandBuffer(cb);
            Initialized = true;
        }

        public void ClearRetroRunCommandQueue() => _retroRunCommandBuffer.Clear();

        public void UpdateRetroRunCommandQueue() => _retroRunCommandBuffer.IssuePluginEvent(LibretroPlugin.GetRenderEventFunc(), 2);

        public void FlushRetroRunCommandQueue() => Graphics.ExecuteCommandBuffer(_retroRunCommandBuffer);

        public void DeInit()
        {
            if (Initialized)
            {
                CommandBuffer cb = new CommandBuffer();
                cb.IssuePluginEvent(LibretroPlugin.GetRenderEventFunc(), 1);
                Graphics.ExecuteCommandBuffer(cb);
                Initialized = false;
            }

            if (Texture != null)
#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(Texture);
#else
                UnityEngine.Object.Destroy(Texture);
#endif
        }

        public unsafe void ProcessFrame0RGB1555(ushort* data, int width, int height, int pitch) => UpdateTexture(width, height);

        public unsafe void ProcessFrameXRGB8888(uint* data, int width, int height, int pitch) => UpdateTexture(width, height);

        public unsafe void ProcessFrameRGB565(ushort* data, int width, int height, int pitch) => UpdateTexture(width, height);

        private unsafe void UpdateTexture(int width, int height)
        {
            if (Texture.width != width || Texture.height != height)
            {
            //    Texture.width = width;
            //    Texture.height = height;

            //    CommandBuffer cb = new CommandBuffer();
            //    cb.IssuePluginEvent(LibretroPlugin.GetRenderEventFunc(), 1);
            //    fixed (void* p = &_initContextData)
            //    {
            //        cb.IssuePluginEventAndData(LibretroPlugin.GetRenderEventFunc(), 0, (IntPtr)p);
            //        Graphics.ExecuteCommandBuffer(cb);
            //    }
            }
        }
    }
}
