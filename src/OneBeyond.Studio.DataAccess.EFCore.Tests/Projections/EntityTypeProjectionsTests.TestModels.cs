using System.Diagnostics.CodeAnalysis;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Projections;

public sealed partial class EntityTypeProjectionsTests
{
    internal abstract class Animal : DomainEntity<Guid>
    {
        protected Animal() : base(Guid.CreateVersion7()) { }
    }

    internal class Dog : Animal;

    [SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = "Mock type for testing.")]
    internal sealed class Husky : Dog;


    internal sealed record DogDto
    {
        public required string Id { get; init; }
    }

    internal sealed record DogSummaryDto
    {
        public required string Name { get; init; }
    }
}
