using System;
using System.Linq.Expressions;
using OneBeyond.Studio.Crosscuts.Expressions;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests;

public sealed class ExpressionTests
{
    private sealed class Entity5
    {
        public Entity5(Entity5Child child)
        {
            Child = child;
        }

        public Entity5Child Child { get; }
    }

    private sealed class Entity5Child
    {
        public int IntProperty { get; set; }
    }

    [Fact]
    public void TestExpressionIsReplacedProperly()
    {
        Expression<Func<Entity5, Entity5Child>> expression1 = (entity5) => entity5.Child;
        Expression<Func<Entity5Child, int>> expression2 = (entity5Child) => entity5Child.IntProperty;

        var expression3Body = expression2.Body.ReplaceExpression(expression2.Parameters[0], expression1.Body);
        var expression3 = Expression.Lambda<Func<Entity5, int>>(expression3Body, expression1.Parameters[0]);

        var func = expression3.Compile();

        var entity5 = new Entity5(new Entity5Child { IntProperty = 12 });

        Assert.Equal(12, func(entity5));
    }
}
