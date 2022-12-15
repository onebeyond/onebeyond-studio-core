namespace OneBeyond.Studio.Domain.SharedKernel.AmbientContexts;

/// <summary>
/// This interface is used for accessing ambient context.
/// </summary>
public interface IAmbientContextAccessor
{
    AmbientContext AmbientContext { get; }
}

/// <summary>
/// This interface is used for accessing ambient context.
/// </summary>
public interface IAmbientContextAccessor<out TAmbientContext> : IAmbientContextAccessor
    where TAmbientContext : AmbientContext
{
    new TAmbientContext AmbientContext { get; }

    AmbientContext IAmbientContextAccessor.AmbientContext => AmbientContext;
}
