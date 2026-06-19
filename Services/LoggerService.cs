using System.IO;

namespace POS.Services;

public static class LoggerService
{
    private static readonly string LogPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "log.txt");

    private static readonly object _lock = new();

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message, Exception? ex = null)
    {
        Write("ERROR", message);
        if (ex != null)
            Write("ERROR", $"  Exception: {ex.GetType().Name}: {ex.Message}");
        if (ex?.InnerException != null)
            Write("ERROR", $"  Inner: {ex.InnerException.Message}");
        if (ex?.StackTrace != null)
            Write("ERROR", $"  StackTrace: {ex.StackTrace}");
    }

    private static void Write(string level, string message)
    {
        try
        {
            lock (_lock)
            {
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}");
            }
        }
        catch { /* never let logging crash the app */ }
    }
}
