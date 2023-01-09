using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.AmbientContexts;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.AmbientContexts;

internal sealed record TestableAmbientContext : AmbientContext
{
    public TestableAmbientContext(string stringValue)
    {
        EnsureArg.IsNotNullOrWhiteSpace(stringValue, nameof(stringValue));

        StringValue = stringValue;
    }

    public string StringValue { get; }
}
