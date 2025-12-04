using System;
using System.Linq;
using System.Reflection;
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

    public EntityTypeProjectionsTests()
    {
        _dto1ProjectionMock = new Mock<IEntityTypeProjection<SomeEntity, SomeDto1>>();
        _dto2ProjectionMock = _dto1ProjectionMock.As<IEntityTypeProjection<SomeEntity, SomeDto2>>();
        _incompleteProjectionMock = new Mock<IEntityTypeProjection<SomeEntity>>();

    }

    [TestMethod]
    public void Throwing_When_EntityTypeProjection_Is_Not_Fully_Implemented()
    {
        // Arrange
        var entityProjectionsMock = CreateEntityProjectionsMock(            
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
        params IEntityTypeProjection<SomeEntity>[] projections)
    {
        var entityProjectionsMock = new Mock<EntityTypeProjections<SomeEntity>>(
            () => new EntityTypeProjections<SomeEntity>(
                projections.ToHashSet()));        

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
