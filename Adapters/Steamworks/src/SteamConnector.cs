namespace Markwardt;

public class SteamConnector(SteamTarget target, int port = 0) : IConnector<ReadOnlyMemory<byte>>
{
    public IConnection<ReadOnlyMemory<byte>> Connect()
        => new SteamConnection(target, port);
}