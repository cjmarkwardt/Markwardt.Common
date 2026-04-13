namespace Markwardt;

public static class ProtobufProtocolExtensions
{
    public static IConnectionProtocol<TSend, ReadOnlyMemory<byte>> AsProtobuf<TSend, TReceive>(this IConnectionProtocol<TSend, TReceive> protocol, bool prefixLength = true, MemoryPool<byte>? pool = null, ILengthPrefixWriter? prefixWriter = null)
        => protocol.Chain(new ProtobufProtocol<TReceive>(prefixLength, pool, prefixWriter));
}

public class ProtobufProtocol<T>(bool prefixLength = true, MemoryPool<byte>? pool = null, ILengthPrefixWriter? prefixWriter = null) : IConnectionProtocol<T, ReadOnlyMemory<byte>>
{
    public IConnectionProcessor<T, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(prefixLength, pool ?? MemoryPool<byte>.Shared, prefixWriter ?? LengthPrefixWriter.Default);

    private sealed class Processor(bool prefixLength, MemoryPool<byte> pool, ILengthPrefixWriter prefixWriter) : ConnectionProcessor<T, ReadOnlyMemory<byte>>
    {
        protected override void SendContent(Packet<T> packet)
        {
            MeasureState<T> measure = Serializer.Measure(packet.Content);
            TriggerSent(packet.As<ReadOnlyMemory<byte>>().SetContent(prefixWriter.WriteStream(pool, stream => measure.Serialize(stream), (int)measure.Length, prefixLength)));
        }

        protected override void ReceiveContent(Packet<ReadOnlyMemory<byte>> packet)
            => TriggerReceived(packet.As<T>().SetContent(Serializer.Deserialize<T>(prefixWriter.ReadData(packet.Content, prefixLength))));
    }
}