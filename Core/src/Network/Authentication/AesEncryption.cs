using System.Security.Cryptography;

namespace Markwardt;

public class AesEncryption(INetworkHasher hasher) : INetworkEncryption
{
    public AesEncryption()
        : this(new Sha256Hasher()) { }

    private readonly byte[] iv = new byte[16];

    public INetworkEncryptor CreateEncryptor(ReadOnlySpan<byte> key)
        => new Encryptor(hasher.Hash(key), iv);

    private sealed class Encryptor(byte[] key, byte[] iv) : INetworkEncryptor
    {
        public void Encrypt(ReadOnlySpan<byte> input, Stream output)
        {
            using Aes aes = CreateAes(key);
            aes.GenerateIV();
            output.Write(aes.IV);
            using CryptoStream encryptor = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            encryptor.Write(input);
        }

        public void Decrypt(ReadOnlySpan<byte> input, Stream output)
        {
            input[..16].CopyTo(iv);
            using Aes aes = CreateAes(key);
            using CryptoStream decryptor = new(output, aes.CreateDecryptor(), CryptoStreamMode.Write);
            decryptor.Write(input[16..]);
        }

        private Aes CreateAes(byte[] key)
        {
            Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            return aes;
        }
    }
}