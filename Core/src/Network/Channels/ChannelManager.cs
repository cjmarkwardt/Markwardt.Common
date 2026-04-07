namespace Markwardt;

public interface IMessageReceiveChannel
{
    
}

public interface IChannelManager
{
    IEnumerable<IMessageChannel> Channels { get; }

    //IObservable<IObservable<Message>> ReceivedChannels { get; }

    IMessageChannel OpenChannel(TimeSpan? autoAssertDelay);
}