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
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SK.Libretro.Unity
{
    [RequireComponent(typeof(AudioSource)), DisallowMultipleComponent]
    public sealed class AudioProcessor : MonoBehaviour, IAudioProcessor
    {
        private const int AUDIO_BUFFER_SIZE = 65535;

        private AudioSource _audioSource;
        private int _inputSampleRate;
        private int _outputSampleRate;

        private NativeRingQueue<float> _circularBuffer;

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_circularBuffer.IsCreated || _circularBuffer.Length < data.Length)
                return;

            for (int i = 0; i < data.Length; ++i)
                data[i] = _circularBuffer.Dequeue();
        }

        private void OnDestroy() => Dispose();

        public void Init(int sampleRate) => MainThreadDispatcher.Enqueue(() =>
        {
            if (_audioSource)
                _audioSource.Stop();

            if (_circularBuffer.IsCreated)
                _circularBuffer.Dispose();
            _circularBuffer = new(AUDIO_BUFFER_SIZE, Allocator.Persistent);

            _inputSampleRate = sampleRate;
            _outputSampleRate = AudioSettings.outputSampleRate;

            if (!_audioSource)
                _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.Play();
        });

        public void Dispose() => MainThreadDispatcher.Enqueue(() =>
        {
            if (_audioSource)
                _audioSource.Stop();

            if (_circularBuffer.IsCreated)
                _circularBuffer.Dispose();
        });

        public void ProcessSample(short left, short right) => MainThreadDispatcher.Enqueue(() =>
        {
            float ratio                 = (float)_outputSampleRate / _inputSampleRate;
            int sourceSamplesCount      = 2;
            int destinationSamplesCount = (int)(sourceSamplesCount * ratio);
            for (int i = 0; i < destinationSamplesCount; i++)
            {
                float sampleIndex = i / ratio;
                int sampleIndex1 = (int)math.floor(sampleIndex);
                if (sampleIndex1 > sourceSamplesCount - 1)
                    sampleIndex1 = sourceSamplesCount - 1;
                int sampleIndex2 = (int)math.ceil(sampleIndex);
                if (sampleIndex2 > sourceSamplesCount - 1)
                    sampleIndex2 = sourceSamplesCount - 1;
                float interpolationFactor = sampleIndex - sampleIndex1;
                _circularBuffer.Enqueue(math.lerp(left  * AudioHandler.NORMALIZED_GAIN,
                                                  right * AudioHandler.NORMALIZED_GAIN,
                                                  interpolationFactor));
            }
        });

        public unsafe void ProcessSampleBatch(IntPtr data, nuint frames) => MainThreadDispatcher.Enqueue(() =>
        {
            short* sourceSamples        = (short*)data;
            float ratio                 = (float)_outputSampleRate / _inputSampleRate;
            int sourceSamplesCount      = (int)frames * 2;
            int destinationSamplesCount = (int)(sourceSamplesCount * ratio);
            for (int i = 0; i < destinationSamplesCount; i++)
            {
                float sampleIndex = i / ratio;
                int sampleIndex1 = (int)math.floor(sampleIndex);
                if (sampleIndex1 > sourceSamplesCount - 1)
                    sampleIndex1 = sourceSamplesCount - 1;
                int sampleIndex2 = (int)math.ceil(sampleIndex);
                if (sampleIndex2 > sourceSamplesCount - 1)
                    sampleIndex2 = sourceSamplesCount - 1;
                float interpolationFactor = sampleIndex - sampleIndex1;
                _circularBuffer.Enqueue(math.lerp(sourceSamples[sampleIndex1] * AudioHandler.NORMALIZED_GAIN,
                                                  sourceSamples[sampleIndex2] * AudioHandler.NORMALIZED_GAIN,
                                                  interpolationFactor));
            }
        });
    }
}
