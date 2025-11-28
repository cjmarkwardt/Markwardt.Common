namespace Markwardt;

public class TcpBlockConnector(string host, int port, int clampedCapacity = 1024) : INetworkConnector
{
    public INetworkLink CreateLink()
        => new TcpBlockLink(new TcpClient(), clampedCapacity, (host, port));
}