namespace Markwardt;

public interface INetworkSerializer
{
    void Serialize(object message, IMemoryWriteable<byte> writer);
    object Deserialize(MemoryReader<byte> reader, ReadOnlySpan<byte> data);
}