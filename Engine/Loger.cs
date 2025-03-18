using System.Text;

namespace Engine
{
    public static class Loger
    {
        private static readonly StringBuilder _text = new();
        private static string _filename = "log.txt";

        /// <summary>
        /// Sets name of file where logs will saves
        /// </summary>
        /// <param name="filename"></param>
        public static void SetFilename(string filename) =>
            _filename = string.IsNullOrEmpty(filename) ? "log.txt" : filename;

        /// <summary>
        /// Logs some kind of message to log file
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object? message)
        {
            if (message == null) return;

            using StreamWriter writer = new(_filename);
            _text.AppendLine($"[LOG] {DateTime.Now:MM/dd/yy HH:mm:ss:fff}" +
                $" : {message.ToString()}");
            writer.Write(_text.ToString());
        }
    }
}
