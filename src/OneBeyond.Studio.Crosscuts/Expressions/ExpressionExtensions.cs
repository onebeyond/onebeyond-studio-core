using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using EnsureThat;

namespace OneBeyond.Studio.Crosscuts.Expressions;

/// <summary>
/// Extension methods for various types of <see cref="Expression"/>
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Replaces all occurences of <paramref name="source"/> paramter by <paramref name="target"/> parameter in the expression.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static TExpression ReplaceParameter<TExpression>(this TExpression @this, ParameterExpression source, ParameterExpression target)
        where TExpression : Expression
    {
        EnsureArg.IsNotNull(@this, nameof(@this));

        var parameterReplacer = new ExpressionReplacer(source, target);
        return (TExpression)parameterReplacer.Visit(@this);
    }

    public static TExpression ReplaceExpression<TExpression>(this TExpression @this, Expression source, Expression target)
        where TExpression : Expression
    {
        EnsureArg.IsNotNull(@this, nameof(@this));

        var expressionReplacer = new ExpressionReplacer(source, target);
        return (TExpression)expressionReplacer.Visit(@this);
    }

    [return: NotNullIfNotNull("operand1"), NotNullIfNotNull("operand2")]
    public static Expression<Func<T, bool>>? And<T>(
        this Expression<Func<T, bool>>? operand1,
        Expression<Func<T, bool>>? operand2)
    {
        if (operand1 is null)
        {
            return operand2;
        }
        if (operand2 is null)
        {
            return operand1;
        }
        operand2 = operand2.ReplaceParameter(operand2.Parameters[0], operand1.Parameters[0]);
        var body = Expression.AndAlso(operand1.Body, operand2.Body);
        return Expression.Lambda<Func<T, bool>>(body, operand1.Parameters);
    }

    [return: NotNullIfNotNull("operand1"), NotNullIfNotNull("operand2")]
    public static Expression<Func<T, bool>>? Or<T>(
        this Expression<Func<T, bool>>? operand1,
        Expression<Func<T, bool>>? operand2)
    {
        if (operand1 is null)
        {
            return operand2;
        }
        if (operand2 is null)
        {
            return operand1;
        }
        operand2 = operand2.ReplaceParameter(operand2.Parameters[0], operand1.Parameters[0]);
        var body = Expression.OrElse(operand1.Body, operand2.Body);
        return Expression.Lambda<Func<T, bool>>(body, operand1.Parameters);
    }

    /// <summary>
    /// Given an expression for a method that takes in a single parameter and
    /// returns a bool, converts the parameter type of the parameter
    /// from TSource to TTarget.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <param name="this"></param>
    /// <returns></returns>
    // Given an expression for a method that takes in a single parameter (and
    // returns a bool), this method converts the parameter type of the parameter
    // from TSource to TTarget.
    public static Expression<Func<TTarget, bool>> ConvertParameterType<TSource, TTarget>(this Expression<Func<TSource, bool>> @this)
    {
        var visitor = new ParameterTypeConverter<TSource, TTarget>();
        return (Expression<Func<TTarget, bool>>)visitor.Visit(@this);
    }

    private sealed class ParameterTypeConverter<TSource, TTarget> : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression>? _parameters;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameters?.FirstOrDefault(p => p.Name == node.Name)
              ?? (node.Type == typeof(TSource)
                    ? Expression.Parameter(typeof(TTarget), node.Name)
                    : node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _parameters = VisitAndConvert(node.Parameters, "VisitLambda");
            return Expression.Lambda(Visit(node.Body), _parameters);
        }
    }

    private sealed class ExpressionReplacer : ExpressionVisitor
    {
        private readonly Expression _source;
        private readonly Expression _target;

        public ExpressionReplacer(Expression source, Expression target)
        {
            EnsureArg.IsNotNull(source, nameof(source));
            EnsureArg.IsNotNull(target, nameof(target));
            EnsureArg.IsTrue(
                source.Type == target.Type,
                default,
                (options) => options.WithMessage($"Source type {source.Type.FullName} is equal to target type {target.Type.FullName}."));

            _source = source;
            _target = target;
        }

        [return: NotNullIfNotNull("node")]
        public override Expression? Visit(Expression? node)
        {
            return _source.Equals(node)
                ? _target
                : base.Visit(node);
        }
    }
}
