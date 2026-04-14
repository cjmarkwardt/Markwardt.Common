namespace Markwardt.Network;

public interface INetworkInterceptable
{
    IEnumerable<INetworkInterceptor> Interceptors { get; }

    void Send(Packet packet);
}