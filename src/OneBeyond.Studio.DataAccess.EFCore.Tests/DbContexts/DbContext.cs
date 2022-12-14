using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OneBeyond.Studio.DataAccess.EFCore.DomainEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.DbContexts;

internal class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbContext(DbContextOptions options, bool areDomainEventsEnabled)
        : base(options)
    {
        AreDomainEventsEnabled = areDomainEventsEnabled;
    }

    /// <summary>
    /// This ctor is intended for building a dynamic proxy around already existing
    /// and properly initialized <see cref="DbContext"/>. The proxy is used for injecting
    /// domain events related functionality.
    /// </summary>
    protected DbContext()
    {
    }

    public bool AreDomainEventsEnabled { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        if (AreDomainEventsEnabled)
        {
            modelBuilder.ApplyConfiguration(new RaisedDomainEventConfiguration());
        }
    }
}
