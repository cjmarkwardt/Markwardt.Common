namespace Markwardt;

public interface INetworkMessageServerHandler
{
    void OnOpened();
    void OnConnected(INetworkMessageConnection connection);
    void OnReceived(INetworkMessageConnection connection, object message, INetworkChannel? channel);
    ValueTask<object> OnRequested(INetworkMessageConnection connection, object message);
    void OnRecycled(object message);
    void OnDisconnected(INetworkMessageConnection connection, Exception? exception);
    void OnClosed(Exception? exception);
}