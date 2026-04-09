namespace Markwardt.Network;

public class LengthPrefixBufferProtocol(MemoryPool<byte>? pool = null) : IConnectionProtocol<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>>
{
    public IConnectionProcessor<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>> CreateProcessor()
        => new Processor(pool ?? MemoryPool<byte>.Shared);

    private sealed class Processor : ConnectionProcessor<ReadOnlyMemory<byte>>
    {
        public Processor(MemoryPool<byte> pool)
        {
            this.pool = pool;

            nextLengthBuffer = new(this.pool, 4);
            nextLengthBuffer.SetLength(0);
            
            receivedDataBuffer = new(this.pool);
        }

        private readonly MemoryPool<byte>? pool;
        private readonly MemoryBufferStream nextLengthBuffer;
        private readonly MemoryBufferStream receivedDataBuffer;

        private int? nextLength;

        protected override void ReceiveContent(Packet packet, ReadOnlyMemory<byte> content)
        {
            while (content.Length > 0)
            {
                if (nextLength is null)
                {
                    int read = Math.Min(4 - (int)nextLengthBuffer.Length, content.Length);
                    nextLengthBuffer.Write(content.Span[..read]);
                    content = content[read..];

                    if (nextLengthBuffer.Length == 4)
                    {
                        nextLength = BitConverter.ToInt32(nextLengthBuffer.Memory.Span) + 4;
                        receivedDataBuffer.Buffer = pool.NewBuffer(nextLength.Value);
                        receivedDataBuffer.SetLength(0);
                        receivedDataBuffer.Write(nextLengthBuffer.Memory.Span);
                    }
                }

                if (nextLength is not null)
                {
                    int read = Math.Min(nextLength.Value - (int)receivedDataBuffer.Length, content.Length);
                    receivedDataBuffer.Write(content.Span[..read]);
                    content = content[read..];

                    if (receivedDataBuffer.Length == nextLength)
                    {
                        Buffer<byte> buffer = receivedDataBuffer.Buffer;
                        receivedDataBuffer.Buffer = pool.NewBuffer();

                        nextLength = null;
                        nextLengthBuffer.SetLength(0);

                        TriggerReceived(Packet.New(buffer.Memory.AsReadOnly(), buffer));
                    }
                }
            }
        }
    }
}