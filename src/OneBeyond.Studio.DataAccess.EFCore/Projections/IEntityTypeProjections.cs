using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OneBeyond.Studio.DataAccess.EFCore.Projections;

public interface IEntityTypeProjections<TEntity>
    where TEntity : class
{
    IQueryable<TResult> ProjectTo<TResult>(IQueryable<TEntity> entityQuery, DbContext dbContext);
}
