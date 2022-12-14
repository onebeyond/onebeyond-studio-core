using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OneBeyond.Studio.Crosscuts.Json;

// https://github.com/danielwertheim/jsonnet-privatesetterscontractresolvers
/// <summary>
/// </summary>
public sealed class PrivateSetterContractResolver : DefaultContractResolver
{
    /// <summary>
    /// </summary>
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var jProperty = base.CreateProperty(member, memberSerialization);
        if (jProperty.Writable)
        {
            return jProperty;
        }
        jProperty.Writable = member.IsPropertyWithSetter();
        return jProperty;
    }
}

/// <summary>
/// </summary>
public sealed class PrivateSetterCamelCasePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
{
    /// <summary>
    /// </summary>
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var jProperty = base.CreateProperty(member, memberSerialization);
        if (jProperty.Writable)
        {
            return jProperty;
        }
        jProperty.Writable = member.IsPropertyWithSetter();
        return jProperty;
    }
}

internal static class MemberInfoExtensions
{
    internal static bool IsPropertyWithSetter(this MemberInfo member)
    {
        var property = member as PropertyInfo;
        return property?.GetSetMethod(true) != null;
    }
}
