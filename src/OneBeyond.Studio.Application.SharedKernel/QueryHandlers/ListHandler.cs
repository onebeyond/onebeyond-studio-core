using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Entities.Dto;
using OneBeyond.Studio.Application.SharedKernel.Entities.Queries;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.Core.Mediator.Queries;
using OneBeyond.Studio.Crosscuts.Expressions;
using OneBeyond.Studio.Crosscuts.Strings;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

#nullable enable

namespace OneBeyond.Studio.Application.SharedKernel.QueryHandlers;

/// <summary>
/// </summary>
/// <typeparam name="TResultDto"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
public class ListHandler<TResultDto, TEntity, TEntityId>
    : ListHandler<List<TResultDto, TEntity, TEntityId>, TResultDto, TEntity, TEntityId>
    where TEntity : DomainEntity<TEntityId>
    where TEntityId : notnull
{
    /// <summary>
    /// </summary>
    /// <param name="repository"></param>
    public ListHandler(IRORepository<TEntity, TEntityId> repository)
        : base(repository)
    {
    }
}

/// <summary>
/// </summary>
public abstract class ListHandler<TQuery, TResultDto, TEntity, TEntityId>
    : IQueryHandler<TQuery, PagedList<TResultDto>>
    where TQuery : List<TResultDto, TEntity, TEntityId>
    where TEntity : DomainEntity<TEntityId>
    where TEntityId : notnull
{
    /// <summary>
    /// </summary>
    /// <param name="repository"></param>
    protected ListHandler(IRORepository<TEntity, TEntityId> repository)
    {
        EnsureArg.IsNotNull(repository, nameof(repository));

        Repository = repository;
    }

    /// <summary>
    /// </summary>
    protected IRORepository<TEntity, TEntityId> Repository { get; }

    /// <summary>
    /// </summary>
    public virtual async Task<PagedList<TResultDto>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(query, nameof(query));

        var preFilterExpression = await GetPreFilterExpressionAsync(
            query,
            cancellationToken).ConfigureAwait(false);

        var filterExpression = await GetFilterExpressionAsync(
            query,
            cancellationToken).ConfigureAwait(false);

        var sortExpressions = await GetSortExpressionsAsync(
            query,
            cancellationToken).ConfigureAwait(false);

        var paging = query.PageSize == null
            ? null
            : new Paging((query.PageNo - 1) * query.PageSize.Value, query.PageSize.Value);

        var sorting = sortExpressions;

        var pagedList = new PagedList<TResultDto>
        {
            Data = await Repository.ListAsync(
                preFilterExpression,
                filterExpression,
                paging,
                sorting,
                cancellationToken).ConfigureAwait(false),
            Count = await Repository.CountAsync(
                preFilterExpression,
                filterExpression,
                cancellationToken).ConfigureAwait(false)
        };

        return pagedList;
    }

    /// <summary>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask<Expression<Func<TEntity, bool>>?> GetPreFilterExpressionAsync(
        TQuery query,
        CancellationToken cancellationToken)
        => default;

    /// <summary>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask<Expression<Func<TResultDto, bool>>?> GetFilterExpressionAsync(
        TQuery query,
        CancellationToken cancellationToken)
        => new ValueTask<Expression<Func<TResultDto, bool>>?>(
            GetFilterExpression(query.FilterByFields, query.SearchText, query.ParentId));

    /// <summary>
    /// </summary>
    /// <param name="filterByFields"></param>
    /// <returns></returns>
    protected virtual Expression<Func<TResultDto, bool>>? GetFilterExpression(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> filterByFields)
        => FilterExpressionBuilder<TResultDto>.Build(filterByFields);

    /// <summary>
    /// </summary>
    protected virtual Expression<Func<TResultDto, bool>>? GetSearchExpression(string searchText)
        => default;

    /// <summary>
    /// Defines an expression checking whether a <typeparamref name="TResultDto"/> is bound to
    /// the specified <paramref name="parentId"/>.
    /// </summary>
    protected virtual Expression<Func<TResultDto, bool>>? GetParentIdExpression(string parentId)
        => default;

    /// <summary>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask<IReadOnlyCollection<Sorting<TResultDto>>?> GetSortExpressionsAsync(
        TQuery query,
        CancellationToken cancellationToken)
        => new ValueTask<IReadOnlyCollection<Sorting<TResultDto>>?>(
            GetSortExpressions(query.SortByFields, query.SortDirection));

    /// <summary>
    /// Defines an expression used for sorting a list of <typeparamref name="TResultDto"/> objects
    /// by the specified <paramref name="sortByFields"/> field name.
    /// </summary>
    protected virtual IReadOnlyCollection<Sorting<TResultDto>>? GetSortExpressions(
        IReadOnlyCollection<string> sortByFields,
        ListSortDirection defaultDirection)
        => sortByFields.Count <= 0
            ? default
            : SortExpressionBuilder<TResultDto>.Build(sortByFields, defaultDirection);

    private Expression<Func<TResultDto, bool>>? GetFilterExpression(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> filterByFields,
        string? searchText,
        string? parentId)
    {
        var filterExpression = filterByFields.Count == 0
            ? default
            : GetFilterExpression(filterByFields);

        var searchExpression = searchText.IsNullOrEmpty()
            ? default
            : GetSearchExpression(searchText);

        var parentIdExpression = parentId.IsNullOrWhiteSpace()
            ? default
            : GetParentIdExpression(parentId);

        var expression = CombineExpressions(parentIdExpression, filterExpression);

        expression = CombineExpressions(expression, searchExpression);

        return expression;
    }

    private Expression<Func<TResultDto, bool>>? CombineExpressions(
        Expression<Func<TResultDto, bool>>? first,
        Expression<Func<TResultDto, bool>>? second)
    {
        return first is null
            ? second
            : second is null
                ? first
                : first.And(second);
    }
}
