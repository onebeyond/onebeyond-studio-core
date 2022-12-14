using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Configurations.AirTravels;

internal sealed class CompanyConfiguration : BaseEntityTypeConfiguration<Company, Guid>
{
    protected override void DoConfigure(EntityTypeBuilder<Company> builder)
    {
        builder.HasDiscriminator<string>("Discriminator")
            .HasValue<Airline.Company>("Airline")
            .HasValue<Airport.Company>("Airport");
    }
}
