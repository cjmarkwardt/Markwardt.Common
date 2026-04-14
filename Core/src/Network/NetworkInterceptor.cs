namespace Markwardt.Network;

/// <summary> Can capture received network packets instead of them being put into the receive queue. </summary>
public interface INetworkInterceptor
{
    void Attach(INetworkInterceptable connection);

    /// <returns> Messages that will replace the intercepted packet, or null if no interception occurred. </returns>
    IEnumerable<Packet>? Intercept(INetworkInterceptable connection, Packet packet);
}

public abstract class NetworkInterceptor : BaseDisposable, INetworkInterceptor
{
    public static IEnumerable<INetworkInterceptor> GetInterceptors<T>(IConnection<T> connection)
        => (connection as INetworkInterceptable)?.Interceptors ?? [];

    private INetworkInterceptable? connection;

    public void Attach(INetworkInterceptable connection)
        => this.connection = connection;

    public IEnumerable<Packet>? Intercept(INetworkInterceptable connection, Packet packet)
        => this.connection == connection ? Intercept(packet) : null;

    protected abstract IEnumerable<Packet>? Intercept(Packet packet);

    protected void Send(Packet packet)
        => connection.NotNull("Interceptor is not attached to a connection").Send(packet);
}