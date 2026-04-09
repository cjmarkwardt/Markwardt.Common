namespace Markwardt;

public class LiteNetHoster(int port, string? key = null) : IHoster<ReadOnlyMemory<byte>>
{
    public IHost<ReadOnlyMemory<byte>> Host()
        => new Server(port, key);

    private sealed class Server : Host<ReadOnlyMemory<byte>>, INetEventListener
    {
        public Server(int port, string? key = null)
        {
            this.key = key ?? string.Empty;
            network = new NetManager(this);
            network.Start(port);

            this.RunInBackground(async cancellation => await network.Listen(cancellation));
        }

        private readonly string key;
        private readonly NetManager network;
        private readonly Dictionary<NetPeer, IncomingConnection> connections = [];

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
            => request.AcceptIfKey(key);

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            IncomingConnection connection = new(peer);
            connections.Add(peer, connection);
            Enqueue(connection);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (connections.TryGetValue(peer, out IncomingConnection? connection))
            {
                connection.Receive(reader);
            }
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (connections.Remove(peer, out IncomingConnection? connection))
            {
                connection.Disconnect(disconnectInfo);
            }
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError) {}
        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }

        protected override void OnDispose()
        {
            base.OnDispose();

            network.DisconnectAll();
            network.Stop();
        }

        private sealed class IncomingConnection : Connection<ReadOnlyMemory<byte>>
        {
            public IncomingConnection(NetPeer peer)
            {
                this.peer = peer;
                
                SetConnected();
            }

            private readonly NetPeer peer;
            
            public void Receive(NetPacketReader reader)
                => TriggerReceived(reader.ToMessage());

            public void Disconnect(DisconnectInfo info)
                => SetDisconnected(new RemoteDisconnectException(info.Reason.ToString()));

            protected override void SendContent(Packet packet, ReadOnlyMemory<byte> content)
                => peer.Send(packet, content);

            protected override void OnDisconnected(Exception? exception)
            {
                base.OnDisconnected(exception);

                peer.Disconnect();
            }
        }
    }
}