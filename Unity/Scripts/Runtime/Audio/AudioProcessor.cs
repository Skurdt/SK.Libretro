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
using Unity.Jobs;
using UnityEngine;

namespace SK.Libretro.Unity
{
    [RequireComponent(typeof(AudioSource)), DisallowMultipleComponent]
    internal sealed class AudioProcessor: MonoBehaviour, IAudioProcessor
    {
        private const int AUDIO_BUFFER_SIZE = 65536;

        private AudioSource _audioSource;
        private NativeList<float> _audioBufferList;

        private NativeArray<float> _samples;
        private JobHandle _jobHandle;

        private void OnDestroy() => Dispose();

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_audioBufferList.IsCreated || _audioBufferList.Length < data.Length)
                return;

            for (int i = 0; i < data.Length; ++i)
                data[i] = _audioBufferList[i];
            _audioBufferList.RemoveRange(0, data.Length);
        }

        public void Init(int sampleRate) => MainThreadDispatcher.Enqueue(() =>
        {
            if (!_audioSource)
                _audioSource = GetComponent<AudioSource>();
            _audioSource.Stop();

            if (_samples.IsCreated)
                _samples.Dispose();

            if (_audioBufferList.IsCreated)
                _audioBufferList.Dispose();
            _audioBufferList = new(AUDIO_BUFFER_SIZE, Allocator.Persistent);

            AudioConfiguration audioConfig = AudioSettings.GetConfiguration();
            audioConfig.sampleRate = sampleRate;
            _ = AudioSettings.Reset(audioConfig);

            _audioSource.Play();
        });

        public void Dispose() => MainThreadDispatcher.Enqueue(() =>
        {
            if (_audioSource)
                _audioSource.Stop();

            if (!_jobHandle.IsCompleted)
                _jobHandle.Complete();

            if (_samples.IsCreated)
                _samples.Dispose();

            if (_audioBufferList.IsCreated)
                _audioBufferList.Dispose();
        });

        public void ProcessSample(short left, short right) => MainThreadDispatcher.Enqueue(() =>
        {
            CreateBuffer(2);
            _samples[0] = left * AudioHandler.NORMALIZED_GAIN;
            _samples[1] = right * AudioHandler.NORMALIZED_GAIN;
            _audioBufferList.AddRange(_samples);
        });

        public unsafe void ProcessSampleBatch(IntPtr data, nuint frames) => MainThreadDispatcher.Enqueue(() =>
        {
            int numFrames = (int)frames * 2;
            CreateBuffer(numFrames);
            _jobHandle = new SampleBatchJob
            {
                SourceSamples      = (short*)data,
                DestinationSamples = _samples
            }.Schedule(numFrames, 64);
            _jobHandle.Complete();
            _audioBufferList.AddRange(_samples);
        });

        private void CreateBuffer(int numFrames)
        {
            if (_samples.Length == numFrames)
                return;

            if (_samples.IsCreated)
                _samples.Dispose();

            _samples = new NativeArray<float>(numFrames, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
    }
}
