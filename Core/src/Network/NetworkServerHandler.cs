namespace Markwardt;

public interface INetworkServerHandler
{
    void OnOpened();
    void OnConnected(INetworkConnection connection);
    void OnReceived(INetworkConnection connection, ReadOnlySpan<byte> data);
    void OnDisconnected(INetworkConnection connection, Exception? exception);
    void OnClosed(Exception? exception);
}