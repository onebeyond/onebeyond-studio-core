using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.Authorization;

internal static class TestableAuthorizationRequirements
{
    public sealed record Requirement1 : AuthorizationRequirement
    {
        public Requirement1(bool isFailRequested, int intValue, string stringValue)
        {
            IsFailRequested = isFailRequested;
            IntValue = intValue;
            StringValue = stringValue;
        }

        public bool IsFailRequested { get; }
        public int IntValue { get; }
        public string StringValue { get; }
    }

    public sealed record Requirement2 : AuthorizationRequirement
    {
    }

    public sealed record Requirement3 : AuthorizationRequirement
    {
        public Requirement3(bool isFailRequested)
        {
            IsFailRequested = isFailRequested;
        }

        public bool IsFailRequested { get; }
    }
}
