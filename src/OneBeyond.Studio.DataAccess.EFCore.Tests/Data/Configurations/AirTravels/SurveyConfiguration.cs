using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Configurations.AirTravels;

internal sealed class SurveyConfiguration : BaseEntityTypeConfiguration<Survey, Guid>
{
    protected override void DoConfigure(EntityTypeBuilder<Survey> builder)
    {
        builder.HasOne((entity) => entity.Company)
            .WithMany()
            .HasForeignKey((entity) => entity.CompanyId)
            .IsRequired();
    }
}
