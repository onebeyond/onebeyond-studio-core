using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using OneBeyond.Studio.Crosscuts.DynamicProxy;
using OneBeyond.Studio.Crosscuts.Reflection;
using OneBeyond.Studio.Crosscuts.Utilities.LogicalCallContext;
using OneBeyond.Studio.Application.SharedKernel.AmbientContexts;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.DomainEvents;

using EntityDomainEventList = IReadOnlyCollection<(DomainEntity Entity, IReadOnlyCollection<DomainEvent> DomainEvents)>;

internal sealed class DomainEventsProcessor : InterceptorBase
{
    private readonly IPreSaveDomainEventDispatcher _preSaveDomainEventDispatcher;
    private readonly IAmbientContextAccessor? _ambientContextAccessor;

    public DomainEventsProcessor(
        IPreSaveDomainEventDispatcher preSaveDomainEventDispatcher,
        IEnumerable<IAmbientContextAccessor> ambientContextAccessors)
    {
        EnsureArg.IsNotNull(preSaveDomainEventDispatcher, nameof(preSaveDomainEventDispatcher));
        EnsureArg.IsNotNull(ambientContextAccessors, nameof(ambientContextAccessors));

        _preSaveDomainEventDispatcher = preSaveDomainEventDispatcher;
        _ambientContextAccessor = ambientContextAccessors.SingleOrDefault();
    }

    protected override T Execute<T>(ISyncExecution<T> execution)
    {
        using (var saveChangesCall = InterceptSaveChangesCall(execution))
        {
            var entityDomainEvents = default(EntityDomainEventList);
            if (saveChangesCall?.IsFirstOnStack == true)
            {
                entityDomainEvents = CollectDomainEvents(saveChangesCall.DbContext);
                DispatchDomainEventsOnPreSaveAsync(entityDomainEvents)
                    .GetAwaiter()
                    .GetResult();
                var ambientContext = _ambientContextAccessor?.AmbientContext;
                var activity = Activity.Current;
                entityDomainEvents.ForEach(
                    (item) =>
                    {
                        var raisedDomainEvents = item.DomainEvents.Select(
                                (domainEvent) => new RaisedDomainEvent(item.Entity, domainEvent, ambientContext, activity));
                        saveChangesCall.RaisedDomainEvents.AddRange(raisedDomainEvents);
                    });
                if (CollectDomainEvents(saveChangesCall.DbContext).Count > 0)
                {
                    throw new InvalidOperationException("Raising domain events in pre-save domain event handlers is not allowed.");
                }
            }

            return base.Execute(execution);
        }
    }

    protected override async Task<T> ExecuteAsync<T>(IAsyncExecution<T> execution)
    {
        using (var saveChangesCall = InterceptSaveChangesCall(execution))
        {
            var entityDomainEvents = default(EntityDomainEventList);
            if (saveChangesCall?.IsFirstOnStack == true)
            {
                entityDomainEvents = CollectDomainEvents(saveChangesCall.DbContext);
                await DispatchDomainEventsOnPreSaveAsync(entityDomainEvents).ConfigureAwait(false);
                var ambientContext = _ambientContextAccessor?.AmbientContext;
                var activity = Activity.Current;
                entityDomainEvents.ForEach(
                    (item) =>
                    {
                        var raisedDomainEvents = item.DomainEvents.Select(
                                (domainEvent) => new RaisedDomainEvent(item.Entity, domainEvent, ambientContext, activity));
                        saveChangesCall.RaisedDomainEvents.AddRange(raisedDomainEvents);
                    });
                if (CollectDomainEvents(saveChangesCall.DbContext).Count > 0)
                {
                    throw new InvalidOperationException("Raising domain events in pre-save domain event handlers is not allowed.");
                }
            }

            return await base.ExecuteAsync(execution).ConfigureAwait(false);
        }
    }

    private async Task DispatchDomainEventsOnPreSaveAsync(EntityDomainEventList entityDomainEvents)
    {
        foreach (var (_, domainEvents) in entityDomainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                await _preSaveDomainEventDispatcher.DispatchAsync(
                        domainEvent,
                        CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }
    }

    private static EntityDomainEventList CollectDomainEvents(DbContext dbContext)
    {
        IEnumerable<(DomainEntity entity, IReadOnlyCollection<DomainEvent> domainEvents)> raisedDomainEvents =
            dbContext.ChangeTracker.Entries<DomainEntity>()
                .Select(
                    (entityEntry) =>
                    (
                        entityEntry.Entity,
                        entityEntry.Entity.ReleaseDomainEvents().ToList() as IReadOnlyCollection<DomainEvent>
                    ));
        raisedDomainEvents = raisedDomainEvents
            .Where(
                (entityDomainEvents) => entityDomainEvents.domainEvents.Count > 0);
        return raisedDomainEvents
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
