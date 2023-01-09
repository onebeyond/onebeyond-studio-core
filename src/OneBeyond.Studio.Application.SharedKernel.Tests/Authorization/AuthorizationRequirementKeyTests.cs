using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;

[TestClass]
public sealed class AuthorizationRequirementKeyTests : AuthorizationRequirementBehavior
{
    [TestMethod]
    public void TestAuthorizationRequirementKeyProperlyImplementsIEquatable()
    {
        var key1 = new AuthorizationRequirementKey(typeof(string), new object[] { 42, "42" });
        var key2 = new AuthorizationRequirementKey(typeof(string), new object[] { 42, "42" });
        var key3 = new AuthorizationRequirementKey(typeof(int), new object[] { 42, "42" });
        var key4 = new AuthorizationRequirementKey(typeof(string), new object[] { "42", 42 });
        var key5 = key1;

        Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
        Assert.IsTrue(key1.Equals(key2));
        Assert.AreEqual(key1, key2);

        Assert.AreEqual(key1.GetHashCode(), key5.GetHashCode());
        Assert.IsTrue(key1.Equals(key5));
        Assert.AreEqual(key1, key5);

        Assert.AreNotEqual(key1.GetHashCode(), key3.GetHashCode());

        Assert.AreNotEqual(key1.GetHashCode(), key4.GetHashCode());
    }
}
