using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using OneBeyond.Studio.Crosscuts.Expressions;
using OneBeyond.Studio.Crosscuts.Reflection;

namespace OneBeyond.Studio.DataAccess.EFCore.DbContexts;

/// <summary>
/// Extension methods for the <see cref="ModelBuilder"/> class.
/// </summary>
public static class ModelBuilderExtensions
{
    private static readonly MethodInfo _setQueryFilterOnEntityMethodInfo =
        Reflector.MethodFrom(() => SetQueryFilterOnEntity<object, object>(default!, default!))
            .GetGenericMethodDefinition();

    /// <summary>
    /// Applies global query filter to all entities implementing <typeparamref name="TEntityInterface"/> interface.
    /// </summary>
    /// <typeparam name="TEntityInterface"></typeparam>
    /// <param name="modelBuilder"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static ModelBuilder SetQueryFilterOnEntities<TEntityInterface>(
        this ModelBuilder modelBuilder,
        Expression<Func<TEntityInterface, bool>> filter)
    {
        EnsureArg.IsNotNull(modelBuilder, nameof(modelBuilder));
        EnsureArg.IsNotNull(filter, nameof(filter));

        var entityInterfaceType = typeof(TEntityInterface);

        modelBuilder.Model.GetEntityTypes()
            .Where(
                (entityType) => entityType.BaseType == null)
            .Select(
                (entityType) => entityType.ClrType)
            .Where(
                (entityClrType) => entityInterfaceType.IsAssignableFrom(entityClrType))
            .ForEach(
                (entityClrType) => SetQueryFilterOnEntity(modelBuilder, entityClrType, filter));

        return modelBuilder;
    }

    private static void SetQueryFilterOnEntity<TEntityInterface>(
        ModelBuilder modelBuilder,
        Type enityClrType,
        Expression<Func<TEntityInterface, bool>> filter)
    {
        var setQueryFilterOnEntityMethod = _setQueryFilterOnEntityMethodInfo.MakeGenericMethod(enityClrType, typeof(TEntityInterface));
        setQueryFilterOnEntityMethod.Invoke(null, new object[] { modelBuilder, filter });
    }

    private static void SetQueryFilterOnEntity<TEntity, TEntityInterface>(
        ModelBuilder modelBuilder,
        Expression<Func<TEntityInterface, bool>> filter)
        where TEntityInterface : class
        where TEntity : class, TEntityInterface
    {
        var entityFilter = filter.ConvertParameterType<TEntityInterface, TEntity>();
        modelBuilder.Entity<TEntity>()
          .HasQueryFilter(entityFilter);
    }
}
