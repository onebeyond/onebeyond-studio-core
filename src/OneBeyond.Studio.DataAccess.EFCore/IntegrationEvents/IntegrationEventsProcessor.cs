using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using OneBeyond.Studio.Crosscuts.DynamicProxy;
using OneBeyond.Studio.Crosscuts.Reflection;
using OneBeyond.Studio.Crosscuts.Utilities.LogicalCallContext;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.DomainEvents;

using EntityIntegrationEventList = IReadOnlyCollection<(DomainEntity Entity, IReadOnlyCollection<IntegrationEvent> IntegrationEvents)>;

internal sealed class IntegrationEventsProcessor : InterceptorBase
{
    private readonly IIntegrationEventDispatcher _integrationEventDispatcher;

    public IntegrationEventsProcessor(
        IIntegrationEventDispatcher integrationEventDispatcher)
    {
        EnsureArg.IsNotNull(integrationEventDispatcher, nameof(integrationEventDispatcher));

        _integrationEventDispatcher = integrationEventDispatcher;
    }

    protected override T Execute<T>(ISyncExecution<T> execution)
    {
        using (var saveChangesCall = InterceptSaveChangesCall(execution))
        {
            var entityIntegrationEvents = default(EntityIntegrationEventList);
            if (saveChangesCall?.IsFirstOnStack == true)
            {
                entityIntegrationEvents = CollectIntegrationEvents(saveChangesCall.DbContext);
                DispatchIntegrationEventsOnPreSaveAsync(entityIntegrationEvents)
                    .GetAwaiter()
                    .GetResult();
            }

            return base.Execute(execution);
        }
    }

    protected override async Task<T> ExecuteAsync<T>(IAsyncExecution<T> execution)
    {
        using (var saveChangesCall = InterceptSaveChangesCall(execution))
        {
            var entityDomainEvents = default(EntityIntegrationEventList);
            if (saveChangesCall?.IsFirstOnStack == true)
            {
                entityDomainEvents = CollectIntegrationEvents(saveChangesCall.DbContext);
                await DispatchIntegrationEventsOnPreSaveAsync(entityDomainEvents).ConfigureAwait(false);
            }

            return await base.ExecuteAsync(execution).ConfigureAwait(false);
        }
    }

    private async Task DispatchIntegrationEventsOnPreSaveAsync(EntityIntegrationEventList entityIntegrationEvents)
    {
        foreach (var (_, integrationEvents) in entityIntegrationEvents)
        {
            foreach (var integrationEvent in integrationEvents)
            {
                await _integrationEventDispatcher.DispatchAsync(
                        integrationEvent,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }
    }

    private static EntityIntegrationEventList CollectIntegrationEvents(DbContext dbContext)
    {
        IEnumerable<(DomainEntity entity, IReadOnlyCollection<IntegrationEvent> integrationEvents)> raisedIntegrationEvents =
            dbContext.ChangeTracker.Entries<DomainEntity>()
                .Select(
                    (entityEntry) =>
                    (
                        entityEntry.Entity,
                        entityEntry.Entity.ReleaseIntegrationEvents().ToList() as IReadOnlyCollection<IntegrationEvent>
                    ));
        raisedIntegrationEvents = raisedIntegrationEvents
            .Where(
                (entityDomainEvents) => entityDomainEvents.integrationEvents.Count > 0);
        return raisedIntegrationEvents
            .ToList();
    }

    private static SaveChangesCall? InterceptSaveChangesCall(IExecution execution)
    {
        return SaveChangesCall.MethodInfoList.Any(
                (methodInfo) =>
                    methodInfo.Equals(execution.Method.GetBaseDefinition())) // SaveChanges can be overriden by a derived class
            ? new SaveChangesCall((DbContext)execution.Target)
            : default;
    }

    private sealed class SaveChangesCall : IDisposable
    {
        public SaveChangesCall(DbContext dbContext)
        {
            EnsureArg.IsNotNull(dbContext, nameof(dbContext));

            DbContext = dbContext;
            RaisedDomainEvents = dbContext.Set<RaisedDomainEvent>();
            if (LogicalCallContext.FindData<bool?>(SAVE_CHANGES_CALL_DATA) == true)
            {
                IsFirstOnStack = false;
            }
            else
            {
                LogicalCallContext.SetData(SAVE_CHANGES_CALL_DATA, true);
                IsFirstOnStack = true;
            }
        }

        private const string SAVE_CHANGES_CALL_DATA = nameof(SaveChangesCall);

        public static MethodInfo[] MethodInfoList { get; } = new[]
        {
                Reflector.MethodFrom(
                    (DbContext dbContext) => dbContext.SaveChanges()),
                Reflector.MethodFrom(
                    (DbContext dbContext) => dbContext.SaveChanges(default)),
                Reflector.MethodFrom(
                    (DbContext dbContext) => dbContext.SaveChangesAsync(default)),
                Reflector.MethodFrom(
                    (DbContext dbContext) => dbContext.SaveChangesAsync(default, default)),
            };

        public DbContext DbContext { get; }
        public DbSet<RaisedDomainEvent> RaisedDomainEvents { get; }
        public bool IsFirstOnStack { get; }

        public void Dispose()
        {
            if (IsFirstOnStack)
            {
                LogicalCallContext.SetData(SAVE_CHANGES_CALL_DATA, null);
            }
        }
    }
}
