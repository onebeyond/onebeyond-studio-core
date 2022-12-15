using System;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.IntegrationEvents;

internal sealed class TestableScopedItem
{
    public Type? HandlerType { get; private set; }

    public void SetHandlerType(Type handlerType)
    {
        EnsureArg.IsNotNull(handlerType, nameof(handlerType));
        Ensure.Bool.IsTrue(HandlerType is null);

        HandlerType = handlerType;
    }
}
