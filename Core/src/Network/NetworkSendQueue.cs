namespace Markwardt;

public interface INetworkSendQueue : IDisposable
{
    INetworkBlockPool? DefaultPool { get; set; }

    void Enqueue(IEnumerable<INetworkSender> senders, NetworkReliability reliability, INetworkEncryptor? encryptor, INetworkBlockPool? pool, Action<IBuffer<byte>> writeHeader, Action<IBuffer<byte>> writeBody);
    void Release(INetworkSender sender);
}

public class NetworkSendQueue : BaseDisposable, INetworkSendQueue
{
    private readonly Dictionary<INetworkSender, SenderQueue> queues = [];
    private readonly MemoryWriteStream encryptionStream = new();

    private INetworkBlockPool defaultPool = new NetworkBlockPool();
    public INetworkBlockPool? DefaultPool
    {
        get => defaultPool;
        set => defaultPool = value ?? new NetworkBlockPool();
    }

    public void Enqueue(IEnumerable<INetworkSender> senders, NetworkReliability reliability, INetworkEncryptor? encryptor, INetworkBlockPool? pool, Action<IBuffer<byte>> writeHeader, Action<IBuffer<byte>> writeBody)
    {
        INetworkBlock block = (pool ?? defaultPool).Create(reliability, buffer =>
        {
            if (encryptor is not null)
            {
                writeBody(buffer);
                encryptor.Encrypt(buffer.Span, encryptionStream);
                encryptionStream.Position = 0;
                buffer.Reset();
            }

            writeHeader(buffer);

            if (encryptor is not null)
            {
                buffer.Append(encryptionStream);
                encryptionStream.SetLength(0);
            }
            else
            {
                writeBody(buffer);
            }
        });

        foreach (INetworkSender sender in senders)
        {
            if (!queues.TryGetValue(sender, out SenderQueue? queue))
            {
                queue = new(sender);
                queues.Add(sender, queue);
            }

            block.MarkSending();
            queue.Enqueue(block);
        }

        block.MarkComplete();
    }

    public void Release(INetworkSender sender)
    {
        if (queues.Remove(sender, out SenderQueue? queue))
        {
            queue.Dispose();
        }
    }

    private sealed class SenderQueue(INetworkSender sender) : BaseDisposable
    {
        private readonly Queue<INetworkBlock> outgoingBlocks = [];

        private bool isSending;

        public void Enqueue(INetworkBlock block)
        {
            outgoingBlocks.Enqueue(block);

            if (!isSending)
            {
                isSending = true;
                BackgroundTaskk.Start(Send).DisposeWith(this);
            }
        }

        private async Task Send(CancellationToken cancellation = default)
        {
            try
            {
                while (!cancellation.IsCancellationRequested && outgoingBlocks.TryDequeue(out INetworkBlock? block))
                {
                    await sender.Send(block.Data, block.Reliability, cancellation);
                    block.MarkSent();
                }
            }
            finally
            {
                isSending = false;
            }
        }
    }
}