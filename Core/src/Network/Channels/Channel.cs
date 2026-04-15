namespace Markwardt.Network;

public interface IChannel : IDisposable
{
    bool IsPending { get; }

    TimeSpan? AutoAssertDelay { get; set; }

    void Send(Packet packet);
    void Assert(Packet packet);
    void Assert();
}

public interface IChannel<T> : IDisposable
{
    bool IsPending { get; }

    TimeSpan? AutoAssertDelay { get; set; }

    void Send(Packet<T> packet);
    void Assert(Packet<T> packet);
    void Assert();
}

public static class ChannelExtensions
{
    public static IChannel<T> As<T>(this IChannel channel)
        => new Channel<T>(channel);

    public static void Send<T>(this IChannel<T> channel, T content, Action<Packet<T>>? configure = null)
        => channel.Send(Packet.New(content).Configure(configure));

    public static void Assert<T>(this IChannel<T> channel, T content, Action<Packet<T>>? configure = null)
        => channel.Assert(Packet.New(content).Configure(configure));
}

public class Channel<T>(IChannel channel) : BaseDisposable, IChannel<T>
{
    public bool IsPending => channel.IsPending;
    public TimeSpan? AutoAssertDelay { get => channel.AutoAssertDelay; set => channel.AutoAssertDelay = value; }

    public void Send(Packet<T> packet)
        => channel.Send(packet);

    public void Assert(Packet<T> packet)
        => channel.Assert(packet);

    public void Assert()
        => channel.Assert();

    protected override void OnDispose()
    {
        base.OnDispose();

        channel.Dispose();
    }
}