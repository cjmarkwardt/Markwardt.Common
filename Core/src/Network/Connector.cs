namespace Markwardt.Network;

public interface IConnector<T>
{
    IConnection<T> Connect();
}

public class Connector<T>(Func<IConnection<T>> connect) : IConnector<T>
{
    public IConnection<T> Connect()
        => connect();
}