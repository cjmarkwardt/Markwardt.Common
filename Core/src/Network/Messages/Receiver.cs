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
        if (packet.TryAsContent<T>().TryGetValue(out Packet<T> contentPacket) && Filter(contentPacket))
        {
            Receive(contentPacket);
        }
    }

    protected abstract void Receive(Packet<T> packet);

    protected virtual bool Filter(Packet<T> packet)
        => true;
}