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

using SK.Libretro.Header;
using System;

namespace SK.Libretro
{
    internal sealed class SystemAVInfo
    {
        public int BaseWidth     { get; private set; }
        public int BaseHeight    { get; private set; }
        public int MaxWidth      { get; private set; }
        public int MaxHeight     { get; private set; }
        public float AspectRatio { get; private set; }
        public double Fps        { get; private set; }
        public int SampleRate    { get; private set; }

        public void Init(retro_system_av_info info)
        {
            SetGeometry(info.geometry);

            Fps         = info.timing.fps;
            SampleRate  = Convert.ToInt32(info.timing.sample_rate);
        }

        public void SetGeometry(retro_game_geometry geometry)
        {
            BaseWidth   = (int)geometry.base_width;
            BaseHeight  = (int)geometry.base_height;
            MaxWidth    = (int)geometry.max_width;
            MaxHeight   = (int)geometry.max_height;
            AspectRatio = geometry.aspect_ratio;
        }
    }
}
