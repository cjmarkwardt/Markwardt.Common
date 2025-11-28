namespace Markwardt;

public interface INetworkChannelProvider
{
    INetworkHandler Handler { get; }
    INetworkFormatSender Sender { get; }
    INetworkTracker<INetworkRemoteChannel> ChannelTracker { get; }

    void SetUnsynced(INetworkRemoteChannel channel);
    void SetSynced(INetworkRemoteChannel channel);
}