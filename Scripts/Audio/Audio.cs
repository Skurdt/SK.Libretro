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

using SK.Libretro.Header;
using System;

namespace SK.Libretro
{
    internal sealed class Audio
    {
        public readonly retro_audio_sample_t SampleCallback;
        public readonly retro_audio_sample_batch_t SampleBatchCallback;

        public retro_audio_callback AudioCallback;

        public bool Enabled { get; set; }

        private const float GAIN            = 1f;
        private const float NORMALIZED_GAIN = GAIN / 0x8000;

        private readonly Wrapper _wrapper;
        private IAudioProcessor _processor;

        public Audio(Wrapper wrapper)
        {
            SampleCallback      = SampleCallbackCall;
            SampleBatchCallback = SampleBatchCallbackCall;
            _wrapper = wrapper;
        }

        public void Init(IAudioProcessor audioProcessor)
        {
            _processor = audioProcessor;
            if (_processor != null)
                _processor.Init(_wrapper.Game.SystemAVInfo.SampleRate);
        }

        public void DeInit()
        {
            if (_processor != null)
                _processor.DeInit();
            _processor = null;
        }

        public void SampleCallbackCall(short left, short right)
        {
            if (!Enabled || _processor == null)
                return;

            float[] floatBuffer = new float[]
            {
                left  * NORMALIZED_GAIN,
                right * NORMALIZED_GAIN
            };

            _processor.ProcessSamples(floatBuffer);
        }

        public unsafe nuint SampleBatchCallbackCall(IntPtr data, nuint frames)
        {
            if (Enabled && _processor != null)
            {
                short* dataPtr = (short*)data;
                ulong numSamples = frames * 2;
                float[] floatBuffer = new float[numSamples];
                for (ulong i = 0; i < numSamples; ++i)
                    floatBuffer[i] = dataPtr[i] * NORMALIZED_GAIN;

                _processor.ProcessSamples(floatBuffer);
            }
            return frames;
        }
    }
}
