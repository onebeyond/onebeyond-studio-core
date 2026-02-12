using Microsoft.EntityFrameworkCore;
using Moq;
using OneBeyond.Studio.DataAccess.EFCore.Projections;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Projections;

[TestClass]
public sealed partial class EntityTypeProjectionsTests
{
    private readonly DbContext _dbContextMock = Mock.Of<DbContext>();

    internal sealed class DogProjection : IEntityTypeProjection<Dog, DogDto>
    {
        public IQueryable<DogDto> Project(IQueryable<Dog> entityQuery, ProjectionContext context)
            => entityQuery.Select(dog => new DogDto { Id = $"DogDto-{dog.IdAsString}" });
    }

    internal sealed class DogMultiProjection : IEntityTypeProjection<Dog, DogDto>, IEntityTypeProjection<Dog, DogSummaryDto>
    {
        IQueryable<DogDto> IEntityTypeProjection<Dog, DogDto>.Project(IQueryable<Dog> entityQuery, ProjectionContext context)
            => entityQuery.Select(dog => new DogDto { Id = $"DogDto-{dog.IdAsString}" });

        IQueryable<DogSummaryDto> IEntityTypeProjection<Dog, DogSummaryDto>.Project(IQueryable<Dog> entityQuery, ProjectionContext context)
            => entityQuery.Select(dog => new DogSummaryDto { Name = $"DogSummary-{dog.IdAsString}" });
    }


    [TestMethod]
    public void Ctor_GenericInterfaceNotImplemented_ArgumentOutOfRangeException()
    {
        // Arrange
        var incompleteProjectionMock = Mock.Of<IEntityTypeProjection>();

        // Act
        var action = () => new EntityTypeProjections<Dog>([incompleteProjectionMock]);

        // Assert
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(action);
        Assert.AreEqual("entityTypeProjection", exception.ParamName);
    }

    [TestMethod]
    public void ProjectTo_Simple_Succeed()
    {
        // Arrange
        var projection = new DogProjection();
        var projections = new EntityTypeProjections<Dog>([projection]);

        var entity = new Dog();
        var query = new[] { entity }.AsQueryable();

        // Act
        var result = projections.ProjectTo<DogDto>(query, _dbContextMock);

        // Assert
        DogDto[] expected = [new DogDto { Id = $"DogDto-{entity.Id}" }];
        CollectionAssert.AreEquivalent(expected, result.ToArray());
    }

    [TestMethod]
    public void ProjectTo_MultipleResultTypes_SupportAll()
    {
        // Arrange
        var entity = new Dog();
        var query = new[] { entity }.AsQueryable();

        var dogSummaryProjectionMock = new Mock<IEntityTypeProjection<Dog, DogSummaryDto>>();
        dogSummaryProjectionMock
            .Setup(mock => mock.Project(
                query,
                It.Is<ProjectionContext>(projectionContext => projectionContext.DbContext == _dbContextMock)))
            .Returns(new[] { new DogSummaryDto { Name = $"summary-{entity.Id}" } }.AsQueryable());
        var dogDtoProjection = new DogProjection();

        var projections = new EntityTypeProjections<Dog>([dogDtoProjection, dogSummaryProjectionMock.Object]);

        // Act
        var dogDtoResult = projections.ProjectTo<DogDto>(query, _dbContextMock);
        var dogSummaryDtoResult = projections.ProjectTo<DogSummaryDto>(query, _dbContextMock);

        // Assert
        DogDto[] expectedDogDtoResults = [new DogDto { Id = $"DogDto-{entity.Id}" }];
        DogSummaryDto[] expectedDogSummaryDtoResults = [new DogSummaryDto { Name = $"summary-{entity.Id}" }];
        CollectionAssert.AreEquivalent(expectedDogDtoResults, dogDtoResult.ToArray());
        CollectionAssert.AreEquivalent(expectedDogSummaryDtoResults, dogSummaryDtoResult.ToArray());
    }

    [TestMethod]
    public void ProjectTo_ImplementingMultipleProjections_BeSupported()
    {
        // Arrange
        var entity = new Dog();
        var query = new[] { entity }.AsQueryable();

        var multiProjection = new DogMultiProjection();
        var projections = new EntityTypeProjections<Dog>([multiProjection]);

        // Act
        var dogDtoResult = projections.ProjectTo<DogDto>(query, _dbContextMock);
        var dogSummaryDtoResult = projections.ProjectTo<DogSummaryDto>(query, _dbContextMock);

        // Assert
        DogDto[] expectedDogDtoResults = [new DogDto { Id = $"DogDto-{entity.Id}" }];
        DogSummaryDto[] expectedDogSummaryDtoResults = [new DogSummaryDto { Name = $"DogSummary-{entity.Id}" }];
        CollectionAssert.AreEquivalent(expectedDogDtoResults, dogDtoResult.ToArray());
        CollectionAssert.AreEquivalent(expectedDogSummaryDtoResults, dogSummaryDtoResult.ToArray());
    }

    [TestMethod]
    public void ProjectTo_MissingExactProjection_FallBackToBaseTypeProjection()
    {
        // Arrange
        var dogProjection = new DogProjection();
        var projections = new EntityTypeProjections<Husky>([dogProjection]);
        var entity = new Husky();
        var query = new[] { entity }.AsQueryable();

        // Act
        var result = projections.ProjectTo<DogDto>(query, _dbContextMock);

        // Assert
        DogDto[] expected = [new DogDto { Id = $"DogDto-{entity.Id}" }];
        CollectionAssert.AreEquivalent(expected, result.ToArray());
    }

    [TestMethod]
    public void ProjectTo_Always_UseMostSpecificProjection()
    {
        // Arrange
        var entity = new Husky();
        var query = new[] { entity }.AsQueryable();
        DogDto[] expected = [new DogDto { Id = $"HuskyDto-{entity.Id}" }];
        var huskyProjectionMock = new Mock<IEntityTypeProjection<Husky, DogDto>>();
        huskyProjectionMock
            .Setup(mock => mock.Project(
                query,
                It.Is<ProjectionContext>(projectionContext => projectionContext.DbContext == _dbContextMock)))
            .Returns(expected.AsQueryable());
        var dogProjection = new DogProjection();

        var projections = new EntityTypeProjections<Husky>([dogProjection, huskyProjectionMock.Object]);

        // Act
        var result = projections.ProjectTo<DogDto>(query, _dbContextMock);

        // Assert
        CollectionAssert.AreEquivalent(expected, result.ToArray());
    }

    [TestMethod]
    public void ProjectTo_MissingProjection_InvalidOperationException()
    {
        // Arrange
        var projections = new EntityTypeProjections<Dog>([new DogProjection()]);
        var query = new[] { new Dog() }.AsQueryable();

        // Act
        var action = () => projections.ProjectTo<DogSummaryDto>(query, _dbContextMock);

        // Assert
        var exception = Assert.ThrowsExactly<InvalidOperationException>(action);

        Assert.Contains("No projection specified", exception.Message);
        Assert.Contains(nameof(Dog), exception.Message);
        Assert.Contains(nameof(DogSummaryDto), exception.Message);
    }
}
