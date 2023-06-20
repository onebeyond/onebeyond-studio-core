using EnsureThat;
using Newtonsoft.Json;

namespace OneBeyond.Studio.ChangeTracker.Domain.Entities;

public sealed record PropertyChange
{
    [JsonConstructor]
    public PropertyChange(
        string name,
        string type,
        object? before,
        object? after)
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
        EnsureArg.IsNotNullOrWhiteSpace(type, nameof(type));

        Name = name;
        Type = type;
        Before = before;
        After = after;
    }

    public PropertyChange(
        string propertyName,
        Type propertyType,
        object? before,
        object? after)
    {
        EnsureArg.IsNotNullOrWhiteSpace(propertyName, nameof(propertyName));
        EnsureArg.IsNotNull(propertyType, nameof(propertyType));
        Name = propertyName;
        var isNullable = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        Type = isNullable ? propertyType.GetGenericArguments()[0].FullName! : propertyType.FullName!;
        Before = before;
        After = after;
    }

    public string Name { get; }
    public string Type { get;  }
    public object? Before { get;  }
    public object? After { get;  }
}
