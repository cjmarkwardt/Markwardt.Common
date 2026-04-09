namespace Markwardt;

public interface IMessageProtocol<TSend, TReceive>
{
    IMessageProcessor<TSend, TReceive> CreateProcessor();
}

public static class MessageProtocolExtensions
{
    public static IMessageHoster<TSend> WithProtocol<TSend, TReceive>(this IMessageHoster<TReceive> hoster, IMessageProtocol<TSend, TReceive> protocol)
        => new MessageHoster<TSend>(() => protocol.Host(hoster.Host()));

    public static IMessageConnector<TSend> WithProtocol<TSend, TReceive>(this IMessageConnector<TReceive> connector, IMessageProtocol<TSend, TReceive> protocol)
        => new MessageConnector<TSend>(() => protocol.Connect(connector.Connect()));

    public static IMessageHost<TSend> Host<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, IMessageHost<TReceive> host)
        => new ProtocolHost<TSend, TReceive>(host, protocol);

    public static IMessageHost<TSend> Host<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, IMessageHoster<TReceive> hoster)
        => hoster.WithProtocol(protocol).Host();

    public static IMessageHost<TSend> HostLoop<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, out IMessageConnector<TSend> connector)
    {
        LoopHoster<TReceive> hoster = new();
        connector = hoster.CastTo<IMessageConnector<TReceive>>().WithProtocol(protocol);
        return protocol.Host(hoster);
    }

    public static IMessageHost<TSend> HostTcp<TSend>(this IMessageProtocol<TSend, ReadOnlyMemory<byte>> protocol, int? port, IPAddress? address = null, MemoryPool<byte>? pool = null)
        => protocol.Host(new TcpHoster(port, address, pool));

    public static IMessageHost<TSend> HostTcp<TSend>(this IMessageProtocol<TSend, ReadOnlyMemory<byte>> protocol, out int port, IPAddress? address = null, MemoryPool<byte>? pool = null)
    {
        IMessageHost<TSend> host = protocol.HostTcp(null, address, pool);
        port = host.Inspect(IpPortKey.Instance).Value;
        return host;
    }

    public static IMessageConnection<TSend> Connect<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, IMessageConnection<TReceive> connection)
        => new ProtocolConnection<TSend, TReceive>(connection, protocol);

    public static IMessageConnection<TSend> Connect<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, IMessageConnector<TReceive> connector)
        => connector.WithProtocol(protocol).Connect();

    public static (IMessageConnection<TSend>, IMessageConnection<TSend>) ConnectLoop<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol)
    {
        (LoopConnection<TReceive> first, LoopConnection<TReceive> second) = LoopConnection<TReceive>.Connect();
        return (protocol.Connect(first), protocol.Connect(second));
    }

    public static IMessageConnection<TSend> ConnectTcp<TSend>(this IMessageProtocol<TSend, ReadOnlyMemory<byte>> protocol, string host, int port, MemoryPool<byte>? pool = null)
        => protocol.Connect(new TcpConnector(host, port, pool));

    public static IMessageProtocol<TSend, TReceive> Chain<TSend, TTransport, TReceive>(this IMessageProtocol<TSend, TTransport> protocol, IMessageProtocol<TTransport, TReceive> chainProtocol)
        => new MessageProtocol<TSend, TReceive>(() => new ChainProcessor<TSend, TTransport, TReceive>(protocol.CreateProcessor(), chainProtocol.CreateProcessor()));

    public static IMessageProtocol<TSend, TReceive> Configure<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, Action<TReceive, Message> configure)
        => protocol.Chain(new ConfigureProtocol<TReceive>(configure));

    public static IMessageProtocol<TSend, TConverted> Convert<TSend, TReceive, TConverted>(this IMessageProtocol<TSend, TReceive> protocol, IConverter<TReceive, TConverted> converter)
        => protocol.Chain(new ConvertProtocol<TReceive, TConverted>(converter));

    public static IMessageProtocol<TSend, TConverted> Convert<TSend, TReceive, TConverted>(this IMessageProtocol<TSend, TReceive> protocol, Func<TReceive, TConverted> convert, Func<TConverted, TReceive> revert)
        => protocol.Chain(new ConvertProtocol<TReceive, TConverted>(convert, revert));

    public static IMessageProtocol<TSend, StandardPacket<TReceive>> AsStandardPackets<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, IValueWindow? sequenceWindow = null, TimeSpan? pollInterval = null, TimeSpan? pollTimeout = null)
        => protocol.Convert(StandardPacket<TReceive>.New, x => x.Content.NotNull()).WithRequests().WithChannels(sequenceWindow).WithPolls(pollInterval, pollTimeout);

    public static IMessageProtocol<TSend, TReceive> WithPolls<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, TimeSpan? pollInterval = null, TimeSpan? pollTimeout = null)
        where TReceive : IPollPacket, IConstructable<TReceive>
        => protocol.Chain(new PollProtocol<TReceive>(pollInterval, pollTimeout));

    public static IMessageProtocol<TSend, TReceive> WithRequests<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol)
        where TReceive : IHeaderPacket<RequestHeader>
        => protocol.Chain(new RequestProtocol<TReceive>());

    public static IMessageProtocol<TSend, TReceive> WithChannels<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, IValueWindow? sequenceWindow = null)
        where TReceive : IHeaderPacket<ChannelHeader>, IConstructable<TReceive>
        => protocol.Chain(new ChannelProtocol<TReceive>(sequenceWindow));

    public static IMessageProtocol<TSend, string> AsJson<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, JsonSerializerOptions? options = null)
        => protocol.Chain(new JsonProtocol<TReceive>(options));

    public static IMessageProtocol<TSend, ReadOnlyMemory<byte>> AsBytes<TSend>(this IMessageProtocol<TSend, string> protocol, Encoding? encoding = null, bool prefixLength = true, MemoryPool<byte>? pool = null)
        => protocol.Chain(new StringBytesProtocol(encoding, prefixLength, pool));

    public static IMessageProtocol<TSend, InterruptPacket> WithInterrupts<TSend>(this IMessageProtocol<TSend, ReadOnlyMemory<byte>> protocol, int packetSize)
        => protocol.Chain(new InterruptProtocol(packetSize));

    public static IMessageProtocol<TSend, ReadOnlyMemory<byte>> WithRateLimit<TSend>(this IMessageProtocol<TSend, ReadOnlyMemory<byte>> protocol, int rate)
        => protocol.Chain(new RateLimitProtocol(rate));

    public static IMessageProtocol<TSend, ReadOnlyMemory<byte>> WithLengthPrefixBuffer<TSend>(this IMessageProtocol<TSend, ReadOnlyMemory<byte>> protocol, MemoryPool<byte>? pool = null)
        => protocol.Chain(new LengthPrefixBufferProtocol(pool));
}

public class MessageProtocol<TSend, TReceive>(Func<IMessageProcessor<TSend, TReceive>> createProcessor) : IMessageProtocol<TSend, TReceive>
{
    public IMessageProcessor<TSend, TReceive> CreateProcessor()
        => createProcessor();
}

public class MessageProtocol<T>() : MessageProtocol<T, T>(() => new MessageProcessor<T>());