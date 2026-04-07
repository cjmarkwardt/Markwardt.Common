namespace Markwardt;

public class ProtocolHoster<TSend, TReceive>(IMessageHoster<TReceive> hoster, IMessageProtocol<TSend, TReceive> protocol) : IMessageHoster<TSend>
{
    public IMessageHost<TSend> Host()
        => protocol.Host(hoster.Host());
}