namespace Markwardt;

public interface IMessageConnector<T>
{
    IMessageConnection<T> Connect();
}

public class MessageConnector<T>(Func<IMessageConnection<T>> connect) : IMessageConnector<T>
{
    public IMessageConnection<T> Connect()
        => connect();
}