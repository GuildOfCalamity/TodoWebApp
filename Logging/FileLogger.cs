using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace TodoWebApp.Logging
{
    internal class FileLogger : ILogger
    {
        private static readonly object _lock = new();
        private readonly string _filePath;
        private readonly string _category;

        public FileLogger(string filePath, string category)
        {
            _filePath = filePath;
            _category = category;
        }

        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter == null)
                return;

            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message) && exception == null)
                return;

            var logRecord = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {message}";
            if (exception != null)
                logRecord += Environment.NewLine + exception;

            lock (_lock)
            {
                try
                {
                    // ensure directory exists
                    var dir = Path.GetDirectoryName(_filePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.AppendAllText(_filePath, logRecord + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Failed to log to file: {ex.Message}");
                }
            }
        }
    }
}