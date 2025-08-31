namespace Markwardt;

public interface ILogger
{
    void Log(object? message, bool isError = false);
}

public static class LoggerExtensions
{
    public static void LogError(this ILogger logger, object? message)
        => logger.Log(message, true);
}

public class Logger : ILogger
{
    public void Log(object? message, bool isError = false)
        => Console.WriteLine($"{(isError ? "[ERROR] " : string.Empty)}{message ?? "NULL"}");
}