namespace Markwardt.Network;

/// <summary> Can capture received network packets instead of them being put into the receive queue. </summary>
public interface INetworkInterceptor
{
    void Attach(ISender sender);

    /// <returns> Messages that will replace the intercepted packet, or null if no interception occurred. </returns>
    IEnumerable<Packet>? Intercept(ISender sender, Packet packet);
}

public abstract class NetworkInterceptor : BaseDisposable, INetworkInterceptor
{
    public static IEnumerable<INetworkInterceptor> GetInterceptors(ISender sender)
        => (sender as INetworkInterceptable)?.Interceptors ?? [];

    private ISender? sender;
    protected ISender Sender => sender ?? throw new InvalidOperationException("Interceptor is not attached to a sender");

    public void Attach(ISender sender)
        => this.sender = sender;

    public IEnumerable<Packet>? Intercept(ISender sender, Packet packet)
        => this.sender == sender ? Intercept(packet) : null;

    protected abstract IEnumerable<Packet>? Intercept(Packet packet);
}