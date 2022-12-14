using System.Diagnostics;
using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Crosscuts.Activities;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Hosting.BackgroundServices;

namespace OneBeyond.Studio.Hosting.IntegrationEvents
{
    internal sealed class IntegrationEventRelay : BackgroundService, IBackgroundService
    {
        private const string IntegrationEventRelayActivityName = "OneBeyond.Hosting.IntegrationEventRelay";

        private static readonly ILogger Logger = LogManager.CreateLogger<IntegrationEventRelay>();

        private readonly IIntegrationEventReceiver _integrationEventReceiver;
        private readonly IIntegrationEventDispatcher _integrationEventDispatcher;

        public IntegrationEventRelay(
            IIntegrationEventReceiver integrationEventReceiver,
            IIntegrationEventDispatcher integrationEventDispatcher)
        {
            EnsureArg.IsNotNull(integrationEventReceiver, nameof(integrationEventReceiver));
            EnsureArg.IsNotNull(integrationEventDispatcher, nameof(integrationEventDispatcher));

            _integrationEventReceiver = integrationEventReceiver;
            _integrationEventDispatcher = integrationEventDispatcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var activity = new Activity(IntegrationEventRelayActivityName).DoStart(
                Activity.Current?.Id,
                Activity.Current?.TraceStateString))
            {
                await _integrationEventReceiver.RunAsync(ProcessIntegrationEventAsync, stoppingToken);
            }
        }

        Task IBackgroundService.ExecuteAsync(CancellationToken stoppingToken)
            => ExecuteAsync(stoppingToken);

        private async Task ProcessIntegrationEventAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation(
                    "Integration event {@IntegrationEvent} of {IntegrationEventClrType} CLR type received from queue. Dispatching it...",
                    integrationEvent,
                    integrationEvent.GetType().AssemblyQualifiedName);

                await _integrationEventDispatcher.DispatchAsync(integrationEvent, cancellationToken);
            }
            catch (Exception exception)
            when (!exception.IsCritical())
            {
                Logger.LogCritical(
                    exception,
                    "Execution of integration event relay has terminated due to error");
                throw;
            }
        }
    }
}
