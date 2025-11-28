namespace Markwardt;

public interface INetworkBlockPool
{
    INetworkBlock Create(NetworkReliability mode, Action<IBuffer<byte>> write);
}

public class NetworkBlockPool(Func<INetworkBlock, bool> isPooled) : INetworkBlockPool
{
    private readonly Func<INetworkBlock, bool> isPooled = isPooled;

    public NetworkBlockPool(int maxPooledBlockSize)
        : this(block => block.Data.Length <= maxPooledBlockSize) { }

    public NetworkBlockPool()
        : this(_ => false) { }

    private readonly Queue<Block> free = [];

    public INetworkBlock Create(NetworkReliability reliability, Action<IBuffer<byte>> write)
    {
        if (!free.TryDequeue(out Block? block))
        {
            block = new(this);
        }

        block.Set(reliability, write);
        return block;
    }

    private sealed class Block(NetworkBlockPool pool) : INetworkBlock
    {
        private int pendingSends;
        private bool isComplete;

        private readonly Buffer<byte> data = new();
        public ReadOnlyMemory<byte> Data => data.Memory;

        public NetworkReliability Reliability { get; private set; }

        public void Set(NetworkReliability mode, Action<IBuffer<byte>> write)
        {
            pendingSends = 0;
            isComplete = false;
            Reliability = mode;
            data.Reset();
            write(data);
        }

        public void MarkSending()
            => pendingSends++;

        public void MarkSent()
        {
            pendingSends--;
            TryRecycle();
        }

        public void MarkComplete()
        {
            isComplete = true;
            TryRecycle();
        }

        private void TryRecycle()
        {
            if (isComplete && pendingSends == 0 && pool.isPooled(this))
            {
                pool.free.Enqueue(this);
            }
        }
    }
}