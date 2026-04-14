namespace Markwardt.Network;

public interface IConnectionProcessor<TSend, TReceive> : IConnection<TSend>
{
    IObservable<Packet<TReceive>> Sent { get; }

    void Receive(Packet<TReceive> packet);
}

public static class ConnectionProcessorExtensions
{
    public static IConnectionProcessor<TSend, TReceive> Chain<TSend, TTransport, TReceive>(this IConnectionProcessor<TSend, TTransport> processor, IConnectionProcessor<TTransport, TReceive> chain)
        => new ChainProcessor<TSend, TTransport, TReceive>(processor, chain);
}

public abstract class ConnectionProcessor<TSend, TReceive> : ConnectionTarget<TSend>, IConnectionProcessor<TSend, TReceive>
{
    private readonly BufferSubject<Packet<TReceive>> sent = new();
    public IObservable<Packet<TReceive>> Sent => sent;

    public void Receive(Packet<TReceive> packet)
    {
        if (packet.IsContent)
        {
            ReceiveContent(packet);
        }
        else
        {
            ReceiveSignal(packet);
        }
    }

    protected abstract void ReceiveContent(Packet<TReceive> packet);

    protected virtual void ReceiveSignal(Packet<TReceive> packet)
        => TriggerReceived(packet);

    protected override void SendSignal(Packet<TSend> packet)
        => TriggerSent(packet);

    protected void TriggerSent(Packet packet)
        => sent.OnNext(packet.As<TReceive>());

    protected void TriggerDisconnect(Exception? exception = null)
        => TriggerSent(Packet.NewSignal<TReceive>(new DisconnectedSignal(exception)));

    protected override void OnDispose()
    {
        base.OnDispose();

        sent.Dispose();
    }
}

public class ConnectionProcessor<T> : ConnectionProcessor<T, T>
{
    protected override void SendContent(Packet<T> packet)
        => TriggerSent(packet);

    protected override void SendSignal(Packet<T> packet)
        => TriggerSent(packet);

    protected override void ReceiveContent(Packet<T> packet)
        => TriggerReceived(packet);

    protected override void ReceiveSignal(Packet<T> packet)
        => TriggerReceived(packet);
}