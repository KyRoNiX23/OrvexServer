using System.Globalization;

namespace Orvex.Server.Logging;

public enum LogLevel
{
    Info,
    Warning,
    Error,
    Debug,
    Success
}

public static class ServerLogger
{
    private static readonly object Sync = new();
    private static readonly string LogDirectory = "logs";

    private static string LogFile =>
        Path.Combine(LogDirectory, $"latest.log");

    public static void Initialize()
    {
        Directory.CreateDirectory(LogDirectory);
    }

    public static void Info(string message) =>
        Write(LogLevel.Info, message);

    public static void Warning(string message) =>
        Write(LogLevel.Warning, message);

    public static void Error(string message) =>
        Write(LogLevel.Error, message);

    public static void Debug(string message) =>
        Write(LogLevel.Debug, message);

    public static void Success(string message) =>
        Write(LogLevel.Success, message);

    private static void Write(LogLevel level, string message)
    {
        lock (Sync)
        {
            string timestamp = DateTime.Now.ToString(
                "HH:mm:ss",
                CultureInfo.InvariantCulture);

            string levelName = level.ToString().ToUpperInvariant();

            string logMessage =
                $"[{timestamp}] [{levelName}] {message}";

            ConsoleColor previousColor = Console.ForegroundColor;

            Console.ForegroundColor = level switch
            {
                LogLevel.Info => ConsoleColor.Gray,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Success => ConsoleColor.Green,
                _ => ConsoleColor.White
            };

            Console.WriteLine(logMessage);
            Console.ForegroundColor = previousColor;

            File.AppendAllText(
                LogFile,
                logMessage + Environment.NewLine);
        }
    }
}