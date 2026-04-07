namespace Markwardt;

public class ProtocolHost<TSend, TReceive> : BaseMessageHost<TSend>
{
    public ProtocolHost(IMessageHost<TReceive> host, IMessageProtocol<TSend, TReceive> protocol)
    {
        ChainInspections(host);
        host.DisposeWith(this);

        host.Connected.Subscribe(x => Enqueue(protocol.Connect(x))).DisposeWith(this);
        host.Stopped.Subscribe(Stop).DisposeWith(this);
    }
}