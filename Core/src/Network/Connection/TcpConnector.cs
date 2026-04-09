namespace Markwardt.Network;

public class TcpConnector(string host, int port, MemoryPool<byte>? pool = null) : IConnector<ReadOnlyMemory<byte>>
{
    public IConnection<ReadOnlyMemory<byte>> Connect()
        => new TcpConnection(new(), (host, port), pool);
}