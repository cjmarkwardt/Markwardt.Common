namespace Markwardt.Network;

public class ProtocolConnector<TSend, TReceive>(IConnector<TReceive> connector, IConnectionProtocol<TSend, TReceive> protocol) : IConnector<TSend>
{
    public IConnection<TSend> Connect()
        => protocol.Connect(connector.Connect());
}