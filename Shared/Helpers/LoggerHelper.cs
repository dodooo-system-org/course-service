using Microsoft.Extensions.Logging;

namespace course_service.Shared.Helpers;

public static class LoggerHelper
{
    private static ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

    public static void SetLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public static ILogger<T> GetLogger<T>()
    {
        return _loggerFactory.CreateLogger<T>();
    }
}
