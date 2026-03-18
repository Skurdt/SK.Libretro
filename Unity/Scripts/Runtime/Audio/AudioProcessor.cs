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
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SK.Libretro.Unity
{
    [RequireComponent(typeof(AudioSource)), DisallowMultipleComponent]
    public sealed class AudioProcessor : MonoBehaviour, IAudioProcessor
    {
        private const int N_INPUT_CHANNELS = 2;

        private int _bufferDeadzone;
        private int _bufferFillTarget;
        private bool _bufferIsPrimed;
        private double _sampleRatio;

        private AudioSource _audioSource;
        private NativeRingQueue<float> _circularBuffer;
        private AudioResampler _resampler;

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_bufferIsPrimed || _circularBuffer.Length == 0)
                return;

            double stepSize = _sampleRatio;

            _resampler.Resample(data, channels, stepSize);
        }

        private void OnDestroy() => Dispose();

        public async void Init(int sampleRate, double fps)
        {
            await Awaitable.MainThreadAsync();

            if (_audioSource)
                _audioSource.Stop();

            int inputSampleRate = sampleRate;
            int outputSampleRate = AudioSettings.outputSampleRate;
            _sampleRatio = (double)inputSampleRate / outputSampleRate;

            var config = AudioSettings.GetConfiguration();
            int dspBufferSize = AudioSettings.GetConfiguration().dspBufferSize;
            int dspBufferInputSize = Mathf.CeilToInt(dspBufferSize * (float)_sampleRatio) * N_INPUT_CHANNELS;

            _bufferFillTarget = dspBufferInputSize * 2;
            _bufferDeadzone = Mathf.CeilToInt(inputSampleRate / (float)fps) * N_INPUT_CHANNELS;

            if (!_audioSource)
                _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;

            InitBuffer(inputSampleRate, N_INPUT_CHANNELS, 3.0f);
            _resampler = new AudioResampler(_circularBuffer);

            _audioSource.Play();
        }

        public async void Dispose()
        {
            await Awaitable.MainThreadAsync();

            if (_audioSource)
                _audioSource.Stop();

            if (_circularBuffer.IsCreated)
                _circularBuffer.Dispose();
        }

        public void ProcessSample(short left, short right)
        {
            if (!_circularBuffer.IsCreated)
                return;

            if (_circularBuffer.Length + 2 > _circularBuffer.Capacity)
                return;

            _circularBuffer.Enqueue(left * AudioHandler.NORMALIZED_GAIN);
            _circularBuffer.Enqueue(right * AudioHandler.NORMALIZED_GAIN);

            UpdateBufferStatus();
        }

        public unsafe void ProcessSampleBatch(IntPtr data, nuint frames, PositionalData positionalData)
        {
            if (!_circularBuffer.IsCreated)
                return;

            short* sourceSamples = (short*)data;
            int sourceSamplesCount = (int)frames * N_INPUT_CHANNELS;

            if (_circularBuffer.Length + sourceSamplesCount > _circularBuffer.Capacity)
                return;

            for (int i = 0; i < sourceSamplesCount; i++)
                _circularBuffer.Enqueue(sourceSamples[i] * AudioHandler.NORMALIZED_GAIN);

            UpdateBufferStatus();
        }

        private void InitBuffer(int sampleRate, int channels, float bufferDurationSeconds)
        {
            int bufferSize = (int)(sampleRate * channels * bufferDurationSeconds);
            if (_circularBuffer.IsCreated)
                _circularBuffer.Dispose();

            _circularBuffer = new(bufferSize, Allocator.Persistent);
            _bufferIsPrimed = false;
        }

        private void UpdateBufferStatus()
        {
            if (!_bufferIsPrimed) _bufferIsPrimed = _circularBuffer.Length >= _bufferFillTarget - _bufferDeadzone;
        }

        private class AudioResampler
        {
            private NativeRingQueue<float> _circularBuffer;
            private double _fractionalPosition = 0;
            private float _L0 = 0f, _L1 = 0f, _L2 = 0f, _L3 = 0f;
            private float _R0 = 0f, _R1 = 0f, _R2 = 0f, _R3 = 0f;

            public AudioResampler(NativeRingQueue<float> circularBuffer)
            {
                _circularBuffer = circularBuffer;
            }

            public void Resample(float[] data, int channels, double stepSize)
            {
                for (int i = 0; i < data.Length; i += channels)
                {
                    while (_fractionalPosition >= 1.0)
                    {
                        _L0 = _L1;
                        _L1 = _L2;
                        _L2 = _L3;
                        _R0 = _R1;
                        _R1 = _R2;
                        _R2 = _R3;

                        if (_circularBuffer.TryDequeue(out float newL) && _circularBuffer.TryDequeue(out float newR))
                        {
                            _L3 = newL;
                            _R3 = newR;
                        }
                        else
                        {
                            _L3 = 0f;
                            _R3 = 0f;
                        }

                        _fractionalPosition -= 1.0;
                    }
    
                    float t = (float)_fractionalPosition;
                    data[i] = CubicInterpolate(_L0, _L1, _L2, _L3, t);

                    if (channels > 1)
                        data[i + 1] = CubicInterpolate(_R0, _R1, _R2, _R3, t);

                    _fractionalPosition += stepSize;
                }
            }

            private static float CubicInterpolate(float v0, float v1, float v2, float v3, float t)
            {
                float t2 = t * t;
                float t3 = t2 * t;

                float P = -v0 + 3f * v1 - 3f * v2 + v3;
                float Q = 2f * v0 - 5f * v1 + 4f * v2 - v3;
                float R = -v0 + v2;
                float S = 2f * v1;

                return 0.5f * (P * t3 + Q * t2 + R * t + S);
            }
        }
    }
}
