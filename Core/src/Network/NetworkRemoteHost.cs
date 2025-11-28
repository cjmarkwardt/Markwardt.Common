namespace Markwardt;

public class NetworkRemoteHost : BaseDisposable, INetworkHost, INetworkListener.IHandler
{
    public NetworkRemoteHost(INetworkHostProvider provider, INetworkHoster hoster, object? profile)
    {
        this.provider = provider;
        Id = provider.HostTracker.Add(this);
        Profile = profile;

        listener = hoster.CreateListener();
        listener.Handler = this;
        BackgroundTaskk.Start(async cancellation => await listener.Run(cancellation)).DisposeWith(this);
    }

    private readonly INetworkHostProvider provider;
    private readonly INetworkListener listener;

    public int Id { get; }
    public object? Profile { get; }

    public IEnumerable<INetworkConnection> Connections => provider.Connections.Where(x => x.Host == this);

    protected override void OnDispose()
    {
        base.OnDispose();

        provider.HostTracker.Remove(Id);
    }

    async void INetworkListener.IHandler.OnConnected(INetworkLink link)
        => await NetworkRemoteConnection.Receive(provider, this, link, Disposal);
}