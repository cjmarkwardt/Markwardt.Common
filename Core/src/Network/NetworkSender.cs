namespace Markwardt;

public interface INetworkSender
{
    ValueTask Send(ReadOnlyMemory<byte> data, NetworkReliability mode, CancellationToken cancellation = default);
}