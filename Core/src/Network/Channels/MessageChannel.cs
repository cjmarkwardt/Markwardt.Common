namespace Markwardt;

public interface IMessageChannel : IDisposable, IMessageSender
{
    bool IsPending { get; }

    TimeSpan? AutoAssertDelay { get; set; }

    IMessageAsserter Asserter { get; }
}

public interface IMessageChannel<T> : IMessageChannel, IMessageSender<T>
{
    new IMessageAsserter<T> Asserter { get; }
}

public static class MessageChannelExtensions
{
    public static IMessageChannel<T> As<T>(this IMessageChannel channel)
        => new MessageChannel<T>(channel);
}

public class MessageChannel<T>(IMessageChannel channel) : BaseDisposable, IMessageChannel<T>
{
    public bool IsPending => channel.IsPending;
    public TimeSpan? AutoAssertDelay { get => channel.AutoAssertDelay; set => channel.AutoAssertDelay = value; }

    public IMessageAsserter<T> Asserter { get; } = new MessageAsserter<T>(channel.Asserter);

    IMessageAsserter IMessageChannel.Asserter => channel.Asserter;

    public void Send(Message message)
        => channel.Send(message);

    protected override void OnDispose()
    {
        base.OnDispose();

        channel.Dispose();
    }
}