namespace Markwardt;

public class LiteNetHoster(int port, string? key = null) : INetworkHoster
{
    public INetworkListener CreateListener()
        => new Listener(port, key);

    private sealed class Listener : NetworkListener, INetEventListener
    {
        public Listener(int port, string? key)
        {
            this.port = port;
            this.key = key;
            network = new(this);
        }

        private readonly int port;
        private readonly string? key;
        private readonly NetManager network;
        private readonly Dictionary<NetPeer, Link> links = [];

        public override async ValueTask Run(CancellationToken cancellation = default)
        {
            network.Start(port);
            await network.Run(cancellation);
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            if (key is null)
            {
                request.Accept();
            }
            else
            {
                request.AcceptIfKey(key);
            }
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            Link link = new(peer);
            links.Add(peer, link);
            Connect(link);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (links.TryGetValue(peer, out Link? link))
            {
                link.Receive(reader.RawData.AsMemory(reader.UserDataOffset, reader.UserDataSize));
            }
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (links.Remove(peer, out Link? link))
            {
                link.Drop(new NetworkException(disconnectInfo.Reason.ToString()));
            }
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }
        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
    }

    private sealed class Link(NetPeer peer) : NetworkLink
    {
        public override ValueTask Run(CancellationToken cancellation = default)
            => ValueTask.CompletedTask;

        public override ValueTask Connect(CancellationToken cancellation = default)
            => ValueTask.CompletedTask;

        public override ValueTask Send(ReadOnlyMemory<byte> data, NetworkReliability mode, CancellationToken cancellation = default)
        {
            peer.Send(data.Span, mode.GetDeliveryMethod());
            return ValueTask.CompletedTask;
        }

        public new void Receive(ReadOnlyMemory<byte> data)
            => base.Receive(data);

        public new void Drop(Exception exception)
            => base.Drop(exception);

        protected override void OnDispose()
        {
            base.OnDispose();

            peer.Disconnect();
        }
    }
}