using EnsureThat;
using Microsoft.EntityFrameworkCore;

namespace OneBeyond.Studio.DataAccess.EFCore.Projections;

public sealed record ProjectionContext
{
    internal ProjectionContext(
        DbContext dbContext)
    {
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));        

        DbContext = dbContext;        
    }

    public DbContext DbContext { get; }    
}
