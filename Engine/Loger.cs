using System.Collections.Concurrent;
using System.Text;

namespace Engine
{
    public static class Loger
    {
        private static readonly ConcurrentQueue<string> _logQueue = new();
        private static readonly StringBuilder _textBuffer = new();

        private static string _filename = "log.txt";

        private static readonly object _fileLock = new();
        private static readonly int _flushTimeout = 5000;
        private static readonly int _maxBufferSize = 10000;

        private static bool _isRunning = true;
        private static readonly CancellationTokenSource _token;

        static Loger()
        {
            _token = new();
            _ = Task.Run(FlushLogs, _token.Token);
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown();
        }

        /// <summary>
        /// Sets filename for logs
        /// </summary>
        /// <param name="filename">Имя файла</param>
        public static void SetFilename(string filename)
        {
            _filename = string.IsNullOrEmpty(filename) ? "log.txt" : filename;
            Flush();
        }

        /// <summary>
        /// Logs message (<paramref name="message"/>)
        /// </summary>
        public static void Log(object? message)
        {
            if (message == null) return;

            var logEntry = $"[LOG] {DateTime.Now:MM/dd/yy HH:mm:ss:fff} : {message}";
            _logQueue.Enqueue(logEntry);

            if (_textBuffer.Length > _maxBufferSize) Flush();
        }

        /// <summary>
        /// Stops the background task and records the remaining logs
        /// </summary>
        public static void Shutdown()
        {
            _isRunning = false;
            _token.Cancel();
            Flush();
        }

        private static void Flush()
        {
            lock (_fileLock)
            {
                while (_logQueue.TryDequeue(out var logEntry))
                {
                    _textBuffer.AppendLine(logEntry);
                }

                if (_textBuffer.Length > 0)
                {
                    try
                    {
                        using StreamWriter writer = new(_filename);
                        writer.Write(_textBuffer.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to write logs: {ex.Message}");
                    }
                }
            }
        }

        private static async Task FlushLogs()
        {
            while (_isRunning)
            {
                if (_token.Token.IsCancellationRequested) return;

                await Task.Delay(_flushTimeout);
                Flush();
            }
        }
    }
}