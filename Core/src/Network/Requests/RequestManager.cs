namespace Markwardt.Network;

public interface IRequestManager
{
    ValueTask<Packet> Request(Packet packet, TimeSpan? timeout, CancellationToken cancellation);
}