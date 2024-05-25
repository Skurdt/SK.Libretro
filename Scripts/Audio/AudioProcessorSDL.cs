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

namespace SK.Libretro
{
    internal sealed class AudioProcessorSDL : IAudioProcessor
    {
        private uint _audioDeviceId = SDL.AUDIO_DEVICE_DEFAULT_OUTPUT;
        private IntPtr _audioStream = IntPtr.Zero;

        public void Init(int sampleRate)
        {
            Dispose();

            if (SDL.InitSubSystem(SDL.INIT_AUDIO) != 0)
            {
                Dispose();
                return;
            }

            if (SDL.GetAudioDeviceFormat(_audioDeviceId, out SDL.AudioSpec destAudioSpec, out int _) != 0)
            {
                Dispose();
                return;
            }

            destAudioSpec.channels = 2;
            destAudioSpec.format = SDL.AudioFormat.S16;

            _audioDeviceId = SDL.OpenAudioDevice(_audioDeviceId, ref destAudioSpec);
            if (_audioDeviceId == 0)
            {
                Dispose();
                return;
            }

            SDL.AudioSpec srcAudioSpec = new() { channels = 2, format = SDL.AudioFormat.S16, freq = sampleRate };
            _audioStream = SDL.CreateAudioStream(ref srcAudioSpec, ref destAudioSpec);
            if (SDL.BindAudioStream(_audioDeviceId, _audioStream) != 0)
            {
                Dispose();
                return;
            }

            if (SDL.ResumeAudioDevice(_audioDeviceId) != 0)
            {
                Dispose();
                return;
            }
        }

        public void Dispose()
        {
            if (_audioStream.IsNotNull())
            {
                SDL.UnbindAudioStream(_audioStream);
                SDL.DestroyAudioStream(_audioStream);
            }

            if (_audioDeviceId != 0)
                SDL.CloseAudioDevice(_audioDeviceId);

            SDL.QuitSubSystem(SDL.INIT_AUDIO);
        }

        public void ProcessSample(short left, short right)
        {
        }

        public void ProcessSampleBatch(IntPtr data, nuint frames)
        {
            if (_audioDeviceId == 0)
                return;

            int numSamples = (int)(frames * 2 * sizeof(short));
            _ = SDL.PutAudioStreamData(_audioStream, data, numSamples);
        }
    }
}
