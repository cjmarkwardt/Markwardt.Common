namespace Markwardt;

public class ProtocolConnection<TSend, TReceive> : BaseDisposable, IMessageConnection<TSend>, IMessageInterceptable, IInspectable
{
    public ProtocolConnection(IMessageConnection<TReceive> connection, IMessageProtocol<TSend, TReceive> protocol)
    {
        processor = protocol.CreateProcessor();
        this.connection = connection;

        inspections = new Dictionary<IInspectKey, object>().ChainInspections(processor).ChainInspections(connection);

        processor.Sent.Subscribe(OnProcessorSent);
        connection.Received.Subscribe(OnConnectionReceived);
    }

    private readonly IMessageProcessor<TSend, TReceive> processor;
    private readonly IMessageConnection<TReceive> connection;
    private readonly IDictionary<IInspectKey, object> inspections;

    private Exception? disconnectException;

    public IObservable<Message> Received => processor.Received;

    IEnumerable<IMessageInterceptor> IMessageInterceptable.Interceptors => MessageInterceptor.GetInterceptors(processor).Concat(MessageInterceptor.GetInterceptors(connection));

    public ConnectionState State => disconnectException is not null ? ConnectionState.Disconnected : connection.State;
    public Exception? DisconnectException => disconnectException ?? connection.DisconnectException;

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspections;

    public void Send(Message message)
        => processor.Send(message);

    private void OnProcessorSent(Message message)
    {
        if (message.Content is DisconnectedSignal signal)
        {
            disconnectException = signal.Exception;
            processor.Receive(message);
            connection.Dispose();
        }
        else
        {
            connection.Send(message);
        }
    }

    private void OnConnectionReceived(Message message)
    {
        if (disconnectException is null)
        {
            processor.Receive(message);
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        connection.Dispose();
        processor.Dispose();
    }
}