using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Configurations.AirTravels;

internal sealed class AirlineCompanyConfiguration : DerivedEntityTypeConfiguration<Airline.Company, Company>
{
    protected override void DoConfigure(EntityTypeBuilder<Airline.Company> builder)
    {
        builder.OwnsOne(
            (entity) => entity.Data,
            (navigationBuilder) =>
            {
                navigationBuilder.ToTable("Airlines");

                navigationBuilder.Property((entity) => entity.IataCode)
                    .IsRequired()
                    .HasMaxLength(2);
            });
    }
}
