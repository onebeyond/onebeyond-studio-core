using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OneBeyond.Studio.ChangeTracker.Domain.Attributes;

namespace OneBeyond.Studio.ChangeTracker.Infrastructure.Helpers;

internal static class AuditPropertyValueHelper
{
    // Overriden properties per entity type attribute cache
    private static readonly ConcurrentDictionary<Type, Dictionary<string, ChangeTrackerPropertyValueAttribute>> _valueCache
        = new();


    /// <summary>
    /// Determine if a property value should be overriden with:
    /// 1) a pre-configured value if using [AuditPropertyValue] attribute
    /// 2) a function result value if using [AuditPropertyValueProvider] attribute
    public static object? GetValueOrOverride(
        EntityEntry entry,
        string propertyName,
        Type proprtyType,
        object? propertyValue)
    {
        if (propertyValue == null)
        {
            return null;
        }

        var entityType = entry.Entity.GetType();
        if (entityType == null)
        {
            return propertyValue;
        }

        var overrideProperties = EnsurePropertiesOverrideAttrCache(entityType);
        if (overrideProperties != null && overrideProperties.TryGetValue(propertyName, out var value))
        {
            // Property overriden with AuditValueOverride attribute
            return value.Value;
        }

        return propertyValue;
    }

    private static Dictionary<string, ChangeTrackerPropertyValueAttribute> EnsurePropertiesOverrideAttrCache(Type type)
    {
        if (!_valueCache.ContainsKey(type))
        {
            var overrideProps = new Dictionary<string, ChangeTrackerPropertyValueAttribute>();
            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                var overrideAttr = prop.GetCustomAttribute<ChangeTrackerPropertyValueAttribute>(true);
                if (overrideAttr != null)
                {
                    var propertyName = prop.GetCustomAttribute<ColumnAttribute>(true)?.Name ?? prop.Name;
                    overrideProps[propertyName] = overrideAttr;
                }
            }

            if (overrideProps.Count > 1)
            {
                throw new ArgumentOutOfRangeException($"Cannot specify more than one {nameof(ChangeTrackerPropertyValueAttribute)} per entity");
            }

            _valueCache[type] = overrideProps;
        }

        return _valueCache[type];
    }
}
