namespace Markwardt;

public interface INetworkConnectionController : INetworkConnection
{
    void Receive(ReadOnlySpan<byte> data);
    void Drop(Exception exception);
}