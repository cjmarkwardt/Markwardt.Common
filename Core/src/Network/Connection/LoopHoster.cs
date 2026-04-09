namespace Markwardt.Network;

public interface ILoopHoster<T> : IHoster<T>, IConnector<T>;

public class LoopHoster<T> : ILoopHoster<T>
{
    private QueueHost<T>? host;

    public IHost<T> Host()
    {
        host?.Stop(new RemoteDisconnectException("Loop host has been replaced"));
        return host = new();
    }

    public IConnection<T> Connect()
    {
        if (host is null)
        {
            return new FailedConnection<T>(new RemoteDisconnectException("No loop host is available"));
        }
        else
        {
            (LoopConnection<T> connection, LoopConnection<T> hostConnection) = LoopConnection<T>.Connect();
            host.Enqueue(hostConnection);
            return connection;
        }
    }
}