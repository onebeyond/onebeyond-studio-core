using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Specifications;

/// <summary>
/// </summary>
public static class SortExpressionBuilder<T>
{
    private static readonly Type TType = typeof(T);
    private static readonly Type ObjectType = typeof(object);

    /// <summary>
    /// Return the casing policies in standard order (last one is the caseInsensitive)
    /// </summary>
    public static readonly CasingPolicy[] OrderedDefaultPolicies = new CasingPolicy[]
    {
            CasingPolicy.SentenceCase,
            CasingPolicy.FirstLetterCase,
            CasingPolicy.LowerCase,
            CasingPolicy.CaseInsensitive
    };

    /// <summary>
    /// </summary>
    public static IReadOnlyCollection<Sorting<T>> Build(
        IReadOnlyCollection<string> sortByFields,
        ListSortDirection defaultDirection,
        params CasingPolicy[] policies)
    {
        EnsureArg.IsNotNull(sortByFields, nameof(sortByFields));

        if (policies is null || policies.Length == 0)
        {
            policies = OrderedDefaultPolicies;
        }

        var sortings = sortByFields
            .Select(
                (sortByField) => ParseFieldNameAndDirection(sortByField, defaultDirection))
            .Select(
                (sortingItem) => BuildSorting(sortingItem, policies))
            .ToArray();

        return sortings;
    }

    private static (string FieldName, ListSortDirection Direction) ParseFieldNameAndDirection(
        string sortByField,
        ListSortDirection defaultDirection)
    {
        EnsureArg.IsNotNullOrWhiteSpace(sortByField, nameof(sortByField));

        var fieldNameAndDirection = sortByField.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
            .Select((stringValue) => stringValue.Trim())
            .ToArray();
        var fieldName = default(string);
        var direction = default(ListSortDirection?);
        if (fieldNameAndDirection.Length == 2)
        {
            fieldName = fieldNameAndDirection[0];
            direction = fieldNameAndDirection[1] switch
            {
                "asc" => ListSortDirection.Ascending,
                "desc" => ListSortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(sortByField),
                    $"Unable to match sorting direction {fieldNameAndDirection[1]} neither to 'asc' nor to 'desc'")
            };
        }
        else if (fieldNameAndDirection.Length == 1)
        {
            fieldName = fieldNameAndDirection[0];
            direction = defaultDirection;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Unable to parse sorting string {sortByField} as 'fieldName[:asc|desc]'");
        }

        return (FieldName: fieldName, Direction: direction.Value);
    }

    private static Sorting<T> BuildSorting(
        (string FieldName, ListSortDirection Direction) sortingItem,
        CasingPolicy[] policies)
    {
        var parameterExpression = Expression.Parameter(TType, "entity");

        var pathExpression = GetPathExpression(parameterExpression, sortingItem.FieldName, policies);

        var convertExpression = Expression.Convert(pathExpression, ObjectType);

        var expression = Expression.Lambda<Func<T, object?>>(convertExpression, parameterExpression);

        return sortingItem.Direction switch
        {
            ListSortDirection.Ascending => Sorting.CreateAscending(expression),
            ListSortDirection.Descending => Sorting.CreateDescending(expression),
            _ => throw new InvalidOperationException()
        };
    }

    private static MemberExpression GetPathExpression(
        Expression pathRootExpression,
        string path,
        CasingPolicy[] policies)
    {
        EnsureArg.IsNotNullOrWhiteSpace(path, nameof(path));

        var pathParts = path.Split(new char[] { '.' }, 2);

        if (pathParts.Length == 2) // Nested path
        {
            pathRootExpression = GetPathExpression(pathRootExpression, pathParts[0], policies);
            return GetPathExpression(pathRootExpression, pathParts[1], policies);
        }
        else
        {
            var propertyInfo = policies.GetProperty(pathRootExpression.Type, path)
                ?? throw new ArgumentException(
                    $"The {pathRootExpression.Type.FullName} type doesn't contain a property called {path}.",
                    nameof(path));
            return Expression.PropertyOrField(pathRootExpression, propertyInfo.Name);
        }
    }
}
