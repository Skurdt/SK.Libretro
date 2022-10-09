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
using static SK.Libretro.Header;

namespace SK.Libretro
{
    internal sealed class Audio
    {
        public retro_audio_callback AudioCallback;

        public bool Enabled { get; set; }

        private const float GAIN            = 1f;
        private const float NORMALIZED_GAIN = GAIN / 0x8000;

        private readonly Wrapper _wrapper;
        private IAudioProcessor _processor;

        public Audio(Wrapper wrapper) => _wrapper = wrapper;

        public void Init(IAudioProcessor audioProcessor)
        {
            _processor = audioProcessor;
            _processor?.Init(Convert.ToInt32(_wrapper.Game.SystemAVInfo.timing.sample_rate));
        }

        public void DeInit()
        {
            _processor?.DeInit();
            _processor = null;
        }

        public void SampleCallback(short left, short right)
        {
            if (_processor == null || !Enabled)
                return;

            float[] floatBuffer = new float[]
            {
                left  * NORMALIZED_GAIN,
                right * NORMALIZED_GAIN
            };

            _processor.ProcessSamples(floatBuffer);
        }

        public unsafe ulong SampleBatchCallback(short* data, ulong frames)
        {
            if (_processor != null)
            {
                ulong numSamples = frames * 2;
                float[] floatBuffer = new float[numSamples];

                for (ulong i = 0; i < numSamples; ++i)
                    floatBuffer[i] = data[i] * NORMALIZED_GAIN;

                _processor.ProcessSamples(floatBuffer);
            }
            return frames;
        }
    }
}
