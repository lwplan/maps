using System;

namespace ConsoleApp3.Core
{
public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine("[LOG] " + message);
    public void Warn(string message) => Console.WriteLine("[WARN] " + message);
    public void Error(string message) => Console.WriteLine("[ERROR] " + message);
    public void LogException(Exception exception) => Console.WriteLine("[Exception] " + exception.Message);
}
}
