using MediatR;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.Authorization;

internal static class TestableCommands
{
    public interface IRequestWithSomething1 : IBaseRequest
    {
    }

    public interface IRequestWithSomething2 : IBaseRequest
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2))]
    public sealed record Command1 : IRequest<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2))]
    public sealed record Command2 : IRequest<bool>, IRequestWithSomething1
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2))]
    public sealed record Command3 : IRequest<bool>, IRequestWithSomething2
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement1), new object[] { true, 42, "Forty two" },
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { false })]
    public sealed record Command4 : IRequest<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2), new object[] { },
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { false })]
    public sealed record Command5 : IRequest<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement1), new object[] { true, 41, "Forty one" },
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { true })]
    public sealed record Command9 : IRequest<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2))]
    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { false })]
    public sealed record Command6 : IRequest<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { true })]
    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { false })]
    public sealed record Command7 : IRequest<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement1), new object[] { false, 45, "Forty five" })]
    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { true })]
    public sealed record Command8 : IRequest<bool>
    {
    }

    public sealed record Command10 : IRequest<bool>
    {
    }
}
