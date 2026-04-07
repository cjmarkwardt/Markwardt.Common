namespace Markwardt;

public class TcpConnector(string host, int port, MemoryPool<byte>? pool = null) : IMessageConnector<ReadOnlyMemory<byte>>
{
    public IMessageConnection<ReadOnlyMemory<byte>> Connect()
        => new TcpConnection(new(), (host, port), pool);
}