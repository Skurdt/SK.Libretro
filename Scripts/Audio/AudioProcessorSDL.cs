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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static SK.Libretro.SDL.Header;

namespace SK.Libretro
{
    internal sealed class AudioProcessorSDL : IAudioProcessor
    {
        private static readonly SDL_AudioStreamCallback _audioCallback = AudioStreamCallback;

        // Static registry for mapping IntPtr handles to AudioProcessorSDL instances
        private static readonly Dictionary<IntPtr, AudioProcessorSDL> _instanceRegistry = new();
        private static long _nextId = 1;

        private readonly short[] _sampleBuffer = new short[2];
        private readonly AudioRingBuffer _ringBuffer = new(64 * 1024);
        private readonly object _lock = new();

        private IntPtr _audioStream = IntPtr.Zero;
        private IntPtr _instanceId = IntPtr.Zero;

        public void Init(int sampleRate, double fps)
        {
            lock (_lock)
            {
                Dispose();

                if (!SDL_InitSubSystem(SDL_INIT_AUDIO))
                {
                    Dispose();
                    return;
                }

                SDL_AudioSpec want = new()
                {
                    channels = 2,
                    format = SDL_AudioFormat.S16,
                    freq = sampleRate
                };

                var callbackPtr = _audioCallback.GetFunctionPointer();

                // Generate a unique IntPtr for this instance
                _instanceId = (IntPtr)System.Threading.Interlocked.Increment(ref _nextId);
                lock (_instanceRegistry)
                {
                    _instanceRegistry[_instanceId] = this;
                }

                _audioStream = SDL_OpenAudioDeviceStream(SDL_AUDIO_DEVICE_DEFAULT_OUTPUT, ref want, callbackPtr, _instanceId);
                if (_audioStream == IntPtr.Zero)
                {
                    Dispose();
                    return;
                }

                if (!SDL_ResumeAudioStreamDevice(_audioStream))
                {
                    Dispose();
                    return;
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _ringBuffer.Clear();

                // Remove from registry if present
                if (_instanceId != IntPtr.Zero)
                {
                    lock (_instanceRegistry)
                    {
                        _ = _instanceRegistry.Remove(_instanceId);
                    }
                    _instanceId = IntPtr.Zero;
                }

                if (_audioStream != IntPtr.Zero)
                {
                    SDL_DestroyAudioStream(_audioStream);
                    _audioStream = IntPtr.Zero;
                }

                SDL_QuitSubSystem(SDL_INIT_AUDIO);
            }
        }

        public void ProcessSample(short left, short right)
        {
            if (_audioStream == IntPtr.Zero || _ringBuffer == null)
                return;

            _sampleBuffer[0] = left;
            _sampleBuffer[1] = right;
            var handle = GCHandle.Alloc(_sampleBuffer, GCHandleType.Pinned);
            try
            {
                ProcessSampleBatch(handle.AddrOfPinnedObject(), 1, default);
            }
            finally
            {
                handle.Free();
            }
        }

        public void ProcessSampleBatch(IntPtr data, nuint frames, in PositionalData positionalData)
        {
            if (_audioStream == IntPtr.Zero || _ringBuffer == null)
                return;

            _ = SDL_LockAudioStream(_audioStream);
            try
            {
                var volume        = 1.0f / (1.0f + positionalData.Distance);
                var sourceAngle   = (float)Math.Atan2(positionalData.Z, positionalData.X);
                var listenerAngle = (float)Math.Atan2(positionalData.ForwardZ, positionalData.ForwardX);
                var relativeAngle = sourceAngle - listenerAngle;
                var pan           = (float)Math.Sin(relativeAngle);
                unsafe
                {
                    var samples = (short*)data.ToPointer();
                    for (nuint i = 0; i < frames * 2; i += 2)
                    {
                        var leftSample = samples[i] * volume * (1.0f - pan);
                        var rightSample = samples[i + 1] * volume * (1.0f + pan);

                        samples[i] = (short)Math.Clamp(leftSample, short.MinValue, short.MaxValue);
                        samples[i + 1] = (short)Math.Clamp(rightSample, short.MinValue, short.MaxValue);
                    }
                }

                var numBytes = (int)(frames * 2 * sizeof(short));
                var managedBuffer = new byte[numBytes];
                Marshal.Copy(data, managedBuffer, 0, numBytes);
                _ = _ringBuffer.Write(managedBuffer, 0, numBytes);
            }
            finally
            {
                _ = SDL_UnlockAudioStream(_audioStream);
            }
        }

        private static void AudioStreamCallback(IntPtr userdata, IntPtr stream, int additional_amount, int total_amount)
        {
            if (userdata == IntPtr.Zero)
                return;

            AudioProcessorSDL processor = null;
            lock (_instanceRegistry)
            {
                _ = _instanceRegistry.TryGetValue(userdata, out processor);
            }
            if (processor == null || processor._ringBuffer == null)
                return;

            var toWrite = additional_amount;
            var temp = new byte[toWrite];
            var read = processor._ringBuffer.Read(temp, 0, toWrite);
            if (read > 0)
            {
                var tempHandle = GCHandle.Alloc(temp, GCHandleType.Pinned);
                try
                {
                    _ = SDL_PutAudioStreamData(stream, tempHandle.AddrOfPinnedObject(), read);
                }
                finally
                {
                    tempHandle.Free();
                }
            }

            if (read < toWrite)
            {
                var silence = new byte[toWrite - read];
                var silenceHandle = GCHandle.Alloc(silence, GCHandleType.Pinned);
                try
                {
                    _ = SDL_PutAudioStreamData(stream, silenceHandle.AddrOfPinnedObject(), toWrite - read);
                }
                finally
                {
                    silenceHandle.Free();
                }
            }
        }

        private class AudioRingBuffer
        {
            private readonly byte[] _buffer;
            private int _readPos, _writePos, _count;
            private readonly object _lock = new();

            public AudioRingBuffer(int size) => _buffer = new byte[size];

            public int Write(byte[] src, int offset, int count)
            {
                lock (_lock)
                {
                    var written = 0;
                    while (written < count && _count < _buffer.Length)
                    {
                        _buffer[_writePos] = src[offset + written];
                        _writePos = (_writePos + 1) % _buffer.Length;
                        _count++;
                        written++;
                    }
                    return written;
                }
            }

            public int Read(byte[] dst, int offset, int count)
            {
                lock (_lock)
                {
                    var read = 0;
                    while (read < count && _count > 0)
                    {
                        dst[offset + read] = _buffer[_readPos];
                        _readPos = (_readPos + 1) % _buffer.Length;
                        _count--;
                        read++;
                    }
                    return read;
                }
            }

            public void Clear()
            {
                lock (_lock)
                {
                    _readPos = 0;
                    _writePos = 0;
                    _count = 0;
                }
            }
        }
    }
}
