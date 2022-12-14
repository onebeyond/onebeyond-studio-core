using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Utilities.LogicalCallContext;

namespace OneBeyond.Studio.Domain.SharedKernel.AmbientContexts;

public sealed class AmbientContextAccessorOverrider<TAmbientContext> : IAmbientContextAccessor<TAmbientContext>
    where TAmbientContext : AmbientContext
{
    public static IDisposable OverrideWith(IAmbientContextAccessor<TAmbientContext> ambientContextAccessor)
        => new AmbientContextAccessorOverride(ambientContextAccessor);

    internal AmbientContextAccessorOverrider(IAmbientContextAccessor<TAmbientContext> ambientContextAccessor)
    {
        EnsureArg.IsNotNull(ambientContextAccessor, nameof(ambientContextAccessor));

        _ambientContextAccessor = ambientContextAccessor;
    }

    internal AmbientContextAccessorOverrider(IAmbientContextAccessor ambientContextAccessor)
    {
        EnsureArg.IsAssignableToType(
            ambientContextAccessor,
            typeof(IAmbientContextAccessor<TAmbientContext>),
            nameof(ambientContextAccessor));

        _ambientContextAccessor = (IAmbientContextAccessor<TAmbientContext>)ambientContextAccessor;
    }

    private const string AmbientContextAccessorStackName = "AmbientContextAccessorStack";
    private readonly IAmbientContextAccessor<TAmbientContext> _ambientContextAccessor;

    TAmbientContext IAmbientContextAccessor<TAmbientContext>.AmbientContext => TryGetOverride(out var ambientContextAccessor)
        ? ambientContextAccessor.AmbientContext
        : _ambientContextAccessor.AmbientContext;

    AmbientContext IAmbientContextAccessor.AmbientContext => TryGetOverride(out var ambientContextAccessor)
        ? ambientContextAccessor.AmbientContext
        : _ambientContextAccessor.AmbientContext;

    private static bool TryGetOverride([NotNullWhen(true)] out IAmbientContextAccessor<TAmbientContext>? ambientContextAccessor)
    {
        var ambientContextAccessorStack =
            LogicalCallContext.FindData<Stack<IAmbientContextAccessor<TAmbientContext>>>(AmbientContextAccessorStackName);
        if (ambientContextAccessorStack?.Count > 0)
        {
            ambientContextAccessor = ambientContextAccessorStack.Peek();
            return true;
        }
        ambientContextAccessor = null;
        return false;
    }

    private sealed class AmbientContextAccessorOverride : IDisposable
    {
        public AmbientContextAccessorOverride(IAmbientContextAccessor<TAmbientContext> ambientContextAccessor)
        {
            EnsureArg.IsNotNull(ambientContextAccessor, nameof(ambientContextAccessor));

            var ambientContextAccessorStack =
                LogicalCallContext.FindData<Stack<IAmbientContextAccessor<TAmbientContext>>>(AmbientContextAccessorStackName);
            if (ambientContextAccessorStack is null)
            {
                ambientContextAccessorStack = new Stack<IAmbientContextAccessor<TAmbientContext>>();
                LogicalCallContext.SetData(AmbientContextAccessorStackName, ambientContextAccessorStack);
            }
            ambientContextAccessorStack.Push(ambientContextAccessor);
            _ambientContextAccessor = ambientContextAccessor;
        }

        private IAmbientContextAccessor<TAmbientContext>? _ambientContextAccessor;

        public void Dispose()
        {
            var ambientContextAccessorStack =
                LogicalCallContext.FindData<Stack<IAmbientContextAccessor<TAmbientContext>>>(AmbientContextAccessorStackName)
                ?? throw new InvalidOperationException("Unable to retrieve disposed ambient context accessor override.");
            if (!ReferenceEquals(ambientContextAccessorStack.Peek(), _ambientContextAccessor))
            {
                throw new InvalidOperationException("Ambient context accessor overriding order is broken.");
            }
            ambientContextAccessorStack.Pop();
            _ambientContextAccessor = null;
        }
    }
}
