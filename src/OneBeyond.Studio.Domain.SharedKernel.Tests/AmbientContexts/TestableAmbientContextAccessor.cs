using OneBeyond.Studio.Domain.SharedKernel.AmbientContexts;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.AmbientContexts;

internal sealed class TestableAmbientContextAccessor : IAmbientContextAccessor<TestableAmbientContext>
{
    public TestableAmbientContext AmbientContext => new("42");
}
