using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
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

    internal sealed class DogDbContext : DbContext
    {
        public DogDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Animal>().HasKey(e => e.Id);
            modelBuilder.Entity<Dog>();
        }
    }
}
