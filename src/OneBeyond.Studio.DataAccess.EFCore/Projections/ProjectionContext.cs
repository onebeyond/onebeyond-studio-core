using AutoMapper;
using EnsureThat;
using Microsoft.EntityFrameworkCore;

namespace OneBeyond.Studio.DataAccess.EFCore.Projections;

public sealed record ProjectionContext
{
    internal ProjectionContext(
        DbContext dbContext,
        IConfigurationProvider mapperConfigurationProvider)
    {
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));
        EnsureArg.IsNotNull(mapperConfigurationProvider, nameof(mapperConfigurationProvider));

        DbContext = dbContext;
        MapperConfigurationProvider = mapperConfigurationProvider;
    }

    public DbContext DbContext { get; }
    public IConfigurationProvider MapperConfigurationProvider { get; }
}
