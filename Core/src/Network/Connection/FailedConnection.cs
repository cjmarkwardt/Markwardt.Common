namespace Markwardt;

public class FailedConnection<T>(Exception? disconnectException) : IMessageConnection<T>
{
    public ConnectionState State => ConnectionState.Disconnected;
    public Exception? DisconnectException => disconnectException;
    public IEnumerable<object> Marks => [];
    public IEnumerable<IMessageInterceptor> Interceptors => [];

    public IObservable<Message> Received => Observable.Never<Message>().StartWith(Message.New(new DisconnectedSignal(disconnectException)));

    public void Send(Message message) { }

    public void Dispose() { }
}