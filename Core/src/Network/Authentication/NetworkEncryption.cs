namespace Markwardt;

public interface INetworkEncryption
{
    INetworkEncryptor CreateEncryptor(ReadOnlySpan<byte> key);
}