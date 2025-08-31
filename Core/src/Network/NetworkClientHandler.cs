namespace Markwardt;

public interface INetworkClientHandler
{
    void OnOpened();
    void OnReceived(ReadOnlySpan<byte> data);
    void OnClosed(Exception? exception);
}