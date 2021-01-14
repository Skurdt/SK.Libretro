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

using System;

namespace SK.Libretro.Utilities
{
    internal static class Logger
    {
        public static bool ColorSupport
        {
            get => _colorSupport;
            set
            {
                _colorSupport = value;

                if (value)
                {
                    _debugPrefix     = "<color=white>[DEBUG]</color>";
                    _infoPrefix      = "<color=yellow>[INFO]</color>";
                    _warningPrefix   = "<color=orange>[WARNING]</color>";
                    _errorPrefix     = "<color=red>[ERROR]</color>";
                    _exceptionPrefix = "<color=red>[EXCEPTION]</color>";
                }
                else
                {
                    _debugPrefix     = "[DEBUG]";
                    _infoPrefix      = "[INFO]";
                    _warningPrefix   = "[WARNING]";
                    _errorPrefix     = "[ERROR]";
                    _exceptionPrefix = "[EXCEPTION]";
                }
            }
        }

        private enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Exception
        }

        private static Action<string> _logInfo    = null;
        private static Action<string> _logWarning = null;
        private static Action<string> _logError   = null;
        private static bool _colorSupport         = false;

        private static string _debugPrefix     = "[DEBUG]";
        private static string _infoPrefix      = "[INFO]";
        private static string _warningPrefix   = "[WARNING]";
        private static string _errorPrefix     = "[ERROR]";
        private static string _exceptionPrefix = "[EXCEPTION]";

        public static void SetLoggers(Action<string> logInfo, Action<string> logWarning, Action<string> logError)
        {
            if (_logInfo != null)
                return;

            _logInfo    = logInfo;
            _logWarning = logWarning;
            _logError   = logError;
        }

        public static void LogDebug(string message, string caller = null) => LogInternal(LogLevel.Debug, message, caller);

        public static void LogInfo(string message, string caller = null) => LogInternal(LogLevel.Info, message, caller);

        public static void LogWarning(string message, string caller = null) => LogInternal(LogLevel.Warning, message, caller);

        public static void LogError(string message, string caller = null) => LogInternal(LogLevel.Error, message, caller);

        public static void LogException(Exception e, string caller = null) => LogInternal(LogLevel.Exception, e.Message, caller);

        private static void LogInternal(LogLevel level, string message, string caller)
        {
            if (!string.IsNullOrEmpty(caller))
                caller = ColorSupport ? $"<color=lightblue>[{caller}]</color> " : $"[{caller}] ";
            else
                caller = "";

            switch (level)
            {
                case LogLevel.Debug:
                    _logInfo?.Invoke($"{_debugPrefix} {caller}{message}");
                    break;
                case LogLevel.Info:
                    _logInfo?.Invoke($"{_infoPrefix} {caller}{message}");
                    break;
                case LogLevel.Warning:
                    _logWarning?.Invoke($"{_warningPrefix} {caller}{message}");
                    break;
                case LogLevel.Error:
                    _logError?.Invoke($"{_errorPrefix} {caller}{message}");
                    break;
                case LogLevel.Exception:
                    _logError?.Invoke($"{_exceptionPrefix} {caller}{message}");
                    break;
            }
        }
    }
}
