namespace Markwardt.Network;

public class FailedConnection<T>(Exception? disconnectException) : IConnection<T>
{
    public ConnectionState State => ConnectionState.Disconnected;
    public Exception? DisconnectException => disconnectException;
    public IEnumerable<object> Marks => [];
    public IEnumerable<INetworkInterceptor> Interceptors => [];

    public IObservable<Packet<T>> Received => Observable.Never<Packet<T>>().StartWith(Packet.NewSignal<T>(new DisconnectedSignal(disconnectException)));

    public void Send(Packet packet) { }

    public void Dispose() { }
}