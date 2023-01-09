namespace OneBeyond.Studio.Application.SharedKernel.Tests;

internal sealed class DerivedValueObject : SomeValueObject
{
    public DerivedValueObject(int x1, int x2, int x3)
        : base(x1, x2, x3)
    {
    }
}
