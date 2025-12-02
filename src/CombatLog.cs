#nullable enable
using System;
using System.Threading;

namespace CombatEngine.Core.Model
{
    public enum CombatLogLevel
    {
        Info,
        Warning,
        Error,
    }

    public delegate void CombatLogHandler(CombatLogLevel level, string message, Exception? exception);

    public static class CombatLog
    {
        private static CombatLogHandler _handler = static (_, _, _) => { };

        public static void SetHandler(CombatLogHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Volatile.Write(ref _handler, handler);
        }

        public static void Info(string message) => Log(CombatLogLevel.Info, message, exception: null);
        public static void Warning(string message) => Log(CombatLogLevel.Warning, message, exception: null);
        public static void Error(string message) => Log(CombatLogLevel.Error, message, exception: null);

        public static void LogException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Log(CombatLogLevel.Error, exception.Message, exception);
        }

        private static void Log(CombatLogLevel level, string message, Exception? exception)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "(no message provided)";
            }

            var handler = Volatile.Read(ref _handler);
            handler(level, message, exception);
        }
    }
}
