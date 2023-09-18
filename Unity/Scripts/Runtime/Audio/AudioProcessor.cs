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

using NAudio.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SK.Libretro.Unity
{
    public class CircularBuffer<T>
    {
        private T[] buffer;
        private int head;
        private int tail;

        public CircularBuffer(int capacity)
        {
            buffer = new T[capacity];
            head = 0;
            tail = 0;
        }

        public int Count
        {
            get { return tail >= head ? tail - head : buffer.Length - (head - tail); }
        }

        public int Capacity
        {
            get { return buffer.Length; }
        }

        public void Enqueue(T item)
        {
            buffer[tail] = item;
            tail = (tail + 1) % buffer.Length;
            if (tail == head)
            {
                head = (head + 1) % buffer.Length;
            }
        }

        public T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("Buffer is empty");
            T item = buffer[head];
            head = (head + 1) % buffer.Length;
            return item;
        }

        public void Clear()
        {
            head = 0;
            tail = 0;
        }

        public T this[int index]
        {
            get { return buffer[(head + index) % buffer.Length]; }
            set { buffer[(head + index) % buffer.Length] = value; }
        }
    }

    [RequireComponent(typeof(AudioSource)), DisallowMultipleComponent]
    internal sealed class AudioProcessor: MonoBehaviour, IAudioProcessor
    {
        private const int AUDIO_BUFFER_SIZE = 262144;
        private const int CIRCULAR_BUFFER_SIZE = AUDIO_BUFFER_SIZE * 2;

        private readonly List<float> _audioBufferList = new List<float>(AUDIO_BUFFER_SIZE);
        private CircularBuffer<float> _circularBuffer = new CircularBuffer<float>(CIRCULAR_BUFFER_SIZE);

        private AudioSource _audioSource;
        private int _inputSampleRate;
        private int _outputSampleRate;

        private JobHandle _jobHandle;
        private NativeArray<float> _samples;

        private void OnDestroy() => Dispose();

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (_circularBuffer.Count < data.Length)
                return;

            // Read samples from circular buffer
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = _circularBuffer.Dequeue();
            }
        }


        public void Init(int sampleRate) => MainThreadDispatcher.Enqueue(() =>
        {
            Debug.Log($"Set sample to {sampleRate}");
            _inputSampleRate  = sampleRate;
            _outputSampleRate = AudioSettings.outputSampleRate;

            if (!_audioSource)
                _audioSource = GetComponent<AudioSource>();
            _audioSource.Stop();

            if (_samples.IsCreated)
                _samples.Dispose();

            _circularBuffer.Clear();
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
        });

        public void ProcessSample(short left, short right) => MainThreadDispatcher.Enqueue(() =>
        {
            CreateBuffer(2);
            _samples[0] = left * AudioHandler.NORMALIZED_GAIN;
            _samples[1] = right * AudioHandler.NORMALIZED_GAIN;
            _audioBufferList.AddRange(_samples);
        });

        public unsafe void ProcessSampleBatch(IntPtr data, nuint frames)
        {
            if ((_inputSampleRate == 0) || (_outputSampleRate == 0))
                return;

            if (!_jobHandle.IsCompleted)
                _jobHandle.Complete();
            //Debug.Log($"Sample Rate: in:{_inputSampleRate}Hz out:{_outputSampleRate}Hz");
            double ratio = (double)_outputSampleRate / (double)_inputSampleRate;
            int numSourceFrames = (int)frames * 2;
            int numDestFrames = (int)((double)(numSourceFrames) * ratio);
            //Debug.Log($"create buffer of {numDestFrames} from {frames} {ratio}");
            CreateBuffer(numDestFrames);

            Resampler.LinearResample((short*)data, numSourceFrames, _samples, numDestFrames, _inputSampleRate, _outputSampleRate);

            // Add samples to circular buffer
            for (int i = 0; i < numDestFrames; i++)
            {
                _circularBuffer.Enqueue(_samples[i]);
            }
        }

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
