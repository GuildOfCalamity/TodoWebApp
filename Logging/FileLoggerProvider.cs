using System;
using Microsoft.Extensions.Logging;

namespace TodoWebApp.Logging
{
    internal class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(_filePath, categoryName);

        public void Dispose() { /* best practices */ }
    }
}