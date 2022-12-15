namespace OneBeyond.Studio.Domain.SharedKernel.Entities.Dto;

/// <summary>
/// </summary>
/// <typeparam name="TItemId"></typeparam>
public record LookupItemDto<TItemId>
{
    /// <summary>
    /// </summary>
    public TItemId Id { get; init; } = default!;
    /// <summary>
    /// </summary>
    public string? Name { get; init; }
}
