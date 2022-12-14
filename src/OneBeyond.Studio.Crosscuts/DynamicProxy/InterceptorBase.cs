using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Reflection;

namespace OneBeyond.Studio.Crosscuts.DynamicProxy;

/// <summary>
/// Implements base algorithm for intercepting method calls.
/// </summary>
public abstract class InterceptorBase : IInterceptor
{
    private static readonly Type TaskType = typeof(Task);
    private static readonly Type VoidType = typeof(void);
    private static readonly MethodInfo GenericExecuteMethodDefinition =
        Reflector.MethodFrom(
                (InterceptorBase interceptor) => interceptor.Execute<int>(default(IInvocation)!))
            .GetGenericMethodDefinition();
    private static readonly ConcurrentDictionary<Type, Func<InterceptorBase, IInvocation, object>> GenericExecuteFuncs =
        new ConcurrentDictionary<Type, Func<InterceptorBase, IInvocation, object>>();
    private static readonly MethodInfo GenericExecuteAsyncMethodDefinition =
        Reflector.MethodFrom(
                (InterceptorBase interceptor) => interceptor.ExecuteAsync<int>(default(IInvocation)!))
            .GetGenericMethodDefinition();
    private static readonly ConcurrentDictionary<Type, Func<InterceptorBase, IInvocation, object>> GenericExecuteAsyncFuncs =
        new ConcurrentDictionary<Type, Func<InterceptorBase, IInvocation, object>>();

    void IInterceptor.Intercept(IInvocation invocation)
    {
        EnsureArg.IsNotNull(invocation, nameof(invocation));

        PreExecute(invocation);

        var returnValueType = invocation.MethodInvocationTarget.ReturnType;

        if (TaskType == returnValueType)
        {
            invocation.ReturnValue = ExecuteAsync(invocation);
        }
        else if (TaskType.IsAssignableFrom(returnValueType))
        {
            var executeAsyncFunc = GenericExecuteAsyncFuncs.GetOrAdd(
                returnValueType,
                (returnValueType) =>
                {
                    var interceptorParameter = Expression.Parameter(typeof(InterceptorBase));
                    var invocationParameter = Expression.Parameter(typeof(IInvocation));
                    var executeAsyncMethodCall = Expression.Call(
                        interceptorParameter,
                        GenericExecuteAsyncMethodDefinition
                            .MakeGenericMethod(returnValueType.GetGenericArguments()[0]),
                        invocationParameter);
                    var lambdaBody = executeAsyncMethodCall;
                    var lambda = Expression.Lambda<Func<InterceptorBase, IInvocation, object>>(
                        lambdaBody,
                        interceptorParameter,
                        invocationParameter);
                    return lambda.Compile();
                });
            invocation.ReturnValue = executeAsyncFunc(this, invocation);
        }
        else if (VoidType.Equals(returnValueType))
        {
            Execute(invocation);
        }
        else
        {
            var executeFunc = GenericExecuteFuncs.GetOrAdd(
                returnValueType,
                (returnValueType) =>
                {
                    var interceptorParameter = Expression.Parameter(typeof(InterceptorBase));
                    var invocationParameter = Expression.Parameter(typeof(IInvocation));
                    var executeMethodCall = Expression.Call(
                        interceptorParameter,
                        GenericExecuteMethodDefinition
                            .MakeGenericMethod(returnValueType),
                        invocationParameter);
                    var lambdaBody = returnValueType.IsValueType
                        ? Expression.Convert(executeMethodCall, typeof(object)) as Expression
                        : executeMethodCall;
                    var lambda = Expression.Lambda<Func<InterceptorBase, IInvocation, object>>(
                        lambdaBody,
                        interceptorParameter,
                        invocationParameter);
                    return lambda.Compile();
                });
            invocation.ReturnValue = executeFunc(this, invocation);
        }
    }

    /// <summary>
    /// Override to do any stuff before an invocation gets executed.
    /// </summary>
    /// <param name="invocation"></param>
    protected virtual void PreExecute(IInvocation invocation)
    {
    }

    /// <summary>
    /// Override to handle a sync execution without any return value.
    /// </summary>
    /// <param name="execution"></param>
    protected virtual void Execute(ISyncExecution execution)
    {
        EnsureArg.IsNotNull(execution, nameof(execution));
        execution.Execute();
    }

