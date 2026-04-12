namespace Markwardt.Network;

public class ChainProcessor<TSend, TTransport, TReceive> : ConnectionProcessor<TSend, TReceive>
{
    public ChainProcessor(IConnectionProcessor<TSend, TTransport> source, IConnectionProcessor<TTransport, TReceive> chain)
    {
        this.source = source.DisposeWith(this);
        this.chain = chain.DisposeWith(this);

        ChainInspections(source);
        ChainInspections(chain);

        source.Sent.Subscribe(chain.Send);
        chain.Received.Select(x => x.Inner).Subscribe(source.Receive);

        source.Received.Select(x => x.Inner).Subscribe(TriggerReceived);
        chain.Sent.Subscribe(TriggerSent);
    }

    private readonly IConnectionProcessor<TSend, TTransport> source;
    private readonly IConnectionProcessor<TTransport, TReceive> chain;

    protected override IEnumerable<INetworkInterceptor> Interceptors => base.Interceptors.Concat(NetworkInterceptor.GetInterceptors(source)).Concat(NetworkInterceptor.GetInterceptors(chain));

    protected override void SendContent(Packet packet, TSend content)
        => source.Send(packet);

    protected override void SendSignal(Packet packet)
        => source.Send(packet);

    protected override void ReceiveContent(Packet packet, TReceive content)
        => chain.Receive(packet);

    protected override void ReceiveSignal(Packet packet)
        => chain.Receive(packet);
}