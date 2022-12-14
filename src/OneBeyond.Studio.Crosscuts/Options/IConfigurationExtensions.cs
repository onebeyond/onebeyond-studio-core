using EnsureThat;
using Microsoft.Extensions.Configuration;
using OneBeyond.Studio.Crosscuts.Exceptions;

namespace OneBeyond.Studio.Crosscuts.Options;

/// <summary>
/// </summary>
public static class IConfigurationExtensions
{
    /// <summary>
    /// Get options object from configuration section
    /// </summary>
    /// <param name="configuration">the configuration to use</param>
    /// <param name="sectionName">the name of the section to be retrieved from configuration</param>
    /// <returns></returns>
    public static TOptions GetOptions<TOptions>(this IConfiguration configuration, string sectionName)
        where TOptions : class
    {
        EnsureArg.IsNotNull(configuration, nameof(configuration));
        EnsureArg.IsNotNullOrWhiteSpace(sectionName, nameof(sectionName));

        var configSection = configuration.GetSection(sectionName)
            ?? throw new OptionsException(
                $"Unable to find section {sectionName}");
        return configSection.Get<TOptions>(
            (binderOptions) =>
                binderOptions.BindNonPublicProperties = true)
            ?? throw new OptionsException(
                $"Unable to bind data from section {sectionName} to an instance of {typeof(TOptions).Name} type");
    }
}
