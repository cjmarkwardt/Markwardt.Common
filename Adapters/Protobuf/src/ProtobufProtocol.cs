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
            Buffer<byte> buffer = prefixWriter.WriteStream(pool, stream => measure.Serialize(stream), (int)measure.Length, prefixLength);

            packet.Inner.Set(buffer.Memory.AsReadOnly(), buffer);
            TriggerSent(packet);
        }

        protected override void ReceiveContent(Packet packet, ReadOnlyMemory<byte> content)
        {
            content = prefixWriter.ReadData(content, prefixLength);
            T value = Serializer.Deserialize<T>(content);

            packet.Set(value);
            TriggerReceived(packet);
        }
    }
}