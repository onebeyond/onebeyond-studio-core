using System;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.AirTravels;

internal abstract class Company : AggregateRoot<Guid>
{
    protected Company()
        : base(Guid.NewGuid())
    {
    }
}
