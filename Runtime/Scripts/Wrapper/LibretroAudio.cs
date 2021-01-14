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

namespace SK.Libretro
{
    internal sealed class LibretroAudio
    {
        public IAudioProcessor Processor;

        private const float AUDIO_GAIN = 1f;

        private readonly LibretroWrapper _wrapper;

        public LibretroAudio(LibretroWrapper wrapper) => _wrapper = wrapper;

        public void Init() => Processor?.Init(Convert.ToInt32(_wrapper.Game.SystemAVInfo.timing.sample_rate));

        public void DeInit() => Processor?.DeInit();

        public void SampleCallback(short left, short right)
        {
            if (Processor == null)
                return;

            float gain          = AUDIO_GAIN / 0x8000;
            float[] floatBuffer = new float[]
            {
                left  * gain,
                right * gain
            };

            Processor.ProcessSamples(floatBuffer);
        }

        public unsafe ulong SampleBatchCallback(short* data, ulong frames)
        {
            if (Processor != null)
            {
                float gain          = AUDIO_GAIN / 0x8000;
                uint numSamples     = Convert.ToUInt32(frames) * 2;
                float[] floatBuffer = new float[numSamples];

                for (int i = 0; i < numSamples; ++i)
                    floatBuffer[i] = data[i] * gain;

                Processor.ProcessSamples(floatBuffer);
            }

            return frames;
        }
    }
}
