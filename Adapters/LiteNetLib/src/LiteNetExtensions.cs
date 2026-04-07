namespace Markwardt;

public static class LiteNetExtensions
{
    public static IMessageHost<TSend> HostLiteNet<TSend>(this IMessageProtocol<TSend, ReadOnlyMemory<byte>> protocol, int port)
        => protocol.Host(new LiteNetHoster(port));

    public static IMessageConnection<TSend> ConnectLiteNet<TSend>(this IMessageProtocol<TSend, ReadOnlyMemory<byte>> protocol, string host, int port)
        => protocol.Connect(new LiteNetConnector(host, port));

    internal static void Send(this NetPeer peer, Message message, ReadOnlyMemory<byte> content)
    {
        peer.Send(content.Span, message.Reliability switch
        {
            Reliability.Unreliable => DeliveryMethod.Unreliable,
            Reliability.Reliable => DeliveryMethod.ReliableUnordered,
            Reliability.Ordered => DeliveryMethod.ReliableOrdered,
            var x => throw new NotSupportedException(x.ToString())
        });
        
        message.Recycle();
    }

    internal static Message ToMessage(this NetPacketReader reader)
        => Message.New(reader.RawData.AsMemory(reader.UserDataOffset, reader.UserDataSize).AsReadOnly(), Recycler.New(reader, static x => x.Recycle()));

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