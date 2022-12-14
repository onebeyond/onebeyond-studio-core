using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests;

internal class SomeValueObject : ValueObject
{
    public SomeValueObject(int x1, int x2, int x3)
    {
        PublicGetPublicSet = x1;
        PublicGetProtectedSet = x2;
        PublicGetPrivateSet = x3;
    }

    public int PublicGetPublicSet { get; set; }
    public int PublicGetProtectedSet { get; protected set; }
    public int PublicGetPrivateSet { get; private set; }
}
