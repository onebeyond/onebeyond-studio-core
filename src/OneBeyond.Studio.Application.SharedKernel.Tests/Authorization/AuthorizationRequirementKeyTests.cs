using Xunit;
using OneBeyond.Studio.Application.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;


public sealed class AuthorizationRequirementKeyTests : AuthorizationRequirementBehavior
{
    [Fact]
    public void TestAuthorizationRequirementKeyProperlyImplementsIEquatable()
    {
        var key1 = new AuthorizationRequirementKey(typeof(string), new object[] { 42, "42" });
        var key2 = new AuthorizationRequirementKey(typeof(string), new object[] { 42, "42" });
        var key3 = new AuthorizationRequirementKey(typeof(int), new object[] { 42, "42" });
        var key4 = new AuthorizationRequirementKey(typeof(string), new object[] { "42", 42 });
        var key5 = key1;

        Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
        Assert.True(key1.Equals(key2));
        Assert.Equal(key1, key2);

        Assert.Equal(key1.GetHashCode(), key5.GetHashCode());
        Assert.True(key1.Equals(key5));
        Assert.Equal(key1, key5);

        Assert.NotEqual(key1.GetHashCode(), key3.GetHashCode());

        Assert.NotEqual(key1.GetHashCode(), key4.GetHashCode());
    }
}


