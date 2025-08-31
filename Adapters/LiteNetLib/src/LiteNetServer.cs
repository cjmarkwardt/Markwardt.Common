namespace Markwardt;

public class LiteNetServer(int port, string key) : NetworkServer<NetPeer>, INetEventListener
{
    private NetManager? manager;

    protected override async ValueTask Run(CancellationToken cancellation)
    {
        manager = new(this);
        await manager.Run(cancellation);
    }

    protected override async ValueTask Link(CancellationToken cancellation)
    {
        await base.Link(cancellation);
        manager?.Start(port);
    }

    protected override ValueTask ExecuteSend(NetPeer connection, ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation)
    {
        connection.Send(data.Span, LiteNetUtils.GetDeliveryMethod(constraints));
        return ValueTask.CompletedTask;
    }

    protected override void ReleaseConnection(NetPeer connection)
        => connection.Disconnect();

    protected override async ValueTask Unlink(CancellationToken cancellation)
    {
        await base.Unlink(cancellation);
        manager?.DisconnectAll();
    }

    protected override void Release()
    {
        base.Release();
        manager?.Stop();
    }

    void INetEventListener.OnConnectionRequest(ConnectionRequest request)
    {
        if (IsOpen)
        {
            request.AcceptIfKey(key);
        }
        else
        {
            request.Reject();
        }
    }

    void INetEventListener.OnPeerConnected(NetPeer peer)
        => Connect(_ => peer);

    void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        => Receive(peer, reader.RawData.AsSpan(reader.UserDataOffset, reader.UserDataSize));

    void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        => Disconnect(peer, new NetworkException(disconnectInfo.Reason.ToString()));

    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }

    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) { }

    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
}