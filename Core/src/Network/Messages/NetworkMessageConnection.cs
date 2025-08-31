namespace Markwardt;

public interface INetworkMessageConnection : INetworkPeer, INetworkChannelManager
{
    void Send(object message, NetworkConstraints constraints = NetworkConstraints.All);
    ValueTask<object> Request(object request, TimeSpan? timeout = null, CancellationToken cancellation = default);
}