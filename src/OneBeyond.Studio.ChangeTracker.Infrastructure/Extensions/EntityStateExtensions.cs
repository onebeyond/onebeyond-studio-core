using Microsoft.EntityFrameworkCore;

namespace OneBeyond.Studio.ChangeTracker.Infrastructure.Extensions;

public static class EntityStateExtensions
{
    public static bool IsChanged(this EntityState source)
        => new[] { EntityState.Added, EntityState.Deleted, EntityState.Modified }.Contains(source);
}

