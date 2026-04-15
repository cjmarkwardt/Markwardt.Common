namespace Markwardt;

public static class LiteNetExtensions
{
    public static IHost<TSend> HostLiteNet<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, int port)
        => protocol.Host(new LiteNetHoster(port));

    public static IConnection<TSend> ConnectLiteNet<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, string host, int port)
        => protocol.Connect(new LiteNetConnector(host, port));

    internal static void Send(this NetPeer peer, Packet packet, ReadOnlyMemory<byte> content)
    {
        peer.Send(content.Span, packet.Reliability switch
        {
            Reliability.Unreliable => DeliveryMethod.Unreliable,
            Reliability.Reliable => DeliveryMethod.ReliableUnordered,
            Reliability.Ordered => DeliveryMethod.ReliableOrdered,
            var x => throw new NotSupportedException(x.ToString())
        });
        
        packet.Recycle();
    }

    internal static Packet<ReadOnlyMemory<byte>> ToMessage(this NetPacketReader reader)
        => Packet.New(reader.RawData.AsMemory(reader.UserDataOffset, reader.UserDataSize).AsReadOnly(), Recycler.New(reader, static x => x.Recycle()));

    internal static async ValueTask Listen(this NetManager network, CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            if (network.IsRunning)
            {
                network.PollEvents();
            }

            await Task.Delay(25, cancellation);
        }
    }
}