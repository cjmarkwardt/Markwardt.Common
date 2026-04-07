namespace Markwardt;

public class ProtocolConnector<TSend, TReceive>(IMessageConnector<TReceive> connector, IMessageProtocol<TSend, TReceive> protocol) : IMessageConnector<TSend>
{
    public IMessageConnection<TSend> Connect()
        => protocol.Connect(connector.Connect());
}