namespace Markwardt.Network;

public class FailedConnection<T>(Exception? disconnectException) : IConnection<T>
{
    public ConnectionState State => ConnectionState.Disconnected;
    public Exception? DisconnectException => disconnectException;
    public IEnumerable<object> Marks => [];
    public IEnumerable<INetworkInterceptor> Interceptors => [];

    public IObservable<Packet> Received => Observable.Never<Packet>().StartWith(Packet.NewSignal<object?>(new DisconnectedSignal(disconnectException)));

    public void Send(Packet packet) { }

    public void Dispose() { }
}