using System;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// source: https://github.com/jhewlett/ValueObject
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IgnoreMemberAttribute : Attribute
{
}
