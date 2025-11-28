namespace Markwardt;

public interface INetworkLink : IDisposable, INetworkSender
{
    IHandler? Handler { get; set; }

    ValueTask Run(CancellationToken cancellation = default);
    ValueTask Connect(CancellationToken cancellation = default);

    interface IHandler
    {
        void OnReceived(ReadOnlyMemory<byte> data);
        void OnDropped(Exception exception);
    }
}

public abstract class NetworkLink : BaseDisposable, INetworkLink
{
    public INetworkLink.IHandler? Handler { get; set; }

    public abstract ValueTask Run(CancellationToken cancellation = default);
    public abstract ValueTask Connect(CancellationToken cancellation = default);
    public abstract ValueTask Send(ReadOnlyMemory<byte> data, NetworkReliability mode, CancellationToken cancellation = default);

    protected void Receive(ReadOnlyMemory<byte> data)
        => Handler?.OnReceived(data);

    protected void Drop(Exception exception)
        => Handler?.OnDropped(exception);
}