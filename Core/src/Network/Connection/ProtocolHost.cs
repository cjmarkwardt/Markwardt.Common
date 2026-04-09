namespace Markwardt.Network;

public class ProtocolHost<TSend, TReceive> : Host<TSend>
{
    public ProtocolHost(IHost<TReceive> host, IConnectionProtocol<TSend, TReceive> protocol)
    {
        ChainInspections(host);
        host.DisposeWith(this);

        host.Connected.Subscribe(x => Enqueue(protocol.Connect(x))).DisposeWith(this);
        host.Stopped.Subscribe(Stop).DisposeWith(this);
    }
}