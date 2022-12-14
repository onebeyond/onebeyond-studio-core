using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.Authorization;

[TestClass]
public sealed class AuthorizationPolicyAttributeTests
{
    [TestMethod]
    public void TestAuthorizationPolicyAttributeThrowsExceptionWhenNoRequirementsProvided()
    {
        try
        {
            var policy = new AuthorizationPolicyAttribute();

            Assert.Fail();
        }
        catch (ArgumentException exception)
        {
            Assert.AreEqual("Empty collection is not allowed. (Parameter 'requirementTypes')", exception.Message);
            Assert.AreEqual("requirementTypes", exception.ParamName);
        }
    }
}
