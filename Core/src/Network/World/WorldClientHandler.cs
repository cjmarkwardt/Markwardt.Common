namespace Markwardt;

public interface IWorldClientHandler : IDisposable
{
    void OnConnected(IWorldPlayer player);
    void OnReceived(object? channel, object message);
    ValueTask<(object Response, NetworkSecurity? Security, INetworkBlockPool? Pool)> OnRequested(object request, CancellationToken cancellation = default);
    (object ChannelProfile, NetworkSecurity? Security, INetworkBlockPool? Pool) OnChannelOpened(object message);
    void OnChannelClosed(object? profile);
    void OnDisconnected(Exception? exception);
}

public abstract class WorldClientHandler : BaseDisposable, IWorldClientHandler
{
    private BackgroundTaskk? process;

    private IWorldPlayer? player;
    protected IWorldPlayer Player => player ?? throw new InvalidOperationException("Client not connected");

    public void OnConnected(IWorldPlayer player)
    {
        this.player = player;
        process = BackgroundTaskk.Start(async cancellation => await Run(cancellation));
    }

    public virtual void OnReceived(object? channel, object message) { }

    public virtual ValueTask<(object Response, NetworkSecurity? Security, INetworkBlockPool? Pool)> OnRequested(object request, CancellationToken cancellation = default)
        => throw NetworkException.Unhandled;

    public virtual (object ChannelProfile, NetworkSecurity? Security, INetworkBlockPool? Pool) OnChannelOpened(object message)
        => (new AnonymousChannel(), null, null);

    public virtual void OnChannelClosed(object? profile) { }

    public virtual void OnDisconnected(Exception? exception) { }

    void IWorldClientHandler.OnDisconnected(Exception? exception)
    {
        process?.Dispose();
        process = null;
        OnDisconnected(exception);
        player = null;
    }

    protected virtual ValueTask Run(CancellationToken cancellation)
        => ValueTask.CompletedTask;

    protected override void OnDispose()
    {
        base.OnDispose();
        process?.Dispose();
    }
}