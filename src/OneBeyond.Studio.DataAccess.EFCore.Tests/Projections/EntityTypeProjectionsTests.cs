using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OneBeyond.Studio.DataAccess.EFCore.Projections;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Projections;

[TestClass]
public sealed class EntityTypeProjectionsTests
{
    private readonly Mock<IEntityTypeProjection<SomeEntity, SomeDto1>> _dto1ProjectionMock;
    private readonly Mock<IEntityTypeProjection<SomeEntity, SomeDto2>> _dto2ProjectionMock;
    private readonly Mock<IEntityTypeProjection<SomeEntity>> _incompleteProjectionMock;
    private readonly Mock<EntityTypeProjections<SomeEntity>> _entityProjectionsMock;

    public EntityTypeProjectionsTests()
    {
        _dto1ProjectionMock = new Mock<IEntityTypeProjection<SomeEntity, SomeDto1>>();
        _dto2ProjectionMock = _dto1ProjectionMock.As<IEntityTypeProjection<SomeEntity, SomeDto2>>();
        _incompleteProjectionMock = new Mock<IEntityTypeProjection<SomeEntity>>();
        _entityProjectionsMock = CreateEntityProjectionsMock(
            new Mock<IConfigurationProvider>().Object,
            _dto1ProjectionMock.Object,
            _dto2ProjectionMock.Object);
    }

    [TestMethod]
    public void Calling_Default_Projection_When_Custom_One_Is_Not_Provided()
    {
        // Act
        _entityProjectionsMock.Object.ProjectTo<SomeDto3>(
            new Mock<IQueryable<SomeEntity>>().Object,
            new Mock<DbContext>().Object);

        // Assert
        _entityProjectionsMock.Verify((entityTypeProjections) =>
            entityTypeProjections.DoProjectDefault<SomeDto3>(
                It.IsAny<IQueryable<SomeEntity>>(),
                It.IsAny<ProjectionContext>()),
                Times.Once());
        _dto1ProjectionMock.VerifyNoOtherCalls();
        _dto2ProjectionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Calling_EntityTypeProjection_When_Provided_In_Lieu_Of_Default_One()
    {
        // Act
        _entityProjectionsMock.Object.ProjectTo<SomeDto1>(
            new Mock<IQueryable<SomeEntity>>().Object,
            new Mock<DbContext>().Object);

        // Assert
        _entityProjectionsMock.Verify(
            (entityTypeProjections) => entityTypeProjections.DoProjectDefault<It.IsAnyType>(
                It.IsAny<IQueryable<SomeEntity>>(),
                It.IsAny<ProjectionContext>()),
            Times.Never());
        _dto1ProjectionMock.Verify(
            (entityTypeProjection) => entityTypeProjection.Project(
                It.IsAny<IQueryable<SomeEntity>>(),
                It.IsAny<ProjectionContext>()),
            Times.Once());
        _dto2ProjectionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Throwing_When_EntityTypeProjection_Is_Not_Fully_Implemented()
    {
        // Arrange
        var entityProjectionsMock = CreateEntityProjectionsMock(
            new Mock<IConfigurationProvider>().Object,
            _incompleteProjectionMock.Object);

        // Act
        try
        {
            _ = entityProjectionsMock.Object;
            Assert.Fail();
        }
        catch (TargetInvocationException exception)
        when (exception.InnerException is ArgumentOutOfRangeException outOfRangeException
            && outOfRangeException.ParamName == "entityTypeProjection")
        {
        }
    }

    private static Mock<EntityTypeProjections<SomeEntity>> CreateEntityProjectionsMock(
        IConfigurationProvider mapperConfigurationProvider,
        params IEntityTypeProjection<SomeEntity>[] projections)
    {
        var entityProjectionsMock = new Mock<EntityTypeProjections<SomeEntity>>(
            () => new EntityTypeProjections<SomeEntity>(
                projections.ToHashSet(),
                mapperConfigurationProvider));

        entityProjectionsMock.Setup((entityTypeProjections) =>
            entityTypeProjections.DoProjectDefault<It.IsAnyType>(
                It.IsAny<IQueryable<SomeEntity>>(),
                It.IsAny<ProjectionContext>()));

        return entityProjectionsMock;
    }

    internal sealed class SomeEntity
    {
    }

    internal sealed record SomeDto1
    {
    }

    internal sealed record SomeDto2
    {
    }

    internal sealed record SomeDto3
    {
    }
}
