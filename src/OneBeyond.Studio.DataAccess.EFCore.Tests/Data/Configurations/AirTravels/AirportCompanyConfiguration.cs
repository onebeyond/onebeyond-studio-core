using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Configurations.AirTravels;

internal sealed class AirportCompanyConfiguration : DerivedEntityTypeConfiguration<Airport.Company, Company>
{
    protected override void DoConfigure(EntityTypeBuilder<Airport.Company> builder)
    {
        builder.OwnsOne(
            (entity) => entity.Data,
            (navigationBuilder) =>
            {
                navigationBuilder.ToTable("Airports");

                navigationBuilder.Property((entity) => entity.IataCode)
                    .IsRequired()
                    .HasMaxLength(3);
            });
    }
}
