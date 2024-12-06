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
    internal sealed class MessageHandler : IDisposable
    {
        private const int VERSION = 1;

        private readonly IMessageProcessor _processor;

        public MessageHandler(IMessageProcessor processor) => _processor = processor ?? new NullMessageProcessor();

        public void Dispose() => _processor.Dispose();

        public bool GetMessageInterfaceVersion(IntPtr data)
        {
            if (data.IsNotNull())
                data.Write(VERSION);
            return true;
        }

        public bool SetMessage(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_message retroMessage = data.ToStructure<retro_message>();
            string message = retroMessage.msg.AsString();
            LogConsole(message);
            LogOSD(message, retroMessage.frames / 60);
            return true;
        }

        public bool SetMessageExt(IntPtr data)
        {
            if (data.IsNull())
                return false;

            retro_message_ext retroMessage = data.ToStructure<retro_message_ext>();
            string message = retroMessage.msg.AsString();
            switch (retroMessage.target)
            {
                case retro_message_target.RETRO_MESSAGE_TARGET_ALL:
                    LogConsole(message);
                    LogOSD(message, ref retroMessage);
                    break;
                case retro_message_target.RETRO_MESSAGE_TARGET_OSD:
                    LogOSD(message, ref retroMessage);
                    break;
                case retro_message_target.RETRO_MESSAGE_TARGET_LOG:
                    LogConsole(message);
                    break;
                default:
                    break;
            }

            return true;
        }

        private void LogConsole(string message) =>
            Wrapper.Instance.LogHandler.LogInfo(message);

        private void LogOSD(string message, uint seconds) =>
            _processor.ShowNotification(message, seconds, LogLevel.Info, 0);

        private void LogOSD(string message, ref retro_message_ext retroMessage)
        {
            switch (retroMessage.type)
            {
                case retro_message_type.RETRO_MESSAGE_TYPE_NOTIFICATION:
                    _processor.ShowNotification(message, retroMessage.duration / 1000, retroMessage.level.ToLogLevel(), retroMessage.priority);
                    break;
                case retro_message_type.RETRO_MESSAGE_TYPE_NOTIFICATION_ALT:
                    _processor.ShowNotificationAlt(message, retroMessage.duration / 1000, retroMessage.level.ToLogLevel(), retroMessage.priority);
                    break;
                case retro_message_type.RETRO_MESSAGE_TYPE_STATUS:
                    _processor.ShowStatus(message);
                    break;
                case retro_message_type.RETRO_MESSAGE_TYPE_PROGRESS:
                    _processor.ShowProgress(message, retroMessage.progress);
                    break;
                default:
                    break;
            }
        }
    }
}
