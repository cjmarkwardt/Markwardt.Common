namespace Markwardt.Network;

public class QueueHost<T> : Host<T>
{
    public new void Enqueue(IConnection<T> connection)
        => base.Enqueue(connection);

    public new void Stop(Exception? exception = null)
        => base.Stop(exception);
}