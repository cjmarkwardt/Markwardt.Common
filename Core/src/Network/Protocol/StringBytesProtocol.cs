namespace Markwardt;

public class StringBytesProtocol(Encoding? encoding = null, bool prefixLength = true, MemoryPool<byte>? pool = null, ILengthPrefixWriter? prefixWriter = null) : IMessageProtocol<string, ReadOnlyMemory<byte>>
{
    public IMessageProcessor<string, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(encoding ?? Encoding.UTF8, prefixLength, pool ?? MemoryPool<byte>.Shared, prefixWriter ?? new LengthPrefixWriter());

    private sealed class Processor(Encoding encoding, bool prefixLength, MemoryPool<byte> pool, ILengthPrefixWriter prefixWriter) : MessageProcessor<string, ReadOnlyMemory<byte>>
    {
        protected override void SendContent(Message message, string content)
        {
            Buffer<byte> buffer = prefixWriter.WriteMemory(pool, encoding.GetByteCount(content), data => encoding.GetBytes(content, data.Span), prefixLength);
            
            message.RecycleContent();
            message.Content = buffer.Memory.AsReadOnly();
            message.Recycler = buffer;
            
            TriggerSent(message);
        }

        protected override void ReceiveContent(Message message, ReadOnlyMemory<byte> content)
        {
            content = prefixWriter.ReadData(content, prefixLength);

            message.RecycleContent();
            message.Content = encoding.GetString(content.Span);

            TriggerReceived(message);
        }
    }
}