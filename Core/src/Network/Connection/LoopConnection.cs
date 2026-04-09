namespace Markwardt.Network;

public class LoopConnection<T> : Connection<T>
{
    public static (LoopConnection<T>, LoopConnection<T>) Connect()
    {
        LoopConnection<T> first = new();
        LoopConnection<T> second = new();

        first.target = second;
        second.target = first;

        first.SetConnected();
        second.SetConnected();

        return (first, second);
    }

    private LoopConnection() { }

    private LoopConnection<T> target = null!;

    protected override void SendContent(Packet packet, T content)
        => target.TriggerReceived(packet);

    protected override void OnDisconnected(Exception? exception)
    {
        base.OnDisconnected(exception);

        target.SetDisconnected(new RemoteDisconnectException());
    }
}