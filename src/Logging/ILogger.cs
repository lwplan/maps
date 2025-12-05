using System;
namespace maps
{
    public interface ILogger
    {
        void Log(string message);
        void Warn(string message);
        void Error(string message);
        void LogException(Exception exception);
    }
}
