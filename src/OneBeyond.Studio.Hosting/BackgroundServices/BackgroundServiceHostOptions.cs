using Autofac;

namespace OneBeyond.Studio.Hosting.BackgroundServices;

internal sealed record BackgroundServiceHostOptions<TBackgroundService>
    where TBackgroundService : IBackgroundService
{
    public Action<ContainerBuilder>? ConfigureServices { get; init; }
}
