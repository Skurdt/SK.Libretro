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

namespace SK.Libretro
{
    internal static class LogLevelEnumExtensions
    {
        public static retro_log_level ToRetroLogLevel(this LogLevel logLevel) => logLevel switch
        {
            LogLevel.Info      => retro_log_level.RETRO_LOG_INFO,
            LogLevel.Warning   => retro_log_level.RETRO_LOG_WARN,
            LogLevel.Error     => retro_log_level.RETRO_LOG_ERROR,
            LogLevel.Exception => retro_log_level.RETRO_LOG_ERROR,
            LogLevel.Debug
            or _ => retro_log_level.RETRO_LOG_DEBUG
        };

        public static LogLevel ToLogLevel(this retro_log_level logLevel) => logLevel switch
        {
            retro_log_level.RETRO_LOG_INFO  => LogLevel.Info,
            retro_log_level.RETRO_LOG_WARN  => LogLevel.Warning,
            retro_log_level.RETRO_LOG_ERROR => LogLevel.Error,
            retro_log_level.RETRO_LOG_DEBUG
            or _ => LogLevel.Debug
        };

        public static string ToStringFast(this LogLevel logLevel) => logLevel switch
        {
            LogLevel.Info      => "Info",
            LogLevel.Warning   => "Warning",
            LogLevel.Error     => "Error",
            LogLevel.Exception => "Exception",
            LogLevel.Debug     => "Debug",
            _ => "NotImplemented"
        };

        public static string ToStringUpperFast(this LogLevel logLevel) => logLevel switch
        {
            LogLevel.Info      => "INFO",
            LogLevel.Warning   => "WARNING",
            LogLevel.Error     => "ERROR",
            LogLevel.Exception => "EXCEPTION",
            LogLevel.Debug     => "DEBUG",
            _ => "NOT_IMPLEMENTED"
        };
    }
}
