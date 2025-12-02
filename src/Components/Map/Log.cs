using System;

namespace GameBase.UI.Components.Map
{
    public static class Log
    {
        public static void Info(string message) => Console.WriteLine($"[INFO] {message}");
        public static void Warning(string message) => Console.WriteLine($"[WARN] {message}");
        public static void Error(string message) => Console.Error.WriteLine($"[ERROR] {message}");
        public static void Debug(string message) => Console.WriteLine($"[DEBUG] {message}");
    }
}
