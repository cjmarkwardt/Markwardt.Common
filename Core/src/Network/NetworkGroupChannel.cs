namespace Markwardt;

public interface INetworkGroupChannel : INetworkChannel
{
    void Open(INetworkConnection connection, object? message = null);
    void Close(INetworkConnection connection);
    void CloseAll();
}