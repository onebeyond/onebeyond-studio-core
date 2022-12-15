using System;
using Ardalis.SmartEnum;
using Microsoft.EntityFrameworkCore.Storage;

namespace OneBeyond.Studio.DataAccess.EFCore.RelationalTypeMappings;

internal sealed class SmartEnumTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        if (mappingInfo.ClrType.IsSmartEnum(out var genericArguments)
            && SmartEnumRelationalTypeMapping.IsValueTypeSupported(genericArguments[1]))
        {
            var mappingType = typeof(SmartEnumRelationalTypeMapping<,>)
                .MakeGenericType(genericArguments);
            var mapping = (RelationalTypeMapping?)Activator.CreateInstance(mappingType, true);
            return mapping;
        }
        return null;
    }
}
