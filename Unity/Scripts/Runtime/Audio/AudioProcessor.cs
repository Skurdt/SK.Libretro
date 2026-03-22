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
        private float _rateControlPitch;
        private float _rateControlPitchFactor;
        private double _sampleRatio;

        private AudioSource _audioSource;
        private NativeRingQueue<float> _circularBuffer;
        private AudioResampler _resampler;

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_bufferIsPrimed || _circularBuffer.Length == 0)
                return;

            var pitch = RateControl();
            var stepSize = _sampleRatio * pitch;

            _resampler.Resample(data, channels, stepSize);
        }

        private void OnDestroy() => Dispose();

        public async void Init(int sampleRate, double fps)
        {
            await Awaitable.MainThreadAsync();

            if (_audioSource)
                _audioSource.Stop();

            var inputSampleRate = sampleRate;
            var outputSampleRate = AudioSettings.outputSampleRate;
            _sampleRatio = (double)inputSampleRate / outputSampleRate;

            var config = AudioSettings.GetConfiguration();
            var dspBufferSize = config.dspBufferSize;
            var dspBufferInputSize = Mathf.CeilToInt(dspBufferSize * (float)_sampleRatio) * N_INPUT_CHANNELS;

            _bufferFillTarget = dspBufferInputSize * 2;
            _bufferDeadzone = Mathf.CeilToInt(inputSampleRate / (float)fps) * N_INPUT_CHANNELS;
            _rateControlPitchFactor = inputSampleRate / 1e9f;
            _rateControlPitch = 1.0f;

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

            var sourceSamples = (short*)data;
            var sourceSamplesCount = (int)frames * N_INPUT_CHANNELS;

            if (_circularBuffer.Length + sourceSamplesCount > _circularBuffer.Capacity)
                return;

            for (var i = 0; i < sourceSamplesCount; i++)
                _circularBuffer.Enqueue(sourceSamples[i] * AudioHandler.NORMALIZED_GAIN);

            UpdateBufferStatus();
        }

        private void InitBuffer(int sampleRate, int channels, float bufferDurationSeconds)
        {
            var bufferSize = (int)(sampleRate * channels * bufferDurationSeconds);
            if (_circularBuffer.IsCreated)
                _circularBuffer.Dispose();

            _circularBuffer = new(bufferSize, Allocator.Persistent);
            _bufferIsPrimed = false;
        }

        private float RateControl()
        {
            var sampleDifference = _circularBuffer.Length - _bufferFillTarget;

            if (sampleDifference > _bufferDeadzone)
                sampleDifference -= _bufferDeadzone;
            else if (sampleDifference < -_bufferDeadzone)
                sampleDifference += _bufferDeadzone;
            else
                return _rateControlPitch = 1.0f;

            var targetPitch = 1.0f + Mathf.Clamp(sampleDifference * _rateControlPitchFactor, -0.05f, 0.05f);
            return _rateControlPitch = Mathf.Lerp(_rateControlPitch, targetPitch, 0.1f);
        }

        private void UpdateBufferStatus()
        {
            if (!_bufferIsPrimed) _bufferIsPrimed = _circularBuffer.Length >= _bufferFillTarget - _bufferDeadzone;
        }

        private class AudioResampler
        {
            private NativeRingQueue<float> _circularBuffer;
            private double _fractionalPosition = 0;
            private float _l0 = 0f, _l1 = 0f, _l2 = 0f, _l3 = 0f;
            private float _r0 = 0f, _r1 = 0f, _r2 = 0f, _r3 = 0f;

            public AudioResampler(NativeRingQueue<float> circularBuffer) => _circularBuffer = circularBuffer;

            public void Resample(float[] data, int channels, double stepSize)
            {
                for (var i = 0; i < data.Length; i += channels)
                {
                    while (_fractionalPosition >= 1.0)
                    {
                        _l0 = _l1;
                        _l1 = _l2;
                        _l2 = _l3;
                        _r0 = _r1;
                        _r1 = _r2;
                        _r2 = _r3;

                        if (_circularBuffer.TryDequeue(out var newL) && _circularBuffer.TryDequeue(out var newR))
                        {
                            _l3 = newL;
                            _r3 = newR;
                        }
                        else
                        {
                            _l3 = 0f;
                            _r3 = 0f;
                        }

                        _fractionalPosition -= 1.0;
                    }
    
                    var t = (float)_fractionalPosition;
                    data[i] = CubicInterpolate(_l0, _l1, _l2, _l3, t);

                    if (channels > 1)
                        data[i + 1] = CubicInterpolate(_r0, _r1, _r2, _r3, t);

                    _fractionalPosition += stepSize;
                }
            }

            private static float CubicInterpolate(float v0, float v1, float v2, float v3, float t)
            {
                var t2 = t * t;
                var t3 = t2 * t;

                var P = -v0 + (3f * v1) - (3f * v2) + v3;
                var Q = (2f * v0) - (5f * v1) + (4f * v2) - v3;
                var R = -v0 + v2;
                var S = 2f * v1;

                return 0.5f * ((P * t3) + (Q * t2) + (R * t) + S);
            }
        }
    }
}
