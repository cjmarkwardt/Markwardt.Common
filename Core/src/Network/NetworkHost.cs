namespace Markwardt;

public interface INetworkHost : INetworkPeer
{
    IEnumerable<INetworkConnection> Connections { get; }
}