using System;
using System.IO;

namespace ConsoleApp3.Core
{
public class FileLogger : ILogger
{
    private readonly string _logFilePath;

    public FileLogger(string path = "app.log")
    {
        _logFilePath = path;
        File.WriteAllText(_logFilePath, string.Empty); // clear on start
    }

    private void Write(string level, string message)
    {
        File.AppendAllText(_logFilePath, $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}{Environment.NewLine}");
    }

    public void Log(string message) => Write("LOG", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);
    public void LogException(Exception ex) => Write("EXCEPTION", ex.ToString());
}
}
