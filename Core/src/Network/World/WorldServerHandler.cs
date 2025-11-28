namespace Markwardt;

public interface IWorldServerHandler<TWorld> : IDisposable
{
    void OnStarted(IWorldServer<TWorld> server);
    void OnConnected(IWorldPlayer player);
    void OnReceived(IWorldPlayer player, object? channel, object message);
    ValueTask<(object Response, NetworkSecurity? Security, INetworkBlockPool? Pool)> OnRequested(IWorldPlayer player, object request, CancellationToken cancellation = default);
    (object ChannelProfile, NetworkSecurity? Security, INetworkBlockPool? Pool) OnChannelOpened(IWorldPlayer player, object message);
    void OnChannelClosed(IWorldPlayer player, object? profile);
    void OnDisconnected(IWorldPlayer player, Exception? exception);
    void OnStopped();
}

public abstract class WorldServerHandler<TWorld> : BaseDisposable, IWorldServerHandler<TWorld>
{
    private BackgroundTaskk? process;

    private IWorldServer<TWorld>? server;
    protected IWorldServer<TWorld> Server => server ?? throw new InvalidOperationException("Server not started");

    public void OnStarted(IWorldServer<TWorld> server)
    {
        this.server = server;
        process = BackgroundTaskk.Start(async cancellation => await Run(cancellation));
    }

    public virtual void OnConnected(IWorldPlayer player) { }

    public virtual void OnReceived(IWorldPlayer player, object? channel, object message) { }

    public virtual ValueTask<(object Response, NetworkSecurity? Security, INetworkBlockPool? Pool)> OnRequested(IWorldPlayer player, object request, CancellationToken cancellation = default)
        => throw NetworkException.Unhandled;

    public virtual (object ChannelProfile, NetworkSecurity? Security, INetworkBlockPool? Pool) OnChannelOpened(IWorldPlayer player, object message)
        => (new AnonymousChannel(), null, null);

    public virtual void OnChannelClosed(IWorldPlayer player, object? profile) {}

    public virtual void OnDisconnected(IWorldPlayer player, Exception? exception) { }

    public virtual void OnStopped() { }

    void IWorldServerHandler<TWorld>.OnStopped()
    {
        process?.Dispose();
        process = null;
        OnStopped();
        server = null;
    }

    protected virtual ValueTask Run(CancellationToken cancellation)
        => ValueTask.CompletedTask;

    protected override void OnDispose()
    {
        base.OnDispose();
        process?.Dispose();
    }
}