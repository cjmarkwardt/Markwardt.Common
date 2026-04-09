namespace Markwardt.Network;

public interface IChannel : IDisposable, ISender
{
    bool IsPending { get; }

    TimeSpan? AutoAssertDelay { get; set; }

    ISender Asserter { get; }

    void Assert();
}

public interface IChannel<T> : IChannel, ISender<T>
{
    new ISender<T> Asserter { get; }
}

public static class ChannelExtensions
{
    public static IChannel<T> As<T>(this IChannel channel)
        => new Channel<T>(channel);
}

public class Channel<T>(IChannel channel) : BaseDisposable, IChannel<T>
{
    public bool IsPending => channel.IsPending;
    public TimeSpan? AutoAssertDelay { get => channel.AutoAssertDelay; set => channel.AutoAssertDelay = value; }

    public ISender<T> Asserter { get; } = new Sender<T>(channel.Asserter);

    ISender IChannel.Asserter => channel.Asserter;

    public void Send(Packet packet)
        => channel.Send(packet);

    public void Assert()
        => channel.Assert();

    protected override void OnDispose()
    {
        base.OnDispose();

        channel.Dispose();
    }
}