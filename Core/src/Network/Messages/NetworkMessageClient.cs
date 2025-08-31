namespace Markwardt;

public interface INetworkMessageClient : INetworkPort, INetworkMessageConnection
{
    INetworkMessageClientHandler? Handler { get; set; }
}

public class NetworkMessageClient : BaseDisposable, INetworkMessageClient, INetworkClientHandler, INetworkMessageProcessorHandler
{
    public NetworkMessageClient(INetworkClient client, INetworkMessageSerializer serializer)
    {
        this.client = client;
        client.Handler = this;

        processor = new(client, this, serializer) { Handler = this };
    }

    private readonly INetworkClient client;
    private readonly NetworkMessageProcessor processor;

    public INetworkMessageClientHandler? Handler { get; set; }

    public IReadOnlyDictionary<int, INetworkChannel> Channels => processor.Channels;
    public bool IsOpen => client.IsOpen;
    public Exception? Exception => client.Exception;

    public async ValueTask Open(CancellationToken cancellation = default)
        => await client.Open(cancellation);

    public async ValueTask Close(CancellationToken cancellation = default)
        => await client.Close(cancellation);

    public INetworkChannel CreateChannel(int id)
        => processor.CreateChannel(id);

    public void Send(object message, NetworkConstraints constraints = NetworkConstraints.All)
        => processor.Send(message, constraints);

    public async ValueTask<object> Request(object request, TimeSpan? timeout = null, CancellationToken cancellation = default)
        => await processor.Request(request, timeout, cancellation);

    void INetworkClientHandler.OnReceived(ReadOnlySpan<byte> data)
        => processor.Receive(data);

    void INetworkMessageProcessorHandler.OnReceived(object message, INetworkChannel? channel)
        => Handler?.OnReceived(message, channel);

    async ValueTask<object> INetworkMessageProcessorHandler.OnRequested(object message)
    {
        if (Handler is null)
        {
            throw new NetworkRequestRejectedException("Requests are not handled");
        }

        return await Handler.OnRequested(message);
    }

    void INetworkMessageProcessorHandler.OnRecycled(object message)
        => Handler?.OnRecycled(message);

    void INetworkClientHandler.OnOpened()
        => Handler?.OnOpened();

    void INetworkClientHandler.OnClosed(Exception? exception)
        => Handler?.OnClosed(exception);

    protected override void OnDispose()
    {
        base.OnDispose();
        client.Dispose();
    }
}
