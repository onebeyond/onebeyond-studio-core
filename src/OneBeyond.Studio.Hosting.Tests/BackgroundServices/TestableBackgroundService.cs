using EnsureThat;
using OneBeyond.Studio.Hosting.BackgroundServices;

namespace OneBeyond.Studio.Hosting.Tests.BackgroundServices;

internal sealed class TestableBackgroundService : IBackgroundService
{
    private readonly ITestableBackgroundServiceDependency _dependency;

    public TestableBackgroundService(ITestableBackgroundServiceDependency dependency)
    {
        EnsureArg.IsNotNull(dependency, nameof(dependency));

        _dependency = dependency;
    }

    public Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _dependency.DoSomething();
        return Task.CompletedTask;
    }
}
