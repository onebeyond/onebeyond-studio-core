using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.DomainEvents;

public sealed class RaisedDomainEventConfiguration : IEntityTypeConfiguration<RaisedDomainEvent>
{
    internal const string RaisedDomainEventTableName = "RaisedDomainEvents";
    internal const string RaisedDomainEventIdColumnName = nameof(RaisedDomainEvent.Id);

    void IEntityTypeConfiguration<RaisedDomainEvent>.Configure(EntityTypeBuilder<RaisedDomainEvent> builder)
    {
        builder.Property((entity) => entity.Id)
            .HasColumnName(RaisedDomainEventIdColumnName)
            .UseIdentityColumn()
            .ValueGeneratedOnAdd();

        builder.Property((entity) => entity.EntityId);

        builder.Property((entity) => entity.EntityType);

        builder.Property((entity) => entity.DomainEventJson);

        builder.Property((entity) => entity.AmbientContextJson);

        builder.Property((entity) => entity.ActivityId);

        builder.Property((entity) => entity.ActivityTraceState);

        builder.ToTable(RaisedDomainEventTableName);

        builder.HasKey((entity) => entity.Id)
            .IsClustered(true);
    }
}
