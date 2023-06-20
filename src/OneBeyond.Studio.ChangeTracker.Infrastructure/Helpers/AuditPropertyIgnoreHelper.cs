using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using OneBeyond.Studio.ChangeTracker.Domain.Attributes;

namespace OneBeyond.Studio.ChangeTracker.Infrastructure.Helpers;

internal static class AuditPropertyIgnoreHelper
{
    // Ignored properties per entity type attribute cache
    private static readonly ConcurrentDictionary<Type, HashSet<string>> _cache
        = new();

    /// <summary>
    /// Determine if the property should be ignored
    /// </summary>
    public static bool IgnoreProperty(string propertyName, Type? entityType)
    {
        if (entityType == null)
        {
            return false;
        }
        var ignoredProperties = EnsurePropertiesIgnoreAttrCache(entityType);
        if (ignoredProperties != null && ignoredProperties.Contains(propertyName))
        {
            // Property marked with AuditIgnore attribute
            return true;
        }

        return false;
    }

    private static HashSet<string> EnsurePropertiesIgnoreAttrCache(Type type)
    {
        if (!_cache.ContainsKey(type))
        {
            var ignoredProps = new HashSet<string>();
            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                var ignoreAttr = prop.GetCustomAttribute(typeof(ChangeTrackerIgnoreAttribute), true);
                if (ignoreAttr != null)
                {
                    var propertyName = prop.GetCustomAttribute<ColumnAttribute>(true)?.Name ?? prop.Name;
                    ignoredProps.Add(propertyName);
                }
            }

            _cache[type] = ignoredProps;
        }

        return _cache[type];
    }
}
