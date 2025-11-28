namespace Markwardt;

public interface INetworkHasher
{
    byte[] Hash(ReadOnlySpan<byte> data);
}