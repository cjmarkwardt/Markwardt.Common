namespace Markwardt;

public class LiteNetConnector(string host, int port, string? key = null) : IMessageConnector<ReadOnlyMemory<byte>>
{
    public IMessageConnection<ReadOnlyMemory<byte>> Connect()
        => new Connection(host, port, key);

    private sealed class Connection : MessageConnection<ReadOnlyMemory<byte>>, INetEventListener
    {
        public Connection(string host, int port, string? key = null)
        {
            network = new NetManager(this);
            network.Start();
            peer = network.Connect(host, port, key ?? string.Empty);

            this.RunInBackground(async cancellation => await network.Listen(cancellation));
        }

        private readonly NetManager network;
        private readonly NetPeer peer;

        protected override void SendContent(Message message, ReadOnlyMemory<byte> content)
            => peer.Send(message, content);

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
            => request.Reject();

        void INetEventListener.OnPeerConnected(NetPeer peer)
            => SetConnected();

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
            => SetDisconnected(new RemoteDisconnectException(disconnectInfo.Reason.ToString()));

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
            => TriggerReceived(reader.ToMessage());

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }
        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

        protected override void OnDisconnected(Exception? exception)
        {
            base.OnDisconnected(exception);

            network.DisconnectAll();
            network.Stop();
        }
    }
}