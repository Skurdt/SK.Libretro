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
using UnityEngine;

namespace SK.Libretro.Unity
{
    internal sealed class GraphicsProcessorSeparateThread : GraphicsProcessor
    {
        public GraphicsProcessorSeparateThread(int width, int height, Action<Texture> textureRecreatedCallback, FilterMode filterMode = FilterMode.Point)
        : base(width, height, textureRecreatedCallback, filterMode)
        {
        }

        public override void Construct(int width, int height, FilterMode filterMode = FilterMode.Point) =>
            MainThreadDispatcher.Enqueue(() => base.Construct(width, height, filterMode));

        public override void Dispose() =>
            MainThreadDispatcher.Enqueue(() => base.Dispose());

        public unsafe override void ProcessFrame0RGB1555(ushort* data, int width, int height, int pitch) =>
            MainThreadDispatcher.Enqueue(() => base.ProcessFrame0RGB1555(data, width, height, pitch));

        public unsafe override void ProcessFrameXRGB8888(uint* data, int width, int height, int pitch) =>
            MainThreadDispatcher.Enqueue(() => base.ProcessFrameXRGB8888(data, width, height, pitch));

        public unsafe override void ProcessFrameXRGB8888VFlip(uint* data, int width, int height, int pitch) =>
            MainThreadDispatcher.Enqueue(() => base.ProcessFrameXRGB8888VFlip(data, width, height, pitch));

        public unsafe override void ProcessFrameRGB565(ushort* data, int width, int height, int pitch) =>
            MainThreadDispatcher.Enqueue(() => base.ProcessFrameRGB565(data, width, height, pitch));
    }
}
