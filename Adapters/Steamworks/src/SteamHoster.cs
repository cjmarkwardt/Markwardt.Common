namespace Markwardt;

public class SteamHoster(int port = 0) : IMessageHoster<ReadOnlyMemory<byte>>
{
    public IMessageHost<ReadOnlyMemory<byte>> Host()
        => new Server(port);

    private sealed class Server : BaseMessageHost<ReadOnlyMemory<byte>>
    {
        public Server(int port = 0)
        {
            Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged).DisposeWith(this);
            handle = new SteamListenerHandle(SteamNetworkingSockets.CreateListenSocketP2P(port, 0, [])).DisposeWith(this);
        }

        private readonly SteamListenerHandle handle;

        private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
        {
            if (handle is not null && callback.m_info.m_hListenSocket == handle.Value && callback.m_eOldState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None && callback.m_info.m_eState is ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
            {
                SteamConnectionHandle handle = new(callback.m_hConn);
                if (SteamNetworkingSockets.AcceptConnection(handle.Value) is EResult.k_EResultOK)
                {
                    Enqueue(new SteamConnection(handle));
                }
                else
                {
                    handle.Dispose();
                }
            }
        }
    }
}