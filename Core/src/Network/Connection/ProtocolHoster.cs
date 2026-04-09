namespace Markwardt.Network;

public class ProtocolHoster<TSend, TReceive>(IHoster<TReceive> hoster, IConnectionProtocol<TSend, TReceive> protocol) : IHoster<TSend>
{
    public IHost<TSend> Host()
        => protocol.Host(hoster.Host());
}