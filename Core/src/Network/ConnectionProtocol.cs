namespace Markwardt.Network;

public interface IConnectionProtocol<TSend, TReceive>
{
    IConnectionProcessor<TSend, TReceive> CreateProcessor();
}

public static class ConnectionProtocolExtensions
{
    public static IHoster<TSend> WithProtocol<TSend, TReceive>(this IHoster<TReceive> hoster, IConnectionProtocol<TSend, TReceive> protocol)
        => new Hoster<TSend>(() => protocol.Host(hoster.Host()));

    public static IConnector<TSend> WithProtocol<TSend, TReceive>(this IConnector<TReceive> connector, IConnectionProtocol<TSend, TReceive> protocol)
        => new Connector<TSend>(() => protocol.Connect(connector.Connect()));

    public static IHost<TSend> Host<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, IHost<TReceive> host)
        => new ProtocolHost<TSend, TReceive>(host, protocol);

    public static IHost<TSend> Host<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, IHoster<TReceive> hoster)
        => hoster.WithProtocol(protocol).Host();

    public static IHost<TSend> HostLoop<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, out IConnector<TSend> connector)
    {
        LoopHoster<TReceive> hoster = new();
        connector = hoster.CastTo<IConnector<TReceive>>().WithProtocol(protocol);
        return protocol.Host(hoster);
    }

    public static IHost<TSend> HostTcp<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, int? port, IPAddress? address = null, MemoryPool<byte>? pool = null)
        => protocol.Host(new TcpHoster(port, address, pool));

    public static IHost<TSend> HostTcp<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, out int port, IPAddress? address = null, MemoryPool<byte>? pool = null)
    {
        IHost<TSend> host = protocol.HostTcp(null, address, pool);
        port = host.Inspect(IpPortKey.Instance).Value;
        return host;
    }

    public static IConnection<TSend> Connect<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, IConnection<TReceive> connection)
        => new ProtocolConnection<TSend, TReceive>(connection, protocol);

    public static IConnection<TSend> Connect<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, IConnector<TReceive> connector)
        => connector.WithProtocol(protocol).Connect();

    public static (IConnection<TSend>, IConnection<TSend>) ConnectLoop<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol)
    {
        (LoopConnection<TReceive> first, LoopConnection<TReceive> second) = LoopConnection<TReceive>.Connect();
        return (protocol.Connect(first), protocol.Connect(second));
    }

    public static IConnection<TSend> ConnectTcp<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, string host, int port, MemoryPool<byte>? pool = null)
        => protocol.Connect(new TcpConnector(host, port, pool));

    public static IConnectionProtocol<TSend, TReceive> Chain<TSend, TTransport, TReceive>(this IConnectionProtocol<TSend, TTransport> protocol, IConnectionProtocol<TTransport, TReceive> chainProtocol)
        => new ConnectionProtocol<TSend, TReceive>(() => new ChainProcessor<TSend, TTransport, TReceive>(protocol.CreateProcessor(), chainProtocol.CreateProcessor()));

    public static IConnectionProtocol<TSend, TReceive> Configure<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, Action<TReceive, Packet> configure)
        => protocol.Chain(new ConfigureProtocol<TReceive>(configure));

    public static IConnectionProtocol<TSend, TConverted> Convert<TSend, TReceive, TConverted>(this IConnectionProtocol<TSend, TReceive> protocol, IConverter<TReceive, TConverted> converter)
        => protocol.Chain(new ConvertProtocol<TReceive, TConverted>(converter));

    public static IConnectionProtocol<TSend, TConverted> Convert<TSend, TReceive, TConverted>(this IConnectionProtocol<TSend, TReceive> protocol, Func<TReceive, TConverted> convert, Func<TConverted, TReceive> revert)
        => protocol.Chain(new ConvertProtocol<TReceive, TConverted>(convert, revert));

    public static IConnectionProtocol<TSend, StandardMessage<TReceive>> AsStandardMessages<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, IValueWindow? sequenceWindow = null, TimeSpan? pollInterval = null, TimeSpan? pollTimeout = null)
        => protocol.Convert(StandardMessage<TReceive>.New, x => x.Content.NotNull()).WithRequests().WithChannels(sequenceWindow).WithPolls(pollInterval, pollTimeout);

    public static IConnectionProtocol<TSend, TReceive> WithPolls<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, TimeSpan? pollInterval = null, TimeSpan? pollTimeout = null)
        where TReceive : IPollPacket, IConstructable<TReceive>
        => protocol.Chain(new PollProtocol<TReceive>(pollInterval, pollTimeout));

    public static IConnectionProtocol<TSend, TReceive> WithRequests<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol)
        where TReceive : IHeaderPacket<RequestHeader>
        => protocol.Chain(new RequestProtocol<TReceive>());

    public static IConnectionProtocol<TSend, TReceive> WithChannels<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, IValueWindow? sequenceWindow = null)
        where TReceive : IHeaderPacket<ChannelHeader>, IConstructable<TReceive>
        => protocol.Chain(new ChannelProtocol<TReceive>(sequenceWindow));

    public static IConnectionProtocol<TSend, string> AsJson<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, JsonSerializerOptions? options = null)
        => protocol.Chain(new JsonProtocol<TReceive>(options));

    public static IConnectionProtocol<TSend, ReadOnlyMemory<byte>> AsBytes<TSend>(this IConnectionProtocol<TSend, string> protocol, Encoding? encoding = null, bool prefixLength = true, MemoryPool<byte>? pool = null)
        => protocol.Chain(new StringBytesProtocol(encoding, prefixLength, pool));

    public static IConnectionProtocol<TSend, InterruptPacket> WithInterrupts<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, int packetSize)
        => protocol.Chain(new InterruptProtocol(packetSize));

    public static IConnectionProtocol<TSend, ReadOnlyMemory<byte>> WithRateLimit<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, int rate)
        => protocol.Chain(new RateLimitProtocol(rate));

    public static IConnectionProtocol<TSend, ReadOnlyMemory<byte>> WithLengthPrefixBuffer<TSend>(this IConnectionProtocol<TSend, ReadOnlyMemory<byte>> protocol, MemoryPool<byte>? pool = null)
        => protocol.Chain(new LengthPrefixBufferProtocol(pool));
}

public class ConnectionProtocol<TSend, TReceive>(Func<IConnectionProcessor<TSend, TReceive>> createProcessor) : IConnectionProtocol<TSend, TReceive>
{
    public IConnectionProcessor<TSend, TReceive> CreateProcessor()
        => createProcessor();
}

public class ConnectionProtocol<T>() : ConnectionProtocol<T, T>(() => new ConnectionProcessor<T>());