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
using System.Collections.Generic;

namespace SK.Libretro.Utilities
{
    internal sealed class Logger
    {
        public static Logger Instance => _instance ?? new Logger();
        private static Logger _instance;

        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Exception
        }

        private sealed class LogStringHandler
        {
            private readonly string _prefix;
            private readonly Action<string> _logAction;
            private readonly bool _colorSupport;
            private readonly string _color;

            public LogStringHandler(LogLevel logLevel, Action<string> logAction, bool colorSupport = false)
            {
                string logLevelString = logLevel.ToString().ToUpper();

                _color = logLevel switch
                {
                    LogLevel.Debug     => "white",
                    LogLevel.Info      => "yellow",
                    LogLevel.Warning   => "orange",
                    LogLevel.Error     => "red",
                    LogLevel.Exception => "red",
                    _ => throw new NotImplementedException(),
                };

                _prefix       = colorSupport ? $"[<color={_color}>{logLevelString}</color>]" : $"[{logLevelString}]";
                _logAction    = logAction;
                _colorSupport = colorSupport;
            }

            public void Log(string message, string caller = null)
            {
                caller = !string.IsNullOrEmpty(caller) ? (_colorSupport ? $"<color=lightblue>[{caller}]</color> " : $"[{caller}] ") : "";
                _logAction?.Invoke($"{_prefix} {caller}{message}");
            }
        }

        private readonly Dictionary<LogLevel, List<LogStringHandler>> _loggers = new Dictionary<LogLevel, List<LogStringHandler>>();
        private readonly List<Action<Exception>> _exceptionLoggers = new List<Action<Exception>>();

        private Logger() => _instance = this;

        public void AddDebughandler(Action<string> function, bool colorSupport = false) => AddHandler(LogLevel.Debug, function, colorSupport);
        public void AddInfoHandler(Action<string> function, bool colorSupport = false) => AddHandler(LogLevel.Info, function, colorSupport);
        public void AddWarningHandler(Action<string> function, bool colorSupport = false) => AddHandler(LogLevel.Warning, function, colorSupport);
        public void AddErrorhandler(Action<string> function, bool colorSupport = false) => AddHandler(LogLevel.Error, function, colorSupport);
        public void AddExceptionHandler(Action<Exception> function) => AddHandler(function);

        public void LogDebug(string message, string caller = null) => LogInternal(LogLevel.Debug, message, caller);
        public void LogInfo(string message, string caller = null) => LogInternal(LogLevel.Info, message, caller);
        public void LogWarning(string message, string caller = null) => LogInternal(LogLevel.Warning, message, caller);
        public void LogError(string message, string caller = null) => LogInternal(LogLevel.Error, message, caller);
        public void LogException(Exception exception) => LogInternal(exception);

        private void AddHandler(LogLevel logLevel, Action<string> function, bool colorSupport)
        {
            if (!_loggers.ContainsKey(logLevel))
                _loggers.Add(logLevel, new List<LogStringHandler>());
            _loggers[logLevel].Add(new LogStringHandler(logLevel, function, colorSupport));
        }

        private void AddHandler(Action<Exception> function)
        {
            if (!_exceptionLoggers.Contains(function))
                _exceptionLoggers.Add(function);
        }

        private void LogInternal(LogLevel level, string message, string caller)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (!_loggers.TryGetValue(level, out List<LogStringHandler> stringHandlers))
                return;

            foreach (LogStringHandler stringHandler in stringHandlers)
                stringHandler.Log(message, caller);
        }

        private void LogInternal(Exception e)
        {
            if (e == null || _exceptionLoggers == null)
                return;

            foreach (Action<Exception> exceptionLogger in _exceptionLoggers)
                exceptionLogger.Invoke(e);
        }
    }
}
