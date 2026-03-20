using EnsureThat;

namespace OneBeyond.Studio.Infrastructure.SignalR;

internal class SignalRParameters
{
    internal string ConnectionStringKey { get; private init; }
    internal string ErrorChannel { get; private init; }
    internal string HubContextName { get; private init; }

    internal SignalRParameters(
        string connectionStringKey,
        string errorChannel,
        string hubContextName)
    {
        ConnectionStringKey = EnsureArg.IsNotNullOrWhiteSpace(connectionStringKey, nameof(connectionStringKey));
        ErrorChannel = EnsureArg.IsNotNullOrWhiteSpace(errorChannel, nameof(errorChannel));
        HubContextName = EnsureArg.IsNotNullOrWhiteSpace(hubContextName, nameof(hubContextName));
    }

}
