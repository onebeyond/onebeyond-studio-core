using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using OneBeyond.Studio.Infrastructure.Azure.KeyVault.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.KeyVault.Configurations;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddKeyVault(
        this IConfigurationBuilder configurationBuilder,
        string sectionName)
    {
        EnsureArg.IsNotNull(configurationBuilder, nameof(configurationBuilder));

        var configuration = configurationBuilder.Build();
        var keyVaultOptions = configuration.GetSection(sectionName).Get<KeyVaultOptions>();

        if (keyVaultOptions?.Enabled == true)
        {
            EnsureArg.IsNotNullOrWhiteSpace(keyVaultOptions?.Name, nameof(KeyVaultOptions.Name));

            var url = new Uri($"https://{keyVaultOptions.Name}.vault.azure.net/");
            var client = new SecretClient(url, new DefaultAzureCredential());
            configurationBuilder = configurationBuilder.AddAzureKeyVault(client, new KeyVaultSecretManager());
        }

        return configurationBuilder;
    }
}