    /// <summary>
    /// Override to handle a sync execution with return value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="execution"></param>
    /// <returns></returns>
    protected virtual T Execute<T>(ISyncExecution<T> execution)
    {
        EnsureArg.IsNotNull(execution, nameof(execution));
        return execution.Execute();
    }

    /// <summary>
    /// Override to handle an async execution without return value.
    /// </summary>
    /// <param name="execution"></param>
    /// <returns></returns>
    protected virtual Task ExecuteAsync(IAsyncExecution execution)
    {
        EnsureArg.IsNotNull(execution, nameof(execution));
        return execution.ExecuteAsync();
    }

    /// <summary>
    /// Override to handle an async execution with return value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="execution"></param>
    /// <returns></returns>
    protected virtual Task<T> ExecuteAsync<T>(IAsyncExecution<T> execution)
    {
        EnsureArg.IsNotNull(execution, nameof(execution));
        return execution.ExecuteAsync();
    }

    private void Execute(IInvocation invocation)
    {
        var execution = new Execution(invocation);
        Execute(execution);
    }

    private T Execute<T>(IInvocation invocation)
    {
        var execution = new Execution<T>(invocation);
        return Execute(execution);
    }

    private Task ExecuteAsync(IInvocation invocation)
    {
        var execution = new AsyncExecution(invocation);
        return ExecuteAsync(execution);
    }

    private Task<T> ExecuteAsync<T>(IInvocation invocation)
    {
        var execution = new AsyncExecution<T>(invocation);
        return ExecuteAsync(execution);
    }

    /// <summary>
    /// </summary>
    protected interface IExecution
    {
        /// <summary>
        /// </summary>
        MethodInfo Method { get; }
        /// <summary>
        /// </summary>
        object Target { get; }
        /// <summary>
        /// </summary>
        IReadOnlyCollection<object> Arguments { get; }
    }

    /// <summary>
    /// </summary>
    protected interface ISyncExecution : IExecution
    {
        /// <summary>
        /// </summary>
        internal void Execute();
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    protected interface ISyncExecution<out T> : IExecution
    {
        /// <summary>
        /// </summary>
        internal T Execute();
    }

    /// <summary>
    /// </summary>
    protected interface IAsyncExecution : IExecution
    {
        /// <summary>
        /// </summary>
        internal Task ExecuteAsync();
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    protected interface IAsyncExecution<T> : IExecution
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal Task<T> ExecuteAsync();
    }

    private abstract class ExecutionBase
    {
        protected ExecutionBase(IInvocation invocation)
        {
            Invocation = invocation;
        }

        public MethodInfo Method => Invocation.Method;
        public object Target => Invocation.InvocationTarget;
        public IReadOnlyCollection<object> Arguments => Invocation.Arguments;
        protected IInvocation Invocation { get; }
    }

    private sealed class Execution : ExecutionBase, ISyncExecution
    {
        public Execution(IInvocation invocation)
            : base(invocation)
        {
        }

        void ISyncExecution.Execute()
        {
            Invocation.Proceed();
        }
    }

    private sealed class Execution<T> : ExecutionBase, ISyncExecution<T>
    {
        public Execution(IInvocation invocation)
            : base(invocation)
        {
        }

        T ISyncExecution<T>.Execute()
        {
            Invocation.Proceed();
            return (T)Invocation.ReturnValue;
        }
    }

    private sealed class AsyncExecution : ExecutionBase, IAsyncExecution
    {
        public AsyncExecution(IInvocation invocation)
            : base(invocation)
        {
        }

        Task IAsyncExecution.ExecuteAsync()
        {
            Invocation.Proceed();
            return (Task)Invocation.ReturnValue;
        }
    }

    private sealed class AsyncExecution<T> : ExecutionBase, IAsyncExecution<T>
    {
        public AsyncExecution(IInvocation invocation)
            : base(invocation)
        {
        }

        Task<T> IAsyncExecution<T>.ExecuteAsync()
        {
            Invocation.Proceed();
            return (Task<T>)Invocation.ReturnValue;
        }
    }
}
