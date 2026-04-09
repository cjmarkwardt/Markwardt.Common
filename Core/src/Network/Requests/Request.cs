namespace Markwardt.Network;

public interface IRequest
{
    int RequestId { get; }

    ValueTask<Packet> GetResponse(TimeSpan? timeout = null, CancellationToken cancellation = default);
}