namespace Markwardt;

public interface INetworkEncryptor
{
    void Encrypt(ReadOnlySpan<byte> input, Stream output);
    void Decrypt(ReadOnlySpan<byte> input, Stream output);
}