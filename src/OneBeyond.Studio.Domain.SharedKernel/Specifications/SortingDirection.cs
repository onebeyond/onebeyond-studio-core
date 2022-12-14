using Ardalis.SmartEnum;
using Ardalis.SmartEnum.JsonNet;
using Newtonsoft.Json;

namespace OneBeyond.Studio.Domain.SharedKernel.Specifications;

/// <summary>
/// </summary>
[JsonConverter(typeof(SmartEnumNameConverter<SortingDirection, int>))]
public sealed class SortingDirection : SmartEnum<SortingDirection, int>
{
    /// <summary>
    /// </summary>
    public static readonly SortingDirection Ascending = new SortingDirection(nameof(Ascending), 0);
    /// <summary>
    /// </summary>
    public static readonly SortingDirection Descending = new SortingDirection(nameof(Descending), 1);

    private SortingDirection(string name, int value)
        : base(name, value)
    {
    }
}
