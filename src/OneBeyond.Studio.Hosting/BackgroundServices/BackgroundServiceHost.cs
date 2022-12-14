using Autofac;
using EnsureThat;
using Microsoft.Extensions.Hosting;

namespace OneBeyond.Studio.Hosting.BackgroundServices
{
    internal sealed class BackgroundServiceHost<TBackgroundService> : BackgroundService
        where TBackgroundService : IBackgroundService
    {
        private readonly ILifetimeScope _hostLifetimeScope;
        private readonly BackgroundServiceHostOptions<TBackgroundService> _hostOptions;

        public BackgroundServiceHost(
            ILifetimeScope hostLifetimeScope,
            BackgroundServiceHostOptions<TBackgroundService> hostOptions)
        {
            EnsureArg.IsNotNull(hostLifetimeScope, nameof(hostLifetimeScope));
            EnsureArg.IsNotNull(hostOptions, nameof(hostOptions));

            _hostLifetimeScope = hostLifetimeScope;
            _hostOptions = hostOptions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var serviceLifetimeScope = _hostLifetimeScope.BeginLifetimeScope(
                (containerBuilder) =>
                    _hostOptions.ConfigureServices?.Invoke(containerBuilder)))
            {
                var service = serviceLifetimeScope.Resolve<TBackgroundService>();
                await service.ExecuteAsync(stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
