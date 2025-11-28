namespace Markwardt;

public interface INetworkSession
{
    string Identifier { get; }
    byte[] Data { get; }

    INetworkEncryptor CreateEncryptor(ReadOnlySpan<byte> responseData);
}

public static class NetworkSessionExtensions
{
    public static byte[] Encrypt(this INetworkEncryptor session, ReadOnlySpan<byte> input)
    {
        using MemoryStream output = new();
        session.Encrypt(input, output);
        return output.ToArray();
    }

    public static byte[] Decrypt(this INetworkEncryptor session, ReadOnlySpan<byte> input)
    {
        using MemoryStream output = new();
        session.Decrypt(input, output);
        return output.ToArray();
    }

    public static string Encrypt(this INetworkEncryptor session, string input)
        => Convert.ToBase64String(session.Encrypt(Encoding.UTF8.GetBytes(input)));

    public static string Decrypt(this INetworkEncryptor session, string input)
        => Encoding.UTF8.GetString(session.Decrypt(Convert.FromBase64String(input)));
}