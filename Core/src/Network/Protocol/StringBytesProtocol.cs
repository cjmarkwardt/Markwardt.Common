namespace Markwardt.Network;

public class StringBytesProtocol(Encoding? encoding = null, bool prefixLength = true, MemoryPool<byte>? pool = null, ILengthPrefixWriter? prefixWriter = null) : IConnectionProtocol<string, ReadOnlyMemory<byte>>
{
    public IConnectionProcessor<string, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(encoding ?? Encoding.UTF8, prefixLength, pool ?? MemoryPool<byte>.Shared, prefixWriter ?? new LengthPrefixWriter());

    private sealed class Processor(Encoding encoding, bool prefixLength, MemoryPool<byte> pool, ILengthPrefixWriter prefixWriter) : ConnectionProcessor<string, ReadOnlyMemory<byte>>
    {
        protected override void SendContent(Packet<string> packet)
            => TriggerSent(packet.As<ReadOnlyMemory<byte>>().SetContent(prefixWriter.WriteMemory(pool, encoding.GetByteCount(packet.Content), data => encoding.GetBytes(packet.Content, data.Span), prefixLength)));

        protected override void ReceiveContent(Packet<ReadOnlyMemory<byte>> packet)
            => TriggerReceived(packet.As<string>().SetContent(encoding.GetString(prefixWriter.ReadData(packet.Content, prefixLength).Span)));
    }
}