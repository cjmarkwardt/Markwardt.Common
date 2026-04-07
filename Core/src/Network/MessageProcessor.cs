namespace Markwardt;

public interface IMessageProcessor : IMessageConnection
{
    IObservable<Message> Sent { get; }

    void Receive(Message message);
}

public interface IMessageProcessor<TSend, TReceive> : IMessageProcessor, IMessageConnection<TSend>;

public static class MessageProcessorExtensions
{
    public static IMessageProcessor<TSend, TReceive> Chain<TSend, TTransport, TReceive>(this IMessageProcessor<TSend, TTransport> processor, IMessageProcessor<TTransport, TReceive> chain)
        => new ChainProcessor<TSend, TTransport, TReceive>(processor, chain);
}

public abstract class MessageProcessor<TSend, TReceive> : MessageTarget<TSend>, IMessageProcessor<TSend, TReceive>
{
    private readonly BufferSubject<Message> sent = new();
    public IObservable<Message> Sent => sent;

    public void Receive(Message message)
    {
        if (message.Content is TReceive content)
        {
            ReceiveContent(message, content);
        }
        else
        {
            ReceiveSignal(message);
        }
    }

    protected abstract void ReceiveContent(Message message, TReceive content);

    protected virtual void ReceiveSignal(Message message)
        => TriggerReceived(message);

    protected override void SendSignal(Message message)
        => TriggerSent(message);

    protected void TriggerSent(Message message)
        => sent.OnNext(message);

    protected void TriggerDisconnect(Exception? exception = null)
        => TriggerSent(Message.New(new DisconnectedSignal(exception)));

    protected override void OnDispose()
    {
        base.OnDispose();

        sent.Dispose();
    }
}

public class MessageProcessor<T> : MessageProcessor<T, T>
{
    protected override void SendContent(Message message, T content)
        => TriggerSent(message);

    protected override void SendSignal(Message message)
        => TriggerSent(message);

    protected override void ReceiveContent(Message message, T content)
        => TriggerReceived(message);

    protected override void ReceiveSignal(Message message)
        => TriggerReceived(message);
}