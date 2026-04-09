namespace Markwardt.Network;

public interface IChannelManager
{
    IEnumerable<IChannel> Channels { get; }

    IObservable<(Packet Message, IObservable<Packet> Messages)> Received { get; }

    IChannel OpenChannel(Packet packet, TimeSpan? autoAssertDelay);
}