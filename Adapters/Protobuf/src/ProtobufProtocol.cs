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
        protected override void SendContent(Packet packet, T content)
        {
            MeasureState<T> measure = Serializer.Measure(content);
            Buffer<byte> buffer = prefixWriter.WriteStream(pool, stream => measure.Serialize(stream), (int)measure.Length, prefixLength);

            packet.RecycleContent();
            packet.Content = buffer.Memory.AsReadOnly();
            packet.Recycler = buffer;

            TriggerSent(packet);
        }

        protected override void ReceiveContent(Packet packet, ReadOnlyMemory<byte> content)
        {
            content = prefixWriter.ReadData(content, prefixLength);
            T value = Serializer.Deserialize<T>(content);

            packet.RecycleContent();
            packet.Content = value;

            TriggerReceived(packet);
        }
    }
}