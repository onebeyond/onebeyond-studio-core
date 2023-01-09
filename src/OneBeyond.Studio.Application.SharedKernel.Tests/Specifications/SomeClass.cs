using System.Collections.Generic;

#nullable disable

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Specifications;

internal sealed class SomeClass
{
    public int SomeProperty1 { get; set; }
    public int SomeProperty2 { get; set; }
    public ICollection<AnotherClass> SomeProperty3 { get; set; }
    public IEnumerable<AnotherClass> SomeProperty4 { get; set; }
}

internal sealed class AnotherClass
{
    public int AnotherProperty1 { get; set; }
    public ICollection<YetAnotherClass> AnotherProperty2 { get; set; }
    public IEnumerable<YetAnotherClass> AnotherProperty3 { get; set; }
}

internal sealed class YetAnotherClass
{
    public IEnumerable<int> YetAnotherProperty1 { get; set; }
}
