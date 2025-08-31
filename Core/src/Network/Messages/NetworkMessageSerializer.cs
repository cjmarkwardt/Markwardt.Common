namespace Markwardt;

public interface INetworkMessageSerializer
{
    void Serialize(INetworkMessageConnection connection, INetworkChannel? channel, object message, IMemoryWriter<byte> writer);
    object Deserialize(INetworkMessageConnection connection, INetworkChannel? channel, ReadOnlySpan<byte> data);
}