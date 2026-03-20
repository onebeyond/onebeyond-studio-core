using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.Infrastructure.SignalR.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a standard SignalR implementation for use with 
    /// Azure SignalR Service in serverless mode. Correctly sets up
    /// ISignalRService for dependency injection. Please ensure it is added to
    /// BOTH the WebAPI and the Worker jobs, otherwise signalr will not function 
    /// correctly.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionStringKey">The Key to be used either in appsettings.json or the environment variables.</param>
    /// <param name="errorChannel">Name of channel to post general errors in the client SPA</param>
    /// <param name="hubContextName">Name of the Hub Context. Recommended to make project-specific</param>
    /// <returns></returns>
    public static IServiceCollection RegisterSignalR(
        this IServiceCollection services, 
        string connectionStringKey = "AzureSignalRConnectionString",
        string errorChannel = "ERROR_CHANNEL",
        string hubContextName = "OBHubContext")
    {
        services.AddSignalR();
        services.AddSingleton(new SignalRParameters(connectionStringKey, errorChannel, hubContextName));
        services.AddTransient<ISignalRService, SignalRService>();
        return services;
    }
}
