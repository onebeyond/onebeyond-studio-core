using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// </summary>
public class PhoneNumber : ValueObject
{
    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    public PhoneNumber(string value)
    {
        EnsureArg.IsNotNullOrWhiteSpace(value, nameof(value));

        Value = value;
    }

    //This private constructor is needed for EF Core to construct an entity from database table row
#nullable disable
    private PhoneNumber()
#nullable restore
    {
    }

    /// <summary>
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => Value;
}
