using OneBeyond.Studio.Application.SharedKernel.AmbientContexts;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.AmbientContexts;

internal sealed class TestableAmbientContextAccessor : IAmbientContextAccessor<TestableAmbientContext>
{
    public TestableAmbientContext AmbientContext => new("42");
}
