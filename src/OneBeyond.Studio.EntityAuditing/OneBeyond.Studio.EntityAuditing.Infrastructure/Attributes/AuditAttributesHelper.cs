using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Audit.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.EntityAuditing.Domain.Attributes;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure.Attributes;

/// <summary>
/// NOTE: most of this class is taken from DbContextHelper in Audit.EntityFramework package:
/// https://github.com/thepirat000/Audit.NET/blob/master/src/Audit.EntityFramework/README.md
/// </summary>
internal class AuditAttributesHelper
{
    // Included entities attributes cache (Opt-In mode)
    private static readonly ConcurrentDictionary<Type, bool> _entitiesIncludeAttrCache
        = new ConcurrentDictionary<Type, bool>();

    // Ignored entities attributes cache (Opt-Out mode)
    private static readonly ConcurrentDictionary<Type, bool> _entitiesIgnoreAttrCache
        = new ConcurrentDictionary<Type, bool>();

    // Replaced property names attribute cache
    private static readonly ConcurrentDictionary<Type, Dictionary<string, AuditPropertyNameAttribute>> _propertiesNameAttrCache
        = new ConcurrentDictionary<Type, Dictionary<string, AuditPropertyNameAttribute>>();

    // Ignored properties per entity type attribute cache
    private static readonly ConcurrentDictionary<Type, HashSet<string>> _propertiesIgnoreAttrCache
        = new ConcurrentDictionary<Type, HashSet<string>>();

    // Descriptor properties per entity type attribute cache
    private static readonly ConcurrentDictionary<Type, string> _descriptionPropertiesAttrCache
        = new ConcurrentDictionary<Type, string>();

    // Overriden properties per entity type attribute cache
    private static readonly ConcurrentDictionary<Type, Dictionary<string, AuditPropertyValueAttribute>> _propertiesOverrideAttrCache
        = new ConcurrentDictionary<Type, Dictionary<string, AuditPropertyValueAttribute>>();

    // Overriden properties with func per entity type attribute cache
    private static readonly ConcurrentDictionary<Type, Dictionary<string, AuditPropertyValueProviderAttribute>> _propertiesOverrideFuncAttrCache
        = new ConcurrentDictionary<Type, Dictionary<string, AuditPropertyValueProviderAttribute>>();

    /// <summary>
    /// Determine if the whole entity should be ignored (Opt-Out)
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static bool IgnoreEntity(EventEntry entry)
        => EnsureEntitiesIgnoreAttrCache(entry.Entity.GetType());

    /// <summary>
    /// Determine if the whole entity should be included (Opt-In)
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static bool IncludeEntity(EventEntry entry)
        => EnsureEntitiesIncludeAttrCache(entry.Entity.GetType());

    /// <summary>
    /// Determine if the property should be ignored
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public static bool IgnoreProperty(EventEntry entry, string columnName)
    {
        var entityType = entry.Entity.GetType();
        if (entityType == null)
        {
            return false;
        }
        var ignoredProperties = EnsurePropertiesIgnoreAttrCache(entityType);
        if (ignoredProperties != null && ignoredProperties.Contains(columnName))
        {
            // Property marked with AuditIgnore attribute
            return true;
        }

        return false;
    }

    /// <summary>
    /// Return the value of the property marked with [AuditEntityDescriptor] attribute
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static object GetPropertyDescriptorValue(EventEntry entry)
    {
        var entityType = entry.Entity.GetType();
        if (entityType == null)
        {
            return null;
        }
        var descriptorPropertyName = EnsureDescriptionPropertiesAttrCache(entityType);
        if (descriptorPropertyName != null && entry.ColumnValues.ContainsKey(descriptorPropertyName))
        {
            var columnValue = entry.ColumnValues[descriptorPropertyName];
            return columnValue;
        }

        return null;
    }

