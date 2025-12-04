using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using EnsureThat;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;
using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Core.Mediator.Pipelines;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Authorization;

public class AuthorizationRequirementBehavior<TRequest, TResponse>
    : AuthorizationRequirementBehavior
    , IMediatorPipelineBehaviour<TRequest, TResponse>
    where TRequest : ICommand
{
    private readonly ILifetimeScope _container;
    private readonly AuthorizationOptions _authorizationOptions;

    public AuthorizationRequirementBehavior(
        ILifetimeScope container,
        AuthorizationOptions authorizationOptions)
    {
        EnsureArg.IsNotNull(container, nameof(container));
        EnsureArg.IsNotNull(authorizationOptions, nameof(authorizationOptions));

        _container = container;
        _authorizationOptions = authorizationOptions;
    }

    private static readonly ILogger Logger = LogManager.CreateLogger<AuthorizationRequirementBehavior<TRequest, TResponse>>();
    private static readonly ConcurrentDictionary<Type, AuthorizationRequirementHandler> AuthorizationRequirementHandlerWrappers = new();

    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {        
        EnsureArg.IsNotNull(next, nameof(next));

        if (request is null)
        {
            throw new ArgumentException(nameof(request));
        }

        var requestType = request.GetType();

        var policies = (AuthorizationPolicyAttribute[])
            Attribute.GetCustomAttributes(requestType, typeof(AuthorizationPolicyAttribute));

        if (!_authorizationOptions.AllowUnattributedRequests
            && policies.Length == 0)
        {
            throw new AuthorizationPolicyMissingException(requestType);
        }

        foreach (var policy in policies)
        {
            var isPolicyMet = false;
            var requirementExceptions = new List<Exception>();

            foreach (var requirementType in policy.RequirementTypes)
            {
                Logger.LogInformation(
                    "Validating authorization requirement {AuthorizationRequirementType} on request {RequestType}",
                    requirementType.Key.FullName,
                    requestType.FullName);

                try
                {
                    var requirementHandlerType = typeof(IAuthorizationRequirementHandler<,>)
                        .MakeGenericType(requirementType.Key, requestType);
                    var requirementKey = new AuthorizationRequirementKey(requirementType.Key, requirementType.Value);
                    var requirement = AuthorizationRequirements.GetOrAdd(
                        requirementKey,
                        (_) => (AuthorizationRequirement)Activator.CreateInstance(
                            requirementType.Key,
                            requirementType.Value.ToArray())!);
                    var requirementHandlerWrapper = AuthorizationRequirementHandlerWrappers.GetOrAdd(
                        requirementType.Key,
                        (_) =>
                        {
                            var type1 = typeof(TRequest);
                            var type2 = typeof(TResponse);
                            var type3 = requirementType.Key;
                            var requirementHandlerWrapperType = typeof(AuthorizationRequirementHandler<>)
                                .MakeGenericType(typeof(TRequest), typeof(TResponse), requirementType.Key);
                            return (AuthorizationRequirementHandler)Activator.CreateInstance(
                                requirementHandlerWrapperType)!;
                        });

                    var requirementHandler = _container.Resolve(requirementHandlerType);

                    await requirementHandlerWrapper.HandleAsync(
                        requirementHandler,
                        requirement,
                        request,
                        cancellationToken).ConfigureAwait(false);

                    Logger.LogInformation(
                        "Authorization requirement {AuthorizationRequirementType} is met on request {RequestType}",
                        requirementType.Key.FullName,
                        requestType.FullName);

                    isPolicyMet = true;
                    break;
                }
                catch (Exception exception) when (!exception.IsCritical())
                {
                    requirementExceptions.Add(exception);
                }
            }

            if (!isPolicyMet)
            {
                throw new AuthorizationPolicyFailedException(policy, requestType, requirementExceptions);
            }
        }

        return await next().ConfigureAwait(false);
    }

    private abstract class AuthorizationRequirementHandler
    {
        public abstract Task HandleAsync(
            object requirementHandler,
            AuthorizationRequirement requirement,
            TRequest request,
            CancellationToken cancellationToken);
    }

    private sealed class AuthorizationRequirementHandler<TRequirement> : AuthorizationRequirementHandler
        where TRequirement : AuthorizationRequirement        
    {
        public override Task HandleAsync(
            object requirementHandler,
            AuthorizationRequirement requirement,
            TRequest request,
            CancellationToken cancellationToken)
        {
            return ((IAuthorizationRequirementHandler<TRequirement, TRequest>)requirementHandler)
                .HandleAsync((TRequirement)requirement, request, cancellationToken);
        }
    }
}

public abstract class AuthorizationRequirementBehavior
{
    protected static ConcurrentDictionary<AuthorizationRequirementKey, AuthorizationRequirement> AuthorizationRequirements { get; } = new();

    protected struct AuthorizationRequirementKey : IEquatable<AuthorizationRequirementKey>
    {
        private int _hashCode;
        private readonly Type _type;
        private readonly IReadOnlyCollection<object> _args;

        public AuthorizationRequirementKey(
            Type authorizationRequirementType,
            IReadOnlyCollection<object> authorizationRequirementArgs)
        {
            _hashCode = 0;
            _type = authorizationRequirementType;
            _args = authorizationRequirementArgs;
        }

        public override int GetHashCode()
        {
            if (_hashCode == 0)
            {
                _hashCode = _type.GetHashCode();
                foreach (var arg in _args)
                {
                    _hashCode = _hashCode * 31 + arg.GetHashCode();
                }
            }
            return _hashCode;
        }

        public bool Equals(AuthorizationRequirementKey other)
            => _type.Equals(other._type)
            && _args.SequenceEqual(other._args);

        public override bool Equals(object? obj)
            => obj is AuthorizationRequirementKey key && Equals(key);

        public static bool operator ==(AuthorizationRequirementKey left, AuthorizationRequirementKey right)
            => left.Equals(right);

        public static bool operator !=(AuthorizationRequirementKey left, AuthorizationRequirementKey right)
            => !(left == right);
    }
}
