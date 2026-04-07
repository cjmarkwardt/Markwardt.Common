namespace Markwardt;

public interface ILoopHoster<T> : IMessageHoster<T>, IMessageConnector<T>;

public class LoopHoster<T> : ILoopHoster<T>
{
    private MessageHost<T>? host;

    public IMessageHost<T> Host()
    {
        host?.Stop(new RemoteDisconnectException("Loop host has been replaced"));
        return host = new();
    }

    public IMessageConnection<T> Connect()
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