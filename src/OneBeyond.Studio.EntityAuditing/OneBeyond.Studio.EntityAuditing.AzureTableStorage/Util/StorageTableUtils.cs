using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.EntityAuditing.AzureTableStorage.Util;

public static class StorageTableUtils
{
    private static readonly Regex TableNameRegex = new("[^a-zA-Z0-9]", RegexOptions.Compiled);
    private static readonly Regex TableNameValidationRegex = new("^[A-Za-z][A-Za-z0-9]{2,62}$", RegexOptions.Compiled);

    public static string BuildPartitionKey(object value)
        => int.TryParse(value.ToString(), out var valueAsInt) ? $"{valueAsInt:D10}" : value.ToString();

    public static string SanitiseTableName(string tableName)
        => TableNameRegex.Replace(tableName, "");

    public static bool IsValidTableName(string tableName)
        => TableNameValidationRegex.IsMatch(tableName);

    public static object ToEntityProperty(this JToken value)
    {
        switch (value.Type)
        {
            case JTokenType.Boolean:
                return value.ToObject<bool>();

            case JTokenType.Date:
                return value.ToObject<DateTime>();

            case JTokenType.Float:
                return value.ToObject<float>();

            case JTokenType.Integer:
                try
                {
                    return value.ToObject<int>();
                }
                catch
                {
                    return value.ToObject<long>();
                }

            case JTokenType.Guid:
                return value.ToObject<Guid>();

            case JTokenType.String:
                return value.ToObject<string>();

            default:
                return value.ToString();
        }
    }
}
