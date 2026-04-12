namespace Markwardt.Network;

public interface IConnectionProcessor : IConnection
{
    IObservable<Packet> Sent { get; }

    void Receive(Packet packet);
}

public interface IConnectionProcessor<TSend, TReceive> : IConnectionProcessor, IConnection<TSend>;

public static class ConnectionProcessorExtensions
{
    public static IConnectionProcessor<TSend, TReceive> Chain<TSend, TTransport, TReceive>(this IConnectionProcessor<TSend, TTransport> processor, IConnectionProcessor<TTransport, TReceive> chain)
        => new ChainProcessor<TSend, TTransport, TReceive>(processor, chain);
}

public abstract class ConnectionProcessor<TSend, TReceive> : ConnectionTarget<TSend>, IConnectionProcessor<TSend, TReceive>
{
    private readonly BufferSubject<Packet> sent = new();
    public IObservable<Packet> Sent => sent;

    public void Receive(Packet packet)
    {
        if (packet.Value is TReceive content)
        {
            ReceiveContent(packet, content);
        }
        else
        {
            ReceiveSignal(packet);
        }
    }

    protected abstract void ReceiveContent(Packet packet, TReceive content);

    protected virtual void ReceiveSignal(Packet packet)
        => TriggerReceived(packet);

    protected override void SendSignal(Packet packet)
        => TriggerSent(packet);

    protected void TriggerSent(Packet packet)
        => sent.OnNext(packet);

    protected void TriggerDisconnect(Exception? exception = null)
        => TriggerSent(Packet.New(new DisconnectedSignal(exception)));

    protected override void OnDispose()
    {
        base.OnDispose();

        sent.Dispose();
    }
}

public class ConnectionProcessor<T> : ConnectionProcessor<T, T>
{
    protected override void SendContent(Packet packet, T content)
        => TriggerSent(packet);

    protected override void SendSignal(Packet packet)
        => TriggerSent(packet);

    protected override void ReceiveContent(Packet packet, T content)
        => TriggerReceived(packet);

    protected override void ReceiveSignal(Packet packet)
        => TriggerReceived(packet);
}