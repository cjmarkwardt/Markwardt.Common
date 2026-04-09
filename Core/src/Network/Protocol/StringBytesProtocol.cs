namespace Markwardt.Network;

public class StringBytesProtocol(Encoding? encoding = null, bool prefixLength = true, MemoryPool<byte>? pool = null, ILengthPrefixWriter? prefixWriter = null) : IConnectionProtocol<string, ReadOnlyMemory<byte>>
{
    public IConnectionProcessor<string, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(encoding ?? Encoding.UTF8, prefixLength, pool ?? MemoryPool<byte>.Shared, prefixWriter ?? new LengthPrefixWriter());

    private sealed class Processor(Encoding encoding, bool prefixLength, MemoryPool<byte> pool, ILengthPrefixWriter prefixWriter) : ConnectionProcessor<string, ReadOnlyMemory<byte>>
    {
        protected override void SendContent(Packet packet, string content)
        {
            Buffer<byte> buffer = prefixWriter.WriteMemory(pool, encoding.GetByteCount(content), data => encoding.GetBytes(content, data.Span), prefixLength);
            
            packet.RecycleContent();
            packet.Content = buffer.Memory.AsReadOnly();
            packet.Recycler = buffer;
            
            TriggerSent(packet);
        }

        protected override void ReceiveContent(Packet packet, ReadOnlyMemory<byte> content)
        {
            content = prefixWriter.ReadData(content, prefixLength);

            packet.RecycleContent();
            packet.Content = encoding.GetString(content.Span);

            TriggerReceived(packet);
        }
    }
}