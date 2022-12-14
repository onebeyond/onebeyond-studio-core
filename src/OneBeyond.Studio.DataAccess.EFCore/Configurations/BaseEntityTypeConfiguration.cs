using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Configurations;

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
public abstract class BaseEntityTypeConfiguration<TEntity, TEntityId> : IEntityTypeConfiguration<TEntity>
    where TEntity : DomainEntity<TEntityId>
{
    private static readonly bool IsGeneratedKey =
        typeof(TEntityId) == typeof(int) || typeof(TEntityId) == typeof(long);

    private readonly bool _hasManyRows;

    /// <summary>
    /// </summary>
    /// <param name="hasManyRows">
    /// Defines whether an alternative identity based key is int (<paramref name="hasManyRows"/> = false)
    /// or long (<paramref name="hasManyRows"/> = true) for non int/long key entities.
    /// </param>
    protected BaseEntityTypeConfiguration(bool hasManyRows = false)
    {
        _hasManyRows = hasManyRows;
    }

    void IEntityTypeConfiguration<TEntity>.Configure(EntityTypeBuilder<TEntity> builder)
    {
        var entityName = typeof(TEntity).Name;
        var tableName = entityName.Pluralize();
        builder.ToTable(tableName);

        builder.HasKey((entity) => entity.Id)
            .IsClustered(IsGeneratedKey);

        if (IsGeneratedKey)
        {
            ConfigureGeneratedKeyEntity(builder);
        }
        else
        {
            ConfigurePreDefinedKeyEntity(builder);
        }

        DoConfigure(builder);
    }

    /// <summary>
    /// Customize a configuration for the Entity
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void DoConfigure(EntityTypeBuilder<TEntity> builder)
    {
    }

    private void ConfigurePreDefinedKeyEntity(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property((entity) => entity.Id)
            .ValueGeneratedNever();

        var identityMetadata = _hasManyRows
            ? builder.Property<long>("Identity")
                .UseIdentityColumn()
                .ValueGeneratedOnAdd()
                .Metadata
            : builder.Property<int>("Identity")
                .UseIdentityColumn()
                .ValueGeneratedOnAdd()
                .Metadata;

        identityMetadata
            .SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex("Identity")
            .IsClustered();
    }

    private void ConfigureGeneratedKeyEntity(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property((entity) => entity.Id)
            .ValueGeneratedOnAdd();
    }
}
