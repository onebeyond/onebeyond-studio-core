using System;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// </summary>
public class Address : ValueObject, IEquatable<Address>
{
    /// <summary>
    /// </summary>
    /// <param name="postcode"></param>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <param name="town"></param>
    /// <param name="county"></param>
    public Address(
        string postcode,
        string line1,
        string line2,
        string? town,
        string? county)
    {
        EnsureArg.IsNotNullOrWhiteSpace(postcode, nameof(postcode));
        EnsureArg.IsNotNull(line1, nameof(line1));
        EnsureArg.IsNotNull(line2, nameof(line2));

        Postcode = postcode.Trim();
        Line1 = line1.Trim();
        Line2 = line2.Trim();
        Town = town?.Trim();
        County = county?.Trim();
    }

    //This private constructor is needed for EF Core to construct an entity from database table row
#nullable disable
    private Address()
#nullable restore
    {
    }

    /// <summary>
    /// </summary>
    public string Postcode { get; }
    /// <summary>
    /// </summary>
    public string Line1 { get; }
    /// <summary>
    /// </summary>
    public string Line2 { get; }
    /// <summary>
    /// </summary>
    public string? Town { get; }
    /// <summary>
    /// </summary>
    public string? County { get; }

    /// <summary>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Address? other)
    {
        return other is { }
            && Postcode.Equals(other.Postcode, StringComparison.OrdinalIgnoreCase)
            && Line1.Equals(other.Line1, StringComparison.OrdinalIgnoreCase)
            && Line2.Equals(other.Line2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
        => obj is Address address && Equals(address);

    /// <summary>
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static bool operator ==(Address obj1, Address obj2)
    {
        return Equals(obj1, null)
            ? Equals(obj2, null)
            : obj1.Equals(obj2);
    }

    /// <summary>
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static bool operator !=(Address obj1, Address obj2)
        => !(obj1 == obj2);

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
        => base.GetHashCode();
}
