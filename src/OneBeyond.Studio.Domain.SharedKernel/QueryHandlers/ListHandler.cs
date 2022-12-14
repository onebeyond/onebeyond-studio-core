using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using MediatR;
using OneBeyond.Studio.Crosscuts.Expressions;
using OneBeyond.Studio.Crosscuts.Strings;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Dto;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Queries;
using OneBeyond.Studio.Domain.SharedKernel.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

#nullable enable

namespace OneBeyond.Studio.Domain.SharedKernel.QueryHandlers;

/// <summary>
/// </summary>
/// <typeparam name="TResultDTO"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
public class ListHandler<TResultDTO, TEntity, TEntityId>
    : ListHandler<List<TResultDTO, TEntity, TEntityId>, TResultDTO, TEntity, TEntityId>
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
public abstract class ListHandler<TQuery, TResultDTO, TEntity, TEntityId>
    : IRequestHandler<TQuery, PagedList<TResultDTO>>
    where TQuery : List<TResultDTO, TEntity, TEntityId>
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
    public virtual async Task<PagedList<TResultDTO>> Handle(
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

        var pagedList = new PagedList<TResultDTO>
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
    protected virtual ValueTask<Expression<Func<TResultDTO, bool>>?> GetFilterExpressionAsync(
        TQuery query,
        CancellationToken cancellationToken)
        => new ValueTask<Expression<Func<TResultDTO, bool>>?>(
            GetFilterExpression(query.FilterByFields, query.SearchText, query.ParentId));

    /// <summary>
    /// </summary>
    /// <param name="filterByFields"></param>
    /// <returns></returns>
    protected virtual Expression<Func<TResultDTO, bool>>? GetFilterExpression(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> filterByFields)
        => FilterExpressionBuilder<TResultDTO>.Build(filterByFields);

    /// <summary>
    /// </summary>
    protected virtual Expression<Func<TResultDTO, bool>>? GetSearchExpression(string searchText)
        => default;

    /// <summary>
    /// Defines an expression checking whether a <typeparamref name="TResultDTO"/> is bound to
    /// the specified <paramref name="parentId"/>.
    /// </summary>
    protected virtual Expression<Func<TResultDTO, bool>>? GetParentIdExpression(string parentId)
        => default;

    /// <summary>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask<IReadOnlyCollection<Sorting<TResultDTO>>?> GetSortExpressionsAsync(
        TQuery query,
        CancellationToken cancellationToken)
        => new ValueTask<IReadOnlyCollection<Sorting<TResultDTO>>?>(
            GetSortExpressions(query.SortByFields, query.SortDirection));

    /// <summary>
    /// Defines an expression used for sorting a list of <typeparamref name="TResultDTO"/> objects
    /// by the specified <paramref name="sortByFields"/> field name.
    /// </summary>
    protected virtual IReadOnlyCollection<Sorting<TResultDTO>>? GetSortExpressions(
        IReadOnlyCollection<string> sortByFields,
        ListSortDirection defaultDirection)
        => sortByFields.Count <= 0
            ? default
            : SortExpressionBuilder<TResultDTO>.Build(sortByFields, defaultDirection);

    private Expression<Func<TResultDTO, bool>>? GetFilterExpression(
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

    private Expression<Func<TResultDTO, bool>>? CombineExpressions(
        Expression<Func<TResultDTO, bool>>? first,
        Expression<Func<TResultDTO, bool>>? second)
    {
        return first is null
            ? second
            : second is null
                ? first
                : first.And(second);
    }
}
