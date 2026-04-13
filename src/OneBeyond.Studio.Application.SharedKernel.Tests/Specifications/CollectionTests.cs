using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;
using OneBeyond.Studio.Application.SharedKernel.Specifications;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Specifications;


public sealed class CollectionTests
{
    [Fact]
    public void TestIncludesCompileWithCollections()
    {
        var includes = new Includes<SomeClass>();

        // ICollection => ICollection => int
        includes = new Includes<SomeClass>();
        includes = includes.Include((some) => some.SomeProperty3)
            .ThenInclude((another) => another.AnotherProperty2)
            .ThenInclude((yetAnother) => yetAnother.YetAnotherProperty1);

        // ICollection => IEnumerable => int
        includes = new Includes<SomeClass>();
        includes = includes.Include((some) => some.SomeProperty3)
            .ThenInclude((another) => another.AnotherProperty3)
            .ThenInclude((yetAnother) => yetAnother.YetAnotherProperty1);

        // IEnumerable => ICollection => int
        includes = new Includes<SomeClass>();
        includes = includes.Include((some) => some.SomeProperty4)
            .ThenInclude((another) => another.AnotherProperty2)
            .ThenInclude((yetAnother) => yetAnother.YetAnotherProperty1);

        // IEnumerable => IEnumerable => int
        includes = new Includes<SomeClass>();
        includes = includes.Include((some) => some.SomeProperty4)
            .ThenInclude((another) => another.AnotherProperty3)
            .ThenInclude((yetAnother) => yetAnother.YetAnotherProperty1);

        var expressions = new List<(Expression, IList<Expression>)>();
        var testIncludes = new TestIncludes<SomeClass>(expressions);

        testIncludes = includes.Replay(testIncludes);
    }
}

