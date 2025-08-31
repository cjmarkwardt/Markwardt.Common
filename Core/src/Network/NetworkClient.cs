namespace Markwardt;

public interface INetworkSenderHandler
{
    ValueTask Send(ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation);
}

public class NetworkSender : BaseDisposable
{
    public NetworkSender(INetworkSenderHandler handler)
    {
        this.handler = handler;

        BackgroundTaskk.Start(StartSending).DisposeWith(this);
    }

    private readonly INetworkSenderHandler handler;
    private readonly Queue<Block> freeBlocks = [];
    private readonly Queue<Block> outgoingBlocks = [];

    public void Enqueue(Action<IBuffer<byte>> write, NetworkConstraints constraints)
    {
        if (!freeBlocks.TryDequeue(out Block? block))
        {
            block = new();
        }

        block.Constraints = constraints;
        write(block.Data);
        outgoingBlocks.Enqueue(block);
    }

    private async Task StartSending(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            while (outgoingBlocks.TryDequeue(out Block? block))
            {
                await handler.Send(block.Data.Memory, block.Constraints, cancellation);
                block.Data.Reset();
                freeBlocks.Enqueue(block);

                if (cancellation.IsCancellationRequested)
                {
                    return;
                }
            }

            await Task.Delay(25, cancellation);
        }
    }

    private sealed class Block
    {
        public IBuffer<byte> Data { get; } = new Buffer<byte>();

        public NetworkConstraints Constraints { get; set; }
    }
}

public interface INetworkClient : INetworkPort, INetworkConnection
{
    INetworkClientHandler? Handler { get; set; }
}

public abstract class BaseNetworkClient : NetworkPort, INetworkSenderHandler, INetworkConnectionController
{
    public BaseNetworkClient()
        => sender = new NetworkSender(this).DisposeWith(this);

    private readonly NetworkSender sender;

    public void Send(Action<IBuffer<byte>> write, NetworkConstraints constraints = NetworkConstraints.All)
        => sender.Enqueue(write, constraints);

    void INetworkConnectionController.Receive(ReadOnlySpan<byte> data)
        => ExecuteReceive(data);

    void INetworkConnectionController.Drop(Exception exception)
        => Drop(exception);

    protected abstract ValueTask ExecuteSend(ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation);
    protected abstract void ExecuteReceive(ReadOnlySpan<byte> data);

    async ValueTask INetworkSenderHandler.Send(ReadOnlyMemory<byte> data, NetworkConstraints constraints, CancellationToken cancellation)
        => await ExecuteSend(data, constraints, cancellation);
}

public abstract class NetworkClient : BaseNetworkClient, INetworkClient
{
    public INetworkClientHandler? Handler { get; set; }

    protected void Receive(ReadOnlySpan<byte> data)
        => ExecuteReceive(data);

    protected sealed override void ExecuteReceive(ReadOnlySpan<byte> data)
        => Handler?.OnReceived(data);

    protected override void OnOpened()
        => Handler?.OnOpened();

    protected override void OnClosed(Exception? exception)
        => Handler?.OnClosed(exception);
}