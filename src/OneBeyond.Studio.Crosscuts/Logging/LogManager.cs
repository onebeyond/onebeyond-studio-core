using EnsureThat;
using Microsoft.Extensions.Logging;

namespace OneBeyond.Studio.Crosscuts.Logging;

/// <summary>
/// Configures and creates application loggers.
/// </summary>
public static class LogManager
{
    private static ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>
    /// Configures which logger factory to be used by application loggers.
    /// It can be called just once throughout an application life cycle.
    /// </summary>
    /// <param name="loggerFactory">Logger factory</param>
    public static void Configure(ILoggerFactory loggerFactory)
    {
        EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
        Ensure.Bool.IsTrue(
            LoggerFactory is null
            || LoggerFactory == loggerFactory,
            nameof(LoggerFactory), (options) => options.WithMessage($"{nameof(LogManager)} has already been configured."));

        LoggerFactory = loggerFactory;
    }

    /// <summary>
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <returns></returns>
    public static bool TryConfigure(ILoggerFactory loggerFactory)
    {
        EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

        var result = LoggerFactory is null;

        LoggerFactory ??= loggerFactory;

        return result;
    }

    /// <summary>
    /// Creates a logger with specified type. LogManager has to be configured before calling this method.
    /// </summary>
    /// <typeparam name="T">Logger type</typeparam>
    /// <returns></returns>
    public static ILogger CreateLogger<T>()
    {
        return CreateLogger(typeof(T).Name);
    }

    /// <summary>
    /// Creates a logger with specified name. LogManager has to be configured before calling this method.
    /// </summary>
    /// <param name="loggerName">Logger name</param>
    /// <returns></returns>
    public static ILogger CreateLogger(string loggerName)
    {
        EnsureArg.IsNotNull(loggerName, nameof(loggerName));
        Ensure.Bool.IsFalse(LoggerFactory == null, nameof(LoggerFactory), (options) => options.WithMessage($"Configure {nameof(LogManager)} first."));

        return LoggerFactory!.CreateLogger(loggerName);
    }
}
