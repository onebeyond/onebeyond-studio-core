using System;
using System.Linq.Expressions;
using System.Reflection;

namespace OneBeyond.Studio.Crosscuts.Reflection;

/// <summary>
/// Contains helper methods for extracting reflection information in a strongly typed manner.
/// </summary>
public static class Reflector
{
    /// <summary>
    /// Extracts <see cref="MethodInfo"/> from a static method call.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="prototype"></param>
    /// <returns></returns>
    public static MethodInfo MethodFrom<TResult>(Expression<Func<TResult>> prototype)
        => MethodFrom((LambdaExpression)prototype);

    /// <summary>
    /// Extracts <see cref="MethodInfo"/> from a static method call.
    /// </summary>
    /// <param name="prototype"></param>
    /// <returns></returns>
    public static MethodInfo MethodFrom(Expression<Action> prototype)
        => MethodFrom((LambdaExpression)prototype);

    /// <summary>
    /// Extracts <see cref="MethodInfo"/> from a member method call.
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="prototype"></param>
    /// <returns></returns>
    public static MethodInfo MethodFrom<TClass, TResult>(Expression<Func<TClass, TResult>> prototype)
        => MethodFrom((LambdaExpression)prototype);

    /// <summary>
    /// Extracts <see cref="MethodInfo"/> from a member method call.
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    /// <param name="prototype"></param>
    /// <returns></returns>
    public static MethodInfo MethodFrom<TClass>(Expression<Action<TClass>> prototype)
        => MethodFrom((LambdaExpression)prototype);

    private static MethodInfo MethodFrom(LambdaExpression prototype)
        => ((MethodCallExpression)prototype.Body).Method;
}
