namespace Markwardt;

public class LiteNetConnector(string host, int port, string? key = null) : INetworkConnector
{
    public INetworkLink CreateLink()
        => new Link(host, port, key);

    private sealed class Link : NetworkLink, INetEventListener
    {
        public Link(string host, int port, string? key)
        {
            this.host = host;
            this.port = port;
            this.key = key;
            network = new(this);
        }

        private readonly TaskCompletionSource connect = new();
        private readonly string host;
        private readonly int port;
        private readonly string? key;
        private readonly NetManager network;
        
        private bool isConnecting;

        public override async ValueTask Run(CancellationToken cancellation = default)
        {
            network.Start();
            await network.Run(cancellation);
        }

        public override async ValueTask Connect(CancellationToken cancellation = default)
        {
            isConnecting = true;
            network.Connect(host, port, key ?? string.Empty);
            await connect.Task.WaitAsync(cancellation);
            isConnecting = false;
        }

        public override ValueTask Send(ReadOnlyMemory<byte> data, NetworkReliability mode, CancellationToken cancellation = default)
        {
            network.SendToAll(data.Span, mode.GetDeliveryMethod());
            return ValueTask.CompletedTask;
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
            => request.Reject();

        void INetEventListener.OnPeerConnected(NetPeer peer)
            => connect.SetResult();

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
            => Receive(reader.RawData.AsMemory(reader.UserDataOffset, reader.UserDataSize));

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            NetworkException exception = new(disconnectInfo.Reason.ToString());

            if (isConnecting)
            {
                connect.SetException(exception);
            }
            else
            {
                Drop(exception);
            }
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }
        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }

        protected override void OnDispose()
        {
            base.OnDispose();

            network.DisconnectAll();
            network.Stop();
        }
    }
}