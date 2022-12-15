using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.FileStorage.Azure.DependencyInjection;
using OneBeyond.Studio.FileStorage.Azure.Options;
using OneBeyond.Studio.FileStorage.Domain;
using OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

namespace OneBeyond.Studio.FileStorage.ConsoleApp;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        await TestFileStorageAsync(configuration).ConfigureAwait(false);
        await TestCloudStorageAsync(configuration).ConfigureAwait(false);
    }

    private static async Task TestFileStorageAsync(IConfigurationRoot configuration)
    {
        var serviceProvider = ConfigureFileServices(configuration);

        var fileStorage = serviceProvider.GetRequiredService<IFileStorage>();

        var file1Rec = await fileStorage.UploadFileAsync("Hello1.txt", Encoding.Default.GetBytes("First"), "text/plain", default);

        var file2Rec = await fileStorage.UploadFileAsync("Hello2.txt", Encoding.Default.GetBytes("Second"), "text/plain", default);

        var file3Rec = await fileStorage.CopyFileAsync(file2Rec, "Hello2(copy).txt", default);

        using (var zipContentStream = await fileStorage.DownloadFileContentsAsZipAsync(new[] { file1Rec, file2Rec, file3Rec }, default))
        {
            using (var zipFileStream = new FileStream(@"d:\Temp\Hello.zip", FileMode.Create))
            {
                await zipContentStream.CopyToAsync(zipFileStream);
            }
        }

        Console.WriteLine(await fileStorage.GetFileUrlAsync(file1Rec.Id, default));

        Console.WriteLine(await fileStorage.GetFileUrlAsync(file2Rec.Id, default));

        Console.WriteLine(await fileStorage.GetFileUrlAsync(file3Rec.Id, default));

        await fileStorage.DeleteFileAsync(file1Rec.Id, default);
        await fileStorage.DeleteFileAsync(file2Rec.Id, default);
        await fileStorage.DeleteFileAsync(file3Rec.Id, default);
    }

    private static async Task TestCloudStorageAsync(IConfigurationRoot configuration)
    {
        var cloudServiceProvider = ConfigureCloudServices(configuration);

        var cloudStorage = cloudServiceProvider.GetRequiredService<ICloudFileStorage>();

        Console.WriteLine(await cloudStorage.GetUploadUrlAsync("anyFile", default));

        Console.WriteLine(await cloudStorage.GetDownloadUrlAsync("anyFile", default));

        Console.WriteLine(await cloudStorage.GetDeleteUrlAsync("anyFile", default));
    }

    private static IServiceProvider ConfigureFileServices(IConfiguration configuration)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddFileStorage(
            (fileManagerBuilder) =>
                fileManagerBuilder.UseAzureBlobs(configuration.GetOptions<AzureBlobFileStorageOptions>("FileStorage:AzureStorageBlobs")));

        return serviceCollection.BuildServiceProvider();
    }

    private static IServiceProvider ConfigureCloudServices(IConfiguration configuration)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCloudStorage((cloudStorageBuilder) =>
            cloudStorageBuilder.UseAzureBlobs(configuration.GetOptions<AzureBlobCloudStorageOptions>("FileStorage:AzureStorageBlobs")
        ));

        return serviceCollection.BuildServiceProvider();
    }

    private static TOptions GetOptions<TOptions>(this IConfiguration configuration, string sectionKey)
        => configuration
            .GetSection(sectionKey)
            .Get<TOptions>();
}
