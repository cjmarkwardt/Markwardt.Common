namespace Markwardt;

public interface IChannelManager
{
    IEnumerable<IMessageChannel> Channels { get; }

    IObservable<(Message Message, IObservable<Message> Messages)> Received { get; }

    IMessageChannel OpenChannel(Message message, TimeSpan? autoAssertDelay);
}