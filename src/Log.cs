using System;

namespace maps
{
    public static class Log
    {
#if UNITY_5_3_OR_NEWER
        private static ILogger _backend = new UnityLogger(); // default fallback
#else
        private static ILogger _backend = new FileLogger(); // default fallback
#endif

        static Log()
        {
            CombatLog.SetHandler(ForwardToBackend);
        }

        public static void SetBackend(ILogger logger)
        {
            _backend = logger ?? throw new ArgumentNullException(nameof(logger));
            CombatLog.SetHandler(ForwardToBackend);
        }

        public static void Info(string message) => CombatLog.Info(message);
        public static void Warning(string message) => CombatLog.Warning(message);
        public static void Error(string message) => CombatLog.Error(message);

        public static void LogException(Exception exception) => CombatLog.LogException(exception);

        private static void ForwardToBackend(CombatLogLevel level, string message, Exception? exception)
        {
            switch (level)
            {
                case CombatLogLevel.Info:
                    _backend.Log(message);
                    break;
                case CombatLogLevel.Warning:
                    _backend.Warn(message);
                    break;
                case CombatLogLevel.Error:
                    _backend.Error(message);
                    break;
                default:
                    _backend.Log(message);
                    break;
            }

            if (exception != null)
            {
                _backend.LogException(exception);
            }
        }
    }

    internal class FileLogger : ILogger
    {
        public void Log(string message)
        {
            
        }

        public void Warn(string message)
        {

        }

        public void Error(string message)
        {

        }

        public void LogException(Exception exception)
        {

        }
    }
}
