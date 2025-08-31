namespace Markwardt;

public interface INetworkPeer : ITrackedDisposable
{
    bool IsOpen { get; }
    Exception? Exception { get; }

    ValueTask Close(CancellationToken cancellation = default);
}