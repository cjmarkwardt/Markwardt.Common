namespace Markwardt;

public class SteamConnector(SteamTarget target, int port = 0) : IMessageConnector<ReadOnlyMemory<byte>>
{
    public IMessageConnection<ReadOnlyMemory<byte>> Connect()
        => new SteamConnection(target, port);
}