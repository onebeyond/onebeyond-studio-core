using System;
using System.IO;
using System.Linq;
using System.Reflection;
using EnsureThat;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OneBeyond.Studio.DataAccess.EFCore.Migrations;

/// <summary>
/// Currently only contains RunSqlFromResource to help with
/// running larger Raw SQL queries.
/// </summary>
public static class MigrationBuilderExtensions
{
    /// <summary>
    /// Extension Method to Run Raw SQL from an assembly resource in migrations.
    /// </summary>
    /// <param name="migrationBuilder">The Migration Builder</param>
    /// <param name="resourceFileName">Basic filename of SQL Query containing file</param>
    /// <param name="assembly">Assembly containing SQL File</param>
    public static void RunSqlFromResource(this MigrationBuilder migrationBuilder, Assembly assembly, string resourceFileName)
    {
        EnsureArg.IsNotNull(migrationBuilder, nameof(migrationBuilder));
        EnsureArg.IsNotNull(assembly, nameof(assembly));
        EnsureArg.IsNotNullOrWhiteSpace(resourceFileName, nameof(resourceFileName));

        var resName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Equals(resourceFileName, StringComparison.InvariantCulture));
        if (string.IsNullOrWhiteSpace(resName))
        {
            throw new FileNotFoundException("Migration Builder could not find the SQL resource file.", resourceFileName);
        }

        using (var stream = assembly.GetManifestResourceStream(resName))
        {
            if (stream is null)
            {
                throw new UnauthorizedAccessException($"Migration build could not read the SQL resource file with name {resName}.");
            }

            using (var streamReader = new StreamReader(stream))
            {
                var text = streamReader.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    migrationBuilder.Sql(text);
                }
            }

        }
    }
}
