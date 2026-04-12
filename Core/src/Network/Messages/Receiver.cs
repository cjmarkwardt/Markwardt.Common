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
        Packet<T> typed = packet.As<T>();
        if (typed.IsContent && Filter(typed))
        {
            Receive(typed);
        }
    }

    protected abstract void Receive(Packet<T> packet);

    protected virtual bool Filter(Packet<T> packet)
        => true;
}