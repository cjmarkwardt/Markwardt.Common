namespace Markwardt;

public interface INetworkHostProvider : INetworkConnectionProvider
{
    IEnumerable<INetworkConnection> Connections { get; }
    INetworkTracker<INetworkHost> HostTracker { get; }
}