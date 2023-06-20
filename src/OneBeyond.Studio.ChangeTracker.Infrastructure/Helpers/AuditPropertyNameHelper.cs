using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using OneBeyond.Studio.ChangeTracker.Domain.Attributes;

namespace OneBeyond.Studio.ChangeTracker.Infrastructure.Helpers;

internal static class AuditPropertyNameHelper
{
    // Replaced property names attribute cache
    private static readonly ConcurrentDictionary<Type, Dictionary<string, ChangeTrackerPropertyNameAttribute>> _cache
        = new();

    /// <summary>
    /// Check whether the property has been renamed through the attribute [AuditPropertyName]
    /// </summary>
    public static string GetCustomPropertyName(string propertyName, Type? entityType)
    {
        if (entityType == null)
        {
            return propertyName;
        }

        var renamedProperties = EnsurePropertiesNameAttrCache(entityType);
        if (renamedProperties != null && renamedProperties.TryGetValue(propertyName, out var value))
        {
            // Property marked with AuditPropertyName attribute
            return value.Name;
        }

        return propertyName;
    }

    private static Dictionary<string, ChangeTrackerPropertyNameAttribute> EnsurePropertiesNameAttrCache(Type type)
    {
        if (!_cache.ContainsKey(type))
        {
            var renamedProps = new Dictionary<string, ChangeTrackerPropertyNameAttribute>();
            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                var renamedAttr = prop.GetCustomAttribute<ChangeTrackerPropertyNameAttribute>(true);
                if (renamedAttr != null)
                {
                    var propertyName = prop.GetCustomAttribute<ColumnAttribute>(true)?.Name ?? prop.Name;
                    renamedProps[propertyName] = renamedAttr;
                }
            }

            _cache[type] = renamedProps;
        }
        return _cache[type];
    }
}
