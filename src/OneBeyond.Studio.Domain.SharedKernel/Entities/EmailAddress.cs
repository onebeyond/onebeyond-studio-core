using System.Net.Mail;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// </summary>
public class EmailAddress : ValueObject
{
    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    public EmailAddress(string value)
    {
        EnsureArg.IsNotNullOrWhiteSpace(value, nameof(value));

        Value = new MailAddress(value).Address;
    }

    //This private constructor is needed for EF Core to construct an entity from database table row
#nullable disable
    private EmailAddress()
#nullable restore
    {
    }

    /// <summary>
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => Value;
}
