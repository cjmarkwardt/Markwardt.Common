namespace Markwardt;

public interface INetworkRemoteConnection : INetworkConnection
{
    INetworkLink Link { get; }
    INetworkEncryptor? Encryptor { get; }
}

public class NetworkRemoteConnection : NetworkConnection, INetworkRemoteConnection, INetworkLink.IHandler, INetworkFormatHandler
{
    private static NetworkRemoteConnection CreateOutgoing(INetworkConnectionProvider provider, object? profile, INetworkConnector connector)
        => new(provider, null, profile, true, connector.CreateLink());

    public static async ValueTask<INetworkConnection> Connect(INetworkConnectionProvider provider, INetworkConnector connector, object? request, object? profile, bool isSecure, CancellationToken cancellation = default)
        => await CreateOutgoing(provider, profile, connector).RequestConnect(isSecure ? new NetworkSecureMessage() : null, null, request, cancellation);

    public static async ValueTask<INetworkConnection> Register(INetworkConnectionProvider provider, INetworkConnector connector, string identifier, string secret, object? details, object? request, object? profile, CancellationToken cancellation = default)
        => await CreateOutgoing(provider, profile, connector).RequestConnect(new NetworkRegisterMessage(identifier, provider.Authenticator.CreateVerifier(identifier, secret), details), (identifier, secret), request, cancellation);

    public static async ValueTask<INetworkConnection> Authenticate(INetworkConnectionProvider provider, INetworkConnector connector, string identifier, string secret, object? request, object? profile, CancellationToken cancellation = default)
        => await CreateOutgoing(provider, profile, connector).RequestConnect(new NetworkAuthenticateMessage(identifier), (identifier, secret), request, cancellation);

    public static async ValueTask Receive(INetworkConnectionProvider provider, INetworkHost host, INetworkLink link, CancellationToken cancellation = default)
    {
        NetworkRemoteConnection connection = new(provider, host, null, false, link);

        await TimeSpan.FromSeconds(30).Delay(cancellation);

        if (!connection.receiveSuccess)
        {
            await connection.Disconnect("Failed to setup connection within timeout period.", cancellation);
        }
    }

    private NetworkRemoteConnection(INetworkConnectionProvider provider, INetworkHost? host, object? profile, bool isOutgoing, INetworkLink link)
        : base(provider, host, profile, false, isOutgoing)
    {
        this.provider = provider;
        requestTracker = new(1) { ReuseDelay = TimeSpan.FromMinutes(1) };
        
        Link = link;
        link.Handler = this;
        BackgroundTaskk.Start(async cancellation => await link.Run(cancellation)).DisposeWith(this);
    }

    private readonly INetworkConnectionProvider provider;
    private readonly NetworkTracker<TaskCompletionSource<object>> requestTracker;

    private bool receiveSuccess;
    private TaskCompletionSource<NetworkControlMessage>? controlHandler;

    public INetworkLink Link { get; }

    public INetworkEncryptor? Encryptor { get; private set; }

    private readonly Dictionary<int, ReceivedChannel> receivedChannels = [];
    public override IEnumerable<object> ReceivedChannels => receivedChannels.Values.Select(x => x.Profile);

    public override bool IsSecure => Encryptor is not null;

    public override TimeSpan RequestIdReuseDelay { get => requestTracker.ReuseDelay; set => requestTracker.ReuseDelay = value; }

    async void INetworkLink.IHandler.OnReceived(ReadOnlyMemory<byte> data)
    {
        try
        {
            await Provider.Receiver.Receive(this, Encryptor, data);
        }
        catch (NetworkException exception)
        {
            await Disconnect(exception.Message);
        }
    }

    void INetworkLink.IHandler.OnDropped(Exception exception)
        => Close(null, exception);

    ValueTask INetworkFormatHandler.OnMessage(object message)
    {
        provider.Handler.OnReceived(this, null, message);
        return ValueTask.CompletedTask;
    }

    async ValueTask INetworkFormatHandler.OnRequest(int requestId, object message)
    {
        (object response, NetworkSecurity? security, INetworkBlockPool? pool) = await Provider.Handler.OnRequested(this, message, Disposal);
        Provider.Sender.SendResponse(this, pool, security, requestId, response);
    }

    ValueTask INetworkFormatHandler.OnResponse(int requestId, object message)
    {
        TaskCompletionSource<object>? request = requestTracker.Find(requestId);
        requestTracker.Remove(requestId);
        request?.SetResult(message);
        return ValueTask.CompletedTask;
    }

    ValueTask INetworkFormatHandler.OnUpdate(int channelId, byte sequence, object message)
    {
        if (receivedChannels.TryGetValue(channelId, out ReceivedChannel? receivedChannel))
        {
            Provider.Sender.SendSync(this.Yield(), receivedChannel.Pool, receivedChannel.Security, channelId, sequence);
            Provider.Handler.OnReceived(this, receivedChannel.Profile, message);
        }

        return ValueTask.CompletedTask;
    }

    ValueTask INetworkFormatHandler.OnSync(int channelId, byte sequence)
    {
        INetworkTracker<INetworkRemoteChannel> tracker = channelId >= 0 ? ChannelTracker : Provider.ChannelTracker;
        tracker.Find(channelId)?.RemoteSync(this, sequence);
        return ValueTask.CompletedTask;
    }

