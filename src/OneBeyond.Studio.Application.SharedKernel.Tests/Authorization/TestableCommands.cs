using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;

internal static class TestableCommands
{
    public interface IRequestWithSomething1 : ICommand
    {
    }

    public interface IRequestWithSomething2 : ICommand
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2))]
    public sealed record Command1 : ICommand<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2))]
    public sealed record Command2 : ICommand<bool>, IRequestWithSomething1
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2))]
    public sealed record Command3 : ICommand<bool>, IRequestWithSomething2
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement1), new object[] { true, 42, "Forty two" },
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { false })]
    public sealed record Command4 : ICommand<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2), new object[] { },
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { false })]
    public sealed record Command5 : ICommand<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement1), new object[] { true, 41, "Forty one" },
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { true })]
    public sealed record Command9 : ICommand<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement2))]
    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { false })]
    public sealed record Command6 : ICommand<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { true })]
    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { false })]
    public sealed record Command7 : ICommand<bool>
    {
    }

    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement1), new object[] { false, 45, "Forty five" })]
    [AuthorizationPolicy(
        typeof(TestableAuthorizationRequirements.Requirement3), new object[] { true })]
    public sealed record Command8 : ICommand<bool>
    {
    }

    public sealed record Command10 : ICommand<bool>
    {
    }
}
