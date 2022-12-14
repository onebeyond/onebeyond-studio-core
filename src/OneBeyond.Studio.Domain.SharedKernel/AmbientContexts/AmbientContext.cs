using System;

namespace OneBeyond.Studio.Domain.SharedKernel.AmbientContexts;

/// <summary>
/// This is a base class for any ambient context an app wants to maintain.
/// </summary>
[Serializable]
public abstract record AmbientContext
{
}
