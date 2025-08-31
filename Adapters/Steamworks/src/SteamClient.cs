namespace Markwardt;

public class SteamClient(SteamTarget target, int port = 0) : NetworkClient
{
    private readonly TaskCompletionSource connect = new();

    private SteamConnection? connection;
    private bool isConnecting;

    protected override async ValueTask Run(CancellationToken cancellation)
    {
        Callback<SteamNetConnectionStatusChangedCallback_t>.Create(callback =>
        {
            if (connection is not null && callback.m_hConn == connection.Handle)
            {
                OnConnectionStatusChanged(callback);
            }
        }).DisposeWith(this);

        connection = new(this);
        await connection.Run(cancellation);
    }

    protected override async ValueTask Link(CancellationToken cancellation)
    {
        await base.Link(cancellation);

        isConnecting = true;
        SteamNetworkingIdentity id = target.Id;
        connection!.Initialize(new(SteamNetworkingSockets.ConnectP2P(ref id, port, 0, [])));
        await connect.Task.WaitAsync(cancellation);
        isConnecting = false;
    }

    protected override ValueTask ExecuteSend(ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation)
    {
        connection?.Send(data.Span, constraints);
        return ValueTask.CompletedTask;
    }

    protected override void Release()
        => connection?.Dispose();

    private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
    {
        if (callback.m_info.m_eState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
        {
            connect.SetResult();
        }
        else if (callback.m_info.m_eState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer || callback.m_info.m_eState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
        {
            NetworkException exception = new(callback.m_info.m_eState.ToString());

            if (isConnecting)
            {
                connect.SetException(exception);
            }
            else
            {
                Drop(exception);
            }
        }
    }
}