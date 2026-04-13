namespace Markwardt.Network;

public interface ISender
{
    void Send(Packet packet);
}

public interface ISender<T> : ISender;

public static class SenderExtensions
{
    public static void Send<T>(this ISender<T> sender, T content, Action<Packet<T>>? configure = null)
        => sender.Send(Packet.New(content).Configure(configure));
}

public class Sender(Action<Packet> send) : ISender
{
    public void Send(Packet packet)
        => send(packet);
}

public class Sender<T>(ISender sender) : ISender<T>
{
    public void Send(Packet packet)
        => sender.Send(packet);
}