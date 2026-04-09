namespace Markwardt.Network;

public interface IHostHandler<T> : IConnectionHandler<T>
{
    void OnHostStopped(Exception? exception);
}

public record HostHandler<T> : ConnectionHandler<T>, IHostHandler<T>
{
    public Action<Exception?>? StoppedHandler { get; init; }

    public void OnHostStopped(Exception? exception)
        => StoppedHandler?.Invoke(exception);
}