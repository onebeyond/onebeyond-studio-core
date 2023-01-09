using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Crosscuts.Logging;

namespace OneBeyond.Studio.Application.SharedKernel.Tests;

internal sealed class LogManagerFixture : IDisposable
{
    static LogManagerFixture()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>()
            ?? throw new Exception("Unable to resolve ILoggerFactory interface.");
        LogManager.TryConfigure(loggerFactory);
    }

    public LogManagerFixture()
    {
    }

    public void Dispose()
    {
    }
}
