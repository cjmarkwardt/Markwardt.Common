using System.Buffers;
using ProtoBuf;

namespace Markwardt;

public static class ProtobufProtocolExtensions
{
    public static IMessageProtocol<TSend, ReadOnlyMemory<byte>> AsProtobuf<TSend, TReceive>(this IMessageProtocol<TSend, TReceive> protocol, bool prefixLength = true, MemoryPool<byte>? pool = null, ILengthPrefixWriter? prefixWriter = null)
        => protocol.Chain(new ProtobufProtocol<TReceive>(prefixLength, pool, prefixWriter));
}

public class ProtobufProtocol<T>(bool prefixLength = true, MemoryPool<byte>? pool = null, ILengthPrefixWriter? prefixWriter = null) : IMessageProtocol<T, ReadOnlyMemory<byte>>
{
    public IMessageProcessor<T, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(prefixLength, pool ?? MemoryPool<byte>.Shared, prefixWriter ?? LengthPrefixWriter.Default);

    private sealed class Processor(bool prefixLength, MemoryPool<byte> pool, ILengthPrefixWriter prefixWriter) : MessageProcessor<T, ReadOnlyMemory<byte>>
    {
        protected override void SendContent(Message message, T content)
        {
            MeasureState<T> measure = Serializer.Measure(content);
            Buffer<byte> buffer = prefixWriter.WriteStream(pool, stream => measure.Serialize(stream), (int)measure.Length, prefixLength);

            message.RecycleContent();
            message.Content = buffer.Memory.AsReadOnly();
            message.Recycler = buffer;

            TriggerSent(message);
        }

        protected override void ReceiveContent(Message message, ReadOnlyMemory<byte> content)
        {
            content = prefixWriter.ReadData(content, prefixLength);
            T value = Serializer.Deserialize<T>(content);

            message.RecycleContent();
            message.Content = value;

            TriggerReceived(message);
        }
    }
}