namespace Markwardt;

public class LoopConnection<T> : MessageConnection<T>
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

    protected override void SendContent(Message message, T content)
        => target.TriggerReceived(message);

    protected override void OnDisconnected(Exception? exception)
    {
        base.OnDisconnected(exception);

        target.SetDisconnected(new RemoteDisconnectException());
    }
}