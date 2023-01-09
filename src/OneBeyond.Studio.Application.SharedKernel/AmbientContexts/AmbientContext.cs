using System;

namespace OneBeyond.Studio.Application.SharedKernel.AmbientContexts;

/// <summary>
/// This is a base class for any ambient context an app wants to maintain.
/// </summary>
[Serializable]
public abstract record AmbientContext
{
}
