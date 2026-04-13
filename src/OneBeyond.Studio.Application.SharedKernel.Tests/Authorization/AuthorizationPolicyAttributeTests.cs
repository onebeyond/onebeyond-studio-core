using System;
using Xunit;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;


public sealed class AuthorizationPolicyAttributeTests
{
    [Fact]
    public void TestAuthorizationPolicyAttributeThrowsExceptionWhenNoRequirementsProvided()
    {
        try
        {
            var policy = new AuthorizationPolicyAttribute();

            Assert.Fail();
        }
        catch (ArgumentException exception)
        {
            Assert.Equal("Empty collection is not allowed. (Parameter 'requirementTypes')", exception.Message);
            Assert.Equal("requirementTypes", exception.ParamName);
        }
    }
}

