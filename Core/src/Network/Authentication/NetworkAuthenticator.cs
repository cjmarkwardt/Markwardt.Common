namespace Markwardt;

public interface INetworkAuthenticator
{
    byte[] CreateVerifier(string identifier, string secret);
    INetworkSession CreateSession(string identifier, ReadOnlySpan<byte> verifier);
    (INetworkEncryptor Encryptor, byte[] ResponseData) CreateEncryptor(ReadOnlySpan<byte> sessionData, string identifier, string secret);
}

public static class NetworkAuthenticatorExtensions
{
    public static INetworkSession CreateSession(this INetworkAuthenticator authenticator)
        => authenticator.CreateSession(string.Empty, authenticator.CreateVerifier(string.Empty, string.Empty));

    public static (INetworkEncryptor Encryptor, byte[] ResponseData) CreateEncryptor(this INetworkAuthenticator authenticator, ReadOnlySpan<byte> sessionData)
        => authenticator.CreateEncryptor(sessionData, string.Empty, string.Empty);

    public static bool Verify(this INetworkAuthenticator authenticator, string identifier, string secret, ReadOnlySpan<byte> verifier)
    {
        try
        {
            INetworkSession session = authenticator.CreateSession(identifier, verifier);
            (_, byte[] responseData) = authenticator.CreateEncryptor(session.Data, identifier, secret);
            session.CreateEncryptor(responseData);
            return true;
        }
        catch (NetworkAuthenticationException)
        {
            return false;
        }
    }
}