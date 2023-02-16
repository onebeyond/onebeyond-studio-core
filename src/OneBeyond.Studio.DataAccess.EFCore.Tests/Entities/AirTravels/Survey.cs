using System;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

internal sealed class Survey : AggregateRoot<Guid>
{
    public Survey(string name, Company company)
        : base(Guid.NewGuid())
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
        EnsureArg.IsNotNull(company, nameof(company));

        Name = name;
        CompanyId = company.Id;
        Company = company;
    }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private Survey()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    {
    }

    public string Name { get; private set; }
    public Guid CompanyId { get; private set; }
    public Company Company { get; private set; }
}
