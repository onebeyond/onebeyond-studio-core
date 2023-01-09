using System;
using System.Collections.Generic;
using System.ComponentModel;
using EnsureThat;
using MediatR;
using OneBeyond.Studio.Application.SharedKernel.Entities.Dto;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Application.SharedKernel.Entities.Queries;

/// <summary>
/// </summary>
public record List<TResultDto, TEntity, TEntityId> : IRequest<PagedList<TResultDto>>
    where TEntity : DomainEntity<TEntityId>
{
    /// <summary>
    /// </summary>
    public List(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>>? filterByFields = default,
        string? searchText = default,
        string? parentId = default,
        IReadOnlyCollection<string>? sortByFields = default,
        ListSortDirection sortDirection = ListSortDirection.Ascending,
        int pageNo = 1,
        int? pageSize = default)
    {
        EnsureArg.IsGte(pageNo, 1, nameof(pageNo));
        EnsureArg.IsTrue(pageSize is null || pageSize > 0, nameof(pageSize));

        FilterByFields = filterByFields ?? new Dictionary<string, IReadOnlyCollection<string>>();
        SearchText = searchText;
        ParentId = parentId;
        SortByFields = sortByFields ?? Array.Empty<string>();
        SortDirection = sortDirection;
        PageNo = pageNo;
        PageSize = pageSize;
    }

    /// <summary>
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> FilterByFields { get; }
    /// <summary>
    /// </summary>
    public string? SearchText { get; }
    /// <summary>
    /// </summary>
    public string? ParentId { get; }
    /// <summary>
    /// </summary>
    public IReadOnlyCollection<string> SortByFields { get; }
    /// <summary>
    /// </summary>
    public ListSortDirection SortDirection { get; }
    /// <summary>
    /// </summary>
    public int PageNo { get; }
    /// <summary>
    /// </summary>
    public int? PageSize { get; }
}
