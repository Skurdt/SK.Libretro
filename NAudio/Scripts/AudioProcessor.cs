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

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Runtime.CompilerServices;

#if UNITY_EDITOR || UNITY_STANDALONE
[assembly: InternalsVisibleTo("SK.Libretro.Unity")]
#endif

namespace SK.Libretro.NAudio
{
    internal sealed class AudioProcessor : IAudioProcessor
    {
        private const int AUDIO_BUFFER_SIZE = 65536;

        private IWavePlayer _audioDevice;
        private BufferedWaveProvider _bufferedWaveProvider;
        private VolumeSampleProvider _volumeProvider;

        private byte[] _byteBuffer = new byte[0];

        public void Init(int sampleRate)
        {
            try
            {
                DeInit();

                WaveFormat audioFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate > 0 ? sampleRate : 44100, 2);
                _bufferedWaveProvider  = new BufferedWaveProvider(audioFormat)
                {
                    DiscardOnBufferOverflow = true,
                    BufferLength            = AUDIO_BUFFER_SIZE
                };

                _volumeProvider = new VolumeSampleProvider(_bufferedWaveProvider.ToSampleProvider())
                {
                    Volume = 1f
                };

                _audioDevice = new WaveOutEvent
                {
                    DesiredLatency = 120
                };

                _audioDevice.Init(_volumeProvider);
                _audioDevice.Play();
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e);
            }
        }

        public void DeInit()
        {
            if (_audioDevice is null)
                return;

            _audioDevice.Stop();
            _audioDevice.Dispose();
            _audioDevice = null;

            _volumeProvider = null;

            _bufferedWaveProvider.ClearBuffer();
            _bufferedWaveProvider = null;
        }

        public void ProcessSamples(float[] samples)
        {
            if (_bufferedWaveProvider is null)
                return;

            int bufferLength = samples.Length * sizeof(float);

            if (_byteBuffer.Length != bufferLength)
                _byteBuffer = new byte[bufferLength];

            Buffer.BlockCopy(samples, 0, _byteBuffer, 0, _byteBuffer.Length);
            _bufferedWaveProvider.AddSamples(_byteBuffer, 0, _byteBuffer.Length);
        }

        public void SetVolume(float volume)
        {
            if (_volumeProvider is not null)
                _volumeProvider.Volume = volume.Clamp(0f, 1f);
        }
    }
}
