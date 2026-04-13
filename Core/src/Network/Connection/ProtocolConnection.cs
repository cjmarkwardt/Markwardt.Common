namespace Markwardt.Network;

public class ProtocolConnection<TSend, TReceive> : BaseDisposable, IConnection<TSend>, INetworkInterceptable, IInspectable
{
    public ProtocolConnection(IConnection<TReceive> connection, IConnectionProtocol<TSend, TReceive> protocol)
    {
        processor = protocol.CreateProcessor();
        this.connection = connection;

        inspections = new Dictionary<IInspectKey, object>().ChainInspections(processor).ChainInspections(connection);

        processor.Sent.Subscribe(OnProcessorSent);
        connection.Received.Select(x => x.Inner).Subscribe(OnConnectionReceived);
    }

    private readonly IConnectionProcessor<TSend, TReceive> processor;
    private readonly IConnection<TReceive> connection;
    private readonly IDictionary<IInspectKey, object> inspections;

    private Exception? disconnectException;

    public IObservable<Packet> Received => processor.Received.Select(x => x.Inner);

    IEnumerable<INetworkInterceptor> INetworkInterceptable.Interceptors => NetworkInterceptor.GetInterceptors(processor).Concat(NetworkInterceptor.GetInterceptors(connection));

    public ConnectionState State => disconnectException is not null ? ConnectionState.Disconnected : connection.State;
    public Exception? DisconnectException => disconnectException ?? connection.DisconnectException;

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspections;

    public void Send(Packet packet)
        => processor.Send(packet);

    private void OnProcessorSent(Packet packet)
    {
        if (packet.IsSignal && packet.Value is DisconnectedSignal signal)
        {
            disconnectException = signal.Exception;
            processor.Receive(packet);
            connection.Dispose();
        }
        else
        {
            connection.Send(packet);
        }
    }

    private void OnConnectionReceived(Packet packet)
    {
        if (disconnectException is null)
        {
            processor.Receive(packet);
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        connection.Dispose();
        processor.Dispose();
    }
}