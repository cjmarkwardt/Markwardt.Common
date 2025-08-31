namespace Markwardt;

public interface INetworkMessageServerGroupHandler
{
    void OnOpened(INetworkMessageHost host, object? tag);
    void OnConnected(INetworkMessageHost host, INetworkMessageConnection connection);
    void OnReceived(INetworkMessageHost host, INetworkMessageConnection connection, object message, INetworkChannel? channel);
    ValueTask<object> OnRequested(INetworkMessageHost host, INetworkMessageConnection connection, object message);
    void OnRecycled(object message);
    void OnDisconnected(INetworkMessageHost host, INetworkMessageConnection connection, Exception? exception);
    void OnClosed(INetworkMessageHost host, Exception? exception);
}