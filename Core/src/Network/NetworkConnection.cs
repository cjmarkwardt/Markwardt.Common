namespace Markwardt;

public interface INetworkConnection : INetworkPeer
{
    void Send(Action<IBuffer<byte>> write, NetworkConstraints constraints = NetworkConstraints.All);
}