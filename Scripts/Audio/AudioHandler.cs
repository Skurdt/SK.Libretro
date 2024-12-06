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

using SK.Libretro.Header;
using System;

namespace SK.Libretro
{
    internal sealed class AudioHandler : IDisposable
    {
        public const float NORMALIZED_GAIN = 1f / 0x8000;

        public bool Enabled { get; set; }

        private static readonly retro_audio_sample_t _sampleCallback            = SampleCallback;
        private static readonly retro_audio_sample_batch_t _sampleBatchCallback = SampleBatchCallback;

        private readonly IAudioProcessor _processor;

        private retro_audio_callback_t _audioCallback;
        private retro_audio_set_state_callback_t _audioCallbackSetState;
        private retro_audio_buffer_status_callback_t _audioBufferStatusCallback;
        private uint _minimumLatency;

        public AudioHandler(IAudioProcessor audioProcessor) => _processor = audioProcessor ?? new NullAudioProcessor();

        public void Init(bool enabled)
        {
            Enabled = enabled;
            _processor.Init(Wrapper.Instance.Game.SystemAVInfo.SampleRate);
        }

        public void Dispose() => _processor.Dispose();

        public void SetCoreCallbacks(retro_set_audio_sample_t setAudioSample, retro_set_audio_sample_batch_t setAudioSampleBatch)
        {
            setAudioSample(_sampleCallback);
            setAudioSampleBatch(_sampleBatchCallback);
        }

        public bool SetAudioCallback(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_audio_callback callback = data.ToStructure<retro_audio_callback>();
            _audioCallback                = callback.callback.GetDelegate<retro_audio_callback_t>();
            _audioCallbackSetState        = callback.set_state.GetDelegate<retro_audio_set_state_callback_t>();
            return true;
        }

        public bool SetAudioBufferStatusCallback(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_audio_buffer_status_callback bufferStatusCallback = data.ToStructure<retro_audio_buffer_status_callback>();
            _audioBufferStatusCallback = bufferStatusCallback.callback.GetDelegate<retro_audio_buffer_status_callback_t>();
            return true;
        }

        public bool SetMinimumAudioLatency(IntPtr data)
        {
            if (data.IsNull())
                return false;

            _minimumLatency = data.ReadUInt32();
            return true;
        }

        [MonoPInvokeCallback(typeof(retro_audio_sample_t))]
        private static void SampleCallback(short left, short right)
        {
            if (Wrapper.Instance.AudioHandler.Enabled)
                Wrapper.Instance.AudioHandler._processor.ProcessSample(left, right);
        }

        [MonoPInvokeCallback(typeof(retro_audio_sample_batch_t))]
        private static nuint SampleBatchCallback(IntPtr data, nuint frames)
        {
            if (Wrapper.Instance.AudioHandler.Enabled)
                Wrapper.Instance.AudioHandler._processor.ProcessSampleBatch(data, frames);
            return frames;
        }
    }
}
