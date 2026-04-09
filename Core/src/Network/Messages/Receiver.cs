namespace Markwardt.Network;

public interface IReceiver
{
    void Receive(Packet packet);
}

public abstract class Receiver<T> : IReceiver
    where T : notnull
{
    public void Receive(Packet packet)
    {
        if (packet.Content is T content && Filter(packet, content))
        {
            Receive(packet, content);
        }
    }

    protected abstract void Receive(Packet packet, T content);

    protected virtual bool Filter(Packet packet, T content)
        => true;
}