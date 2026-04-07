namespace Markwardt;

public class ChainProcessor<TSend, TTransport, TReceive> : MessageProcessor<TSend, TReceive>
{
    public ChainProcessor(IMessageProcessor<TSend, TTransport> source, IMessageProcessor<TTransport, TReceive> chain)
    {
        this.source = source.DisposeWith(this);
        this.chain = chain.DisposeWith(this);

        ChainInspections(source);
        ChainInspections(chain);

        source.Sent.Subscribe(chain.Send);
        chain.Received.Subscribe(source.Receive);

        source.Received.Subscribe(TriggerReceived);
        chain.Sent.Subscribe(TriggerSent);
    }

    private readonly IMessageProcessor<TSend, TTransport> source;
    private readonly IMessageProcessor<TTransport, TReceive> chain;

    protected override IEnumerable<IMessageInterceptor> Interceptors => base.Interceptors.Concat(MessageInterceptor.GetInterceptors(source)).Concat(MessageInterceptor.GetInterceptors(chain));

    protected override void SendContent(Message message, TSend content)
        => source.Send(message);

    protected override void SendSignal(Message message)
        => source.Send(message);

    protected override void ReceiveContent(Message message, TReceive content)
        => chain.Receive(message);

    protected override void ReceiveSignal(Message message)
        => chain.Receive(message);
}