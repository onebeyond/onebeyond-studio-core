using EnsureThat;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace OneBeyond.Studio.Infrastructure.SignalR;

internal class SignalRService : ISignalRService
{
    private readonly string _connectionString;
    private readonly string _globalErrorChannel;
    private readonly string _hubContextName;

    public SignalRService(
        IConfiguration configuration,
        SignalRParameters parameters)
    {
        EnsureArg.IsNotNull(parameters);
        _connectionString = EnsureArg.IsNotNull(configuration, nameof(configuration)).GetValue<string>(parameters.ConnectionStringKey)!;

        _globalErrorChannel = parameters.ErrorChannel;
        _hubContextName = parameters.HubContextName;
    }

    public async Task PublishAsync<TMessageDto>(SignalRMessageDto<TMessageDto> message, CancellationToken cancellationToken)
    {
        var messageObject = JsonConvert.SerializeObject(message.Message);

        using var hubContext = await GetHubContextAsync(cancellationToken);

        await hubContext.Clients.User(message.UserId).SendAsync(message.NotificationChannelName, messageObject, cancellationToken);
    }

    public async Task PublishErrorMessageAsync(string message, string userId, CancellationToken cancellationToken)
    {
        using var hubContext = await GetHubContextAsync(cancellationToken);

        await hubContext.Clients.User(userId).SendAsync(_globalErrorChannel, message, cancellationToken);
    }

    public async Task<ServiceHubContext> GetHubContextAsync(CancellationToken cancellationToken)
    {
        var serviceManager = new ServiceManagerBuilder()
            .WithOptions(options =>
            {
                options.ConnectionString = _connectionString;
            }).BuildServiceManager();

        return await serviceManager.CreateHubContextAsync(_hubContextName, cancellationToken);        
    }
}
