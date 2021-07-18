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
using Unity.Collections;
using static SK.Libretro.LibretroHeader;

namespace SK.Libretro
{
    internal sealed class LibretroAudio
    {
        public IAudioProcessor Processor;
        public retro_audio_callback AudioCallback;

        private const float GAIN            = 1f;
        private const float NORMALIZED_GAIN = GAIN / 0x8000;

        private readonly LibretroWrapper _wrapper;

        public LibretroAudio(LibretroWrapper wrapper) => _wrapper = wrapper;

        public void Enable(IAudioProcessor audioProcessor)
        {
            Processor = audioProcessor;
            Processor?.Init(Convert.ToInt32(_wrapper.Game.SystemAVInfo.timing.sample_rate));
        }

        public void Disable()
        {
            Processor?.DeInit();
            Processor = null;
        }

        public void SampleCallback(short left, short right)
        {
            if (Processor is null)
                return;

            NativeArray<float> floatBuffer = new NativeArray<float>(2, Allocator.TempJob);
            floatBuffer[0] = left * NORMALIZED_GAIN;
            floatBuffer[1] = right * NORMALIZED_GAIN;

            Processor.ProcessSamples(ref floatBuffer);
            floatBuffer.Dispose();
        }

        public unsafe ulong SampleBatchCallback(short* data, ulong frames)
        {
            if (Processor is null)
                return frames;

            int numSamples                 = Convert.ToInt32(frames) * 2;
            NativeArray<float> floatBuffer = new NativeArray<float>(numSamples, Allocator.TempJob);
            for (int i = 0; i < numSamples; ++i)
                floatBuffer[i] = data[i] * NORMALIZED_GAIN;

            Processor.ProcessSamples(ref floatBuffer);
            return frames;
        }
    }
}
