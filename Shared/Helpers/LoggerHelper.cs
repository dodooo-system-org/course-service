using Microsoft.Extensions.Logging;

namespace course_service.Shared.Helpers;

public static class LoggerHelper
{
    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

    public static ILogger<T> GetLogger<T>()
    {
        return _loggerFactory.CreateLogger<T>();
    }
}
