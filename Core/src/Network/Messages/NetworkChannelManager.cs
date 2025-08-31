namespace Markwardt;

public interface INetworkChannelManager
{
    IReadOnlyDictionary<int, INetworkChannel> Channels { get; }

    INetworkChannel CreateChannel(int id);
}