    async ValueTask INetworkFormatHandler.OnControl(NetworkControlMessage message)
    {
        if (message is NetworkDisconnectMessage disconnectMessage)
        {
            Close(disconnectMessage.Reason);
        }
        else if (message is NetworkConnectMessage connectMessage)
        {
            (object? response, Profile) = await Provider.Handler.OnConnection(Host, User, connectMessage.Request, false, IsSecure, Disposal);
            receiveSuccess = true;
            IsConnected = true;
            Provider.Sender.SendControl(this.Yield(), null, NetworkSecurity.TrySecure, new NetworkCompleteConnectMessage(response));
            Provider.Handler.OnConnected(this, connectMessage.Request);
        }
        else if (message is NetworkSecureMessage)
        {
            await RespondSession(null, Disposal);
        }
        else if (message is NetworkRegisterMessage registerMessage)
        {
            object? userProfile = await Provider.Handler.OnRegistration(Host, registerMessage.Identifier, registerMessage.Verifier, registerMessage.Details, false, Disposal);
            await RespondSession((registerMessage.Identifier, registerMessage.Verifier), Disposal);
            User = new NetworkUser(registerMessage.Identifier, userProfile);
        }
        else if (message is NetworkAuthenticateMessage authenticateMessage)
        {
            (object? userProfile, ReadOnlyMemory<byte> verifier) = await Provider.Handler.OnAuthentication(Host, authenticateMessage.Identifier, false, Disposal);
            await RespondSession((authenticateMessage.Identifier, verifier), Disposal);
            User = new NetworkUser(authenticateMessage.Identifier, userProfile);
        }
        else if (message is NetworkOpenChannelMessage openChannelMessage)
        {
            (object channelProfile, NetworkSecurity? security, INetworkBlockPool? pool) = Provider.Handler.OnChannelOpened(this, openChannelMessage.Message);
            receivedChannels.Add(openChannelMessage.ChannelId, new(channelProfile, security, pool));
        }
        else if (message is NetworkCloseChannelMessage closeChannelMessage)
        {
            if (receivedChannels.Remove(closeChannelMessage.ChannelId, out ReceivedChannel? closedChannel))
            {
                Provider.Handler.OnChannelClosed(this, closedChannel.Profile);
            }
        }
        else
        {
            controlHandler?.SetResult(message);
        }
    }

    protected override void ExecuteSend(object message, NetworkSecurity? security, NetworkReliability reliability, INetworkBlockPool? pool)
        => provider.Sender.SendMessage(this.Yield(), pool, security, reliability, message);

    protected override async ValueTask<object> ExecuteRequest(object message, NetworkSecurity? security, INetworkBlockPool? pool, CancellationToken cancellation = default)
    {
        TaskCompletionSource<object> completion = new();
        int requestId = requestTracker.Add(completion);

        try
        {
            provider.Sender.SendRequest(this, pool, security, requestId, message);
            return await completion.Task.WaitAsync(cancellation);
        }
        finally
        {
            requestTracker.Remove(requestId);
        }
    }

    protected override void SendDisconnect(string reason)
        => provider.Sender.SendControl(this.Yield(), null, NetworkSecurity.Insecure, new NetworkDisconnectMessage(reason));

    protected override void OnDispose()
    {
        base.OnDispose();

        Link.Dispose();
    }

    private async ValueTask<TResponse> RequestControl<TResponse>(NetworkSecurity security, NetworkControlMessage message, CancellationToken cancellation = default)
        where TResponse : NetworkControlMessage
    {
        controlHandler = new();
        provider.Sender.SendControl(this.Yield(), null, security, message);
        NetworkControlMessage response = await controlHandler.Task.WaitAsync(cancellation);
        controlHandler = null;
        return (TResponse)response;
    }

    private async ValueTask RequestSession(NetworkControlMessage control, (string Identifier, string Secret)? credentials, CancellationToken cancellation = default)
    {
        ReadOnlyMemory<byte> sessionData = (await RequestControl<NetworkCreateSessionMessage>(NetworkSecurity.Insecure, control, cancellation)).SessionData;
        (Encryptor, byte[] responseData) = credentials is null ? Provider.Authenticator.CreateEncryptor(sessionData.Span) : Provider.Authenticator.CreateEncryptor(sessionData.Span, credentials.Value.Identifier, credentials.Value.Secret);
        provider.Sender.SendControl(this.Yield(), null, NetworkSecurity.Insecure, new NetworkStartSessionMessage(responseData));
    }

    private async ValueTask RespondSession((string Identifier, ReadOnlyMemory<byte> Verifier)? verification, CancellationToken cancellation = default)
    {
        INetworkSession session = verification is null ? Provider.Authenticator.CreateSession() : Provider.Authenticator.CreateSession(verification.Value.Identifier, verification.Value.Verifier.Span);
        NetworkStartSessionMessage sessionMessage = await RequestControl<NetworkStartSessionMessage>(NetworkSecurity.Insecure, new NetworkCreateSessionMessage(session.Data), cancellation);
        Encryptor = session.CreateEncryptor(sessionMessage.SessionResponseData.Span);
    }

    private async ValueTask<NetworkRemoteConnection> RequestConnect(NetworkControlMessage? secureControl, (string Identifier, string Secret)? credentials, object? request, CancellationToken cancellation = default)
    {
        await Link.Connect(cancellation);

        if (secureControl is not null)
        {
            await RequestSession(secureControl, credentials, cancellation);
        }

        object? response = (await RequestControl<NetworkCompleteConnectMessage>(NetworkSecurity.TrySecure, new NetworkConnectMessage(request), cancellation)).Response;
        IsConnected = true;
        provider.Handler.OnConnected(this, response);
        return this;
    }

    private sealed record ReceivedChannel(object Profile, NetworkSecurity? Security, INetworkBlockPool? Pool);
}