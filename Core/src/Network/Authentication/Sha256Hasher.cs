using System.Security.Cryptography;

namespace Markwardt;

public class Sha256Hasher : INetworkHasher
{
    public byte[] Hash(ReadOnlySpan<byte> data)
        => SHA256.HashData(data);
}