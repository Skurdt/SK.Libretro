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

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using Unity.Mathematics;

namespace SK.Libretro.NAudio
{
    internal sealed class AudioProcessor : IAudioProcessor
    {
        private const int AUDIO_BUFFER_SIZE = 65536;

        private WaveOutEvent _audioDevice;
        private BufferedWaveProvider _bufferedWaveProvider;
        private VolumeSampleProvider _volumeProvider;

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
                    DesiredLatency = 140
                };

                _audioDevice.Init(_volumeProvider);
                _audioDevice.Play();
            }
            catch (Exception e)
            {
                Utilities.Logger.LogException(e);
            }
        }

        public void DeInit()
        {
            if (_audioDevice == null)
                return;

            _audioDevice.Stop();
            _audioDevice.Dispose();
            _bufferedWaveProvider.ClearBuffer();
        }

        public void ProcessSamples(float[] samples)
        {
            if (_bufferedWaveProvider == null)
                return;

            byte[] byteBuffer = new byte[samples.Length * sizeof(float)];
            Buffer.BlockCopy(samples, 0, byteBuffer, 0, byteBuffer.Length);
            _bufferedWaveProvider.AddSamples(byteBuffer, 0, byteBuffer.Length);
        }

        public void SetVolume(float volume)
        {
            if (_volumeProvider != null)
                _volumeProvider.Volume = math.clamp(volume, 0f, 1f);
        }
    }
}
#endif
