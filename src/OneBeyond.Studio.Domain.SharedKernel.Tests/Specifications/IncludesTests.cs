using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.Specifications;

[TestClass]
public sealed class IncludesTests
{
    [TestMethod]
    public void TestIncludesWithWhere()
    {
        var includeList = new List<(Expression, IList<Expression>)>();
        var includes = new TestIncludes<SomeClass>(includeList) as IIncludes<SomeClass>;

        Expression<Func<SomeClass, IEnumerable<AnotherClass>>> includeSomeProperty4 = (some) => some.SomeProperty4;
        Expression<Func<SomeClass, ICollection<AnotherClass>>> includeSomeProperty3 = (some) => some.SomeProperty3;
        Expression<Func<AnotherClass, ICollection<YetAnotherClass>>> includeAnotherProperty2 = (another) => another.AnotherProperty2;
        Expression<Func<AnotherClass, bool>> filterAnotherProperty1 = (another) => another.AnotherProperty1 == 0;
        Expression<Func<YetAnotherClass, bool>> filterYetAnotherProperty1 = (yetAnother) => yetAnother.YetAnotherProperty1.Contains(1);

        includes = includes
            .Include(includeSomeProperty4)
            .Where(filterAnotherProperty1)
                .ThenInclude((another) => another.AnotherProperty3)
            .Include(includeSomeProperty3)
                .ThenInclude(includeAnotherProperty2)
                .Where(filterYetAnotherProperty1);

        Assert.AreEqual(4, includeList.Count);

        var some4 = includeList.Single((include) => include.Item1.Equals(includeSomeProperty4));

        Assert.AreEqual(1, some4.Item2.Count);
        Assert.AreEqual(filterAnotherProperty1, some4.Item2[0]);

        var some3 = includeList.Single((include) => include.Item1.Equals(includeSomeProperty3));

        Assert.AreEqual(0, some3.Item2.Count);

        var another2 = includeList.Single((include) => include.Item1.Equals(includeAnotherProperty2));

        Assert.AreEqual(1, another2.Item2.Count);
        Assert.AreEqual(filterYetAnotherProperty1, another2.Item2[0]);
    }

    [TestMethod]
    public void HaveCartesionExplosion_Propagation_Down_To_Resulting_Instance()
    {
        var includes = new Includes<SomeClass>(haveCartesianExplosion: true)
            .Include((some) => some.SomeProperty4)
                .ThenInclude((another) => another.AnotherProperty2);

        Assert.AreEqual(true, includes.HaveCartesianExplosion);

        includes = new Includes<SomeClass>(haveCartesianExplosion: false)
            .Include((some) => some.SomeProperty4)
                .ThenInclude((another) => another.AnotherProperty2);

        Assert.AreEqual(false, includes.HaveCartesianExplosion);
    }
}
