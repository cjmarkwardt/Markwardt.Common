namespace Markwardt;

public class SteamServer(int port = 0) : NetworkServer<SteamConnection>
{
    private SteamListenerHandle? handle;

    protected override ValueTask Run(CancellationToken cancellation)
    {
        Callback<SteamNetConnectionStatusChangedCallback_t>.Create(callback =>
        {
            if (handle is not null && callback.m_info.m_hListenSocket == handle.Value)
            {
                OnConnectionStatusChanged(callback);
            }
        }).DisposeWith(this);

        return ValueTask.CompletedTask;
    }

    protected override async ValueTask Link(CancellationToken cancellation)
    {
        await base.Link(cancellation);
        handle = new(SteamNetworkingSockets.CreateListenSocketP2P(port, 0, []));
    }

    protected override async ValueTask RunConnection(SteamConnection connection, CancellationToken cancellation)
        => await connection.Run(cancellation);

    protected override ValueTask ExecuteSend(SteamConnection connection, ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation)
    {
        connection.Send(data.Span, constraints);
        return ValueTask.CompletedTask;
    }

    protected override void ReleaseConnection(SteamConnection connection)
        => connection.Dispose();

    protected override void Release()
    {
        base.Release();
        handle?.Dispose();
    }

    private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
    {
        if (callback.m_eOldState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None && callback.m_info.m_eState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
        {
            SteamConnectionHandle handle = new(callback.m_hConn);
            if (IsOpen && SteamNetworkingSockets.AcceptConnection(handle.Value) is EResult.k_EResultOK)
            {
                Connect(x => new(x, handle));
            }
            else
            {
                handle.Dispose();
            }
        }
    }
}