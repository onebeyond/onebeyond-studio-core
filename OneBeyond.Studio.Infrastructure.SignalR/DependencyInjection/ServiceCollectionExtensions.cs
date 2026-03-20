using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.Infrastructure.SignalR.DependencyInjection;

public static class ServiceCollectionExtensions
{
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
