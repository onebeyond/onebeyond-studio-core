using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OneBeyond.Studio.DataAccess.EFCore.Configurations;

/// <summary>
/// </summary>
/// <typeparam name="TDerivedEntity"></typeparam>
/// <typeparam name="TBaseEntity"></typeparam>
public abstract class DerivedEntityTypeConfiguration<TDerivedEntity, TBaseEntity>
    : IEntityTypeConfiguration<TDerivedEntity>
    where TDerivedEntity : class, TBaseEntity
{
    /// <summary>
    /// </summary>
    /// <param name="builder"></param>
    void IEntityTypeConfiguration<TDerivedEntity>.Configure(EntityTypeBuilder<TDerivedEntity> builder)
    {
        builder.HasBaseType<TBaseEntity>();

        DoConfigure(builder);
    }

    /// <summary>
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void DoConfigure(EntityTypeBuilder<TDerivedEntity> builder)
    {
    }
}
