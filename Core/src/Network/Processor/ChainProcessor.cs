namespace Markwardt.Network;

public class ChainProcessor<TSend, TTransport, TReceive> : ConnectionProcessor<TSend, TReceive>
{
    public ChainProcessor(IConnectionProcessor<TSend, TTransport> source, IConnectionProcessor<TTransport, TReceive> chain)
    {
        this.source = source.DisposeWith(this);
        this.chain = chain.DisposeWith(this);

        ChainInspections(source);
        ChainInspections(chain);

        source.Sent.Subscribe(x => chain.Send(x));
        chain.Received.Select(x => x.Inner).Subscribe(source.Receive);

        source.Received.Select(x => x.Inner).Subscribe(TriggerReceived);
        chain.Sent.Subscribe(x => TriggerSent(x));
    }

    private readonly IConnectionProcessor<TSend, TTransport> source;
    private readonly IConnectionProcessor<TTransport, TReceive> chain;

    protected override IEnumerable<INetworkInterceptor> Interceptors => base.Interceptors.Concat(NetworkInterceptor.GetInterceptors(source)).Concat(NetworkInterceptor.GetInterceptors(chain));

    protected override void SendContent(Packet<TSend> packet)
        => source.Send(packet);

    protected override void SendSignal(Packet<TSend> packet)
        => source.Send(packet);

    protected override void ReceiveContent(Packet<TReceive> packet)
        => chain.Receive(packet);

    protected override void ReceiveSignal(Packet<TReceive> packet)
        => chain.Receive(packet);
}