    /// <summary>
    /// Check whether the property has been renamed through the attribute [AuditPropertyName]
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="columnName"></param>
    /// <param name="currentName"></param>
    /// <returns></returns>
    public static string GetCustomPropertyName(EventEntry entry, string columnName, string currentName)
    {
        var entityType = entry.Entity.GetType();
        if (entityType == null)
        {
            return currentName;
        }
        var renamedProperties = EnsurePropertiesNameAttrCache(entityType);
        if (renamedProperties != null && renamedProperties.ContainsKey(columnName))
        {
            // Property marked with AuditPropertyName attribute
            return renamedProperties[columnName].Name;
        }

        return currentName;
    }

    /// <summary>
    /// Determine if a property value should be overriden with:
    /// 1) a pre-configured value if using [AuditValueOverride] attribute
    /// 2) a static function result value if using [AuditFuncOverride] attribute
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="columnName"></param>
    /// <param name="currentValue"></param>
    /// <returns></returns>
    public static object GetValueOrOverride(IServiceProvider serviceProvider, EventEntry entry, string columnName, object currentValue)
    {
        if (currentValue == null)
        {
            return currentValue;
        }

        var entityType = entry.Entity.GetType();
        if (entityType == null)
        {
            return currentValue;
        }

        var overrideProperties = EnsurePropertiesOverrideAttrCache(entityType);
        if (overrideProperties != null && overrideProperties.ContainsKey(columnName))
        {
            // Property overriden with AuditValueOverride attribute
            return overrideProperties[columnName].Value;
        }

        var overridePropertiesWithFunc = EnsurePropertiesOverrideFuncAttrCache(entityType);
        if (overridePropertiesWithFunc != null && overridePropertiesWithFunc.ContainsKey(columnName))
        {
            var valueProvider =
                (IAuditPropertyValueProvider)serviceProvider.GetRequiredService(overridePropertiesWithFunc[columnName].Type);
            currentValue = valueProvider.GetValue(entry.Entity, columnName, currentValue);
        }

        return currentValue;
    }

    private static bool EnsureEntitiesIncludeAttrCache(Type type)
    {
        if (!_entitiesIncludeAttrCache.ContainsKey(type))
        {
            if (type.GetTypeInfo().GetCustomAttribute(typeof(AuditTrackAttribute), true) != null)
            {
                _entitiesIncludeAttrCache[type] = true; // Type included by AuditTrackAttribute
            }
            else
            {
                _entitiesIncludeAttrCache[type] = false; // Include the entry
            }
        }
        return _entitiesIncludeAttrCache[type];
    }

    private static bool EnsureEntitiesIgnoreAttrCache(Type type)
    {
        if (!_entitiesIgnoreAttrCache.ContainsKey(type))
        {
            if (type.GetTypeInfo().GetCustomAttribute(typeof(AuditDontTrackAttribute), true) != null)
            {
                _entitiesIgnoreAttrCache[type] = true; // Type ignored by AuditDontTrackAttribute
            }
            else
            {
                _entitiesIgnoreAttrCache[type] = false; // Include the entry
            }
        }
        return _entitiesIgnoreAttrCache[type];
    }

    private static HashSet<string> EnsurePropertiesIgnoreAttrCache(Type type)
    {
        if (!_propertiesIgnoreAttrCache.ContainsKey(type))
        {
            var ignoredProps = new HashSet<string>();
            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                var ignoreAttr = prop.GetCustomAttribute(typeof(AuditDontTrackAttribute), true);
                if (ignoreAttr != null)
                {
                    var columnName = prop.GetCustomAttribute<ColumnAttribute>(true)?.Name ?? prop.Name;
                    ignoredProps.Add(columnName);
                }
            }
            if (ignoredProps.Count > 0)
            {
                _propertiesIgnoreAttrCache[type] = ignoredProps;
            }
            else
            {
                _propertiesIgnoreAttrCache[type] = null;
            }
        }
        return _propertiesIgnoreAttrCache[type];
    }

    private static string EnsureDescriptionPropertiesAttrCache(Type type)
    {
        if (!_descriptionPropertiesAttrCache.ContainsKey(type))
        {
            var attributeFound = false;

            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                var propertyDescriptorAttribute = prop.GetCustomAttribute(typeof(AuditDescriptionAttribute), true);
                if (propertyDescriptorAttribute != null)
                {
                    if (attributeFound)
                    {
                        throw new ArgumentOutOfRangeException($"Cannot specify more than one {nameof(AuditDescriptionAttribute)} per class");
                    }

                    var columnName = prop.GetCustomAttribute<ColumnAttribute>(true)?.Name ?? prop.Name;
                    _descriptionPropertiesAttrCache[type] = columnName;
                    attributeFound = true;
                }
            }

            if (!_descriptionPropertiesAttrCache.ContainsKey(type))
            {
                _descriptionPropertiesAttrCache[type] = null;
            }
        }

        return _descriptionPropertiesAttrCache[type];
    }

    private static Dictionary<string, AuditPropertyNameAttribute> EnsurePropertiesNameAttrCache(Type type)
    {
        if (!_propertiesNameAttrCache.ContainsKey(type))
        {
            var renamedProps = new Dictionary<string, AuditPropertyNameAttribute>();
            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                var renamedAttr = prop.GetCustomAttribute<AuditPropertyNameAttribute>(true);
                if (renamedAttr != null)
                {
                    var columnName = prop.GetCustomAttribute<ColumnAttribute>(true)?.Name ?? prop.Name;
                    renamedProps[columnName] = renamedAttr;
                }
            }
            if (renamedProps.Count > 0)
            {
                _propertiesNameAttrCache[type] = renamedProps;
            }
            else
            {
                _propertiesNameAttrCache[type] = null;
            }
        }
        return _propertiesNameAttrCache[type];
    }

    private static Dictionary<string, AuditPropertyValueAttribute> EnsurePropertiesOverrideAttrCache(Type type)
    {
        if (!_propertiesOverrideAttrCache.ContainsKey(type))
        {
            var overrideProps = new Dictionary<string, AuditPropertyValueAttribute>();
            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                var overrideAttr = prop.GetCustomAttribute<AuditPropertyValueAttribute>(true);
                if (overrideAttr != null)
                {
                    var columnName = prop.GetCustomAttribute<ColumnAttribute>(true)?.Name ?? prop.Name;
                    overrideProps[columnName] = overrideAttr;
                }
            }
            if (overrideProps.Count > 0)
            {
                if (overrideProps.Count > 1)
                {
                    throw new ArgumentOutOfRangeException($"Cannot specify more than one {nameof(AuditPropertyValueAttribute)} per class");
                }
                _propertiesOverrideAttrCache[type] = overrideProps;
            }
            else
            {
                _propertiesOverrideAttrCache[type] = null;
            }
        }
        return _propertiesOverrideAttrCache[type];
    }

    private static Dictionary<string, AuditPropertyValueProviderAttribute> EnsurePropertiesOverrideFuncAttrCache(Type type)
    {
        if (!_propertiesOverrideFuncAttrCache.ContainsKey(type))
        {
            var overrideProps = new Dictionary<string, AuditPropertyValueProviderAttribute>();
            foreach (var prop in type.GetTypeInfo().GetProperties())
            {
                var overrideAttr = prop.GetCustomAttribute<AuditPropertyValueProviderAttribute>(true);
                if (overrideAttr != null)
                {
                    overrideProps[prop.Name] = overrideAttr;
                }
            }
            if (overrideProps.Count > 0)
            {
                if (overrideProps.Count > 1)
                {
                    throw new ArgumentOutOfRangeException($"Cannot specify more than one {nameof(AuditPropertyValueProviderAttribute)} per class");
                }
                _propertiesOverrideFuncAttrCache[type] = overrideProps;
            }
            else
            {
                _propertiesOverrideFuncAttrCache[type] = null;
            }
        }
        return _propertiesOverrideFuncAttrCache[type];
    }
}
