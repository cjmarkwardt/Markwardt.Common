namespace Markwardt;

internal class SteamConnection : MessageConnection<ReadOnlyMemory<byte>>
{
    private static SteamConnectionHandle Connect(SteamTarget target, int port)
    {
        SteamNetworkingIdentity id = target.Id;
        return new(SteamNetworkingSockets.ConnectP2P(ref id, port, 0, []));
    }

    public SteamConnection(SteamConnectionHandle handle)
    {
        this.handle = handle.DisposeWith(this);
        Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged).DisposeWith(this);

        this.RunInBackground(Run);
    }

    public SteamConnection(SteamTarget target, int port = 0)
        : this(Connect(target, port)) { }

    private readonly SteamConnectionHandle handle;

    protected override void SendContent(Message message, ReadOnlyMemory<byte> content)
    {
        EResult result = handle.Write(content.Span, message.Reliability is Reliability.Unreliable ? 0 : 8);
        if (result is not EResult.k_EResultOK)
        {
            SetDisconnected(new RemoteDisconnectException($"Failed to send ({result})"));
        }
    }

    private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
    {
        if (callback.m_hConn == handle.Value)
        {
            if (callback.m_info.m_eState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
            {
                SetConnected();
            }
            else if (callback.m_info.m_eState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer or ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
            {
                SetDisconnected(new RemoteDisconnectException(callback.m_info.m_eState.ToString()));
            }
        }
    }

    private async ValueTask Run(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            void Receive(ReadOnlySpan<byte> data)
            {
                Buffer<byte> buffer = data.ToBuffer();
                TriggerReceived(Message.New(buffer.Memory.AsReadOnly(), buffer));
            }

            if (!handle.Read(Receive))
            {
                SetDisconnected(new RemoteDisconnectException("Failed to receive messages"));
            }

            await Task.Delay(25, cancellation);
        }
    }
}