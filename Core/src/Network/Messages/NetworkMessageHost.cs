namespace Markwardt;

public interface INetworkMessageHost : INetworkPeer
{
    IEnumerable<INetworkMessageConnection> Connections { get; }
}