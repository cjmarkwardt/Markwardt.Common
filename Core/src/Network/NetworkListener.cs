namespace Markwardt;

public interface INetworkListener : IDisposable
{
    IHandler? Handler { get; set; }

    ValueTask Run(CancellationToken cancellation = default);

    interface IHandler
    {
        void OnConnected(INetworkLink link);
    }
}

public abstract class NetworkListener : BaseDisposable, INetworkListener
{
    public INetworkListener.IHandler? Handler { get; set; }

    public abstract ValueTask Run(CancellationToken cancellation = default);

    protected void Connect(INetworkLink link)
        => Handler?.OnConnected(link);
}