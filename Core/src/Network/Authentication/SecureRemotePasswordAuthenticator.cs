using System.Security;
using SecureRemotePassword;

namespace Markwardt;

public class SecureRemotePasswordAuthenticator(INetworkEncryption encryption) : INetworkAuthenticator
{
    private static byte[] HexToBytes(string data)
        => SrpInteger.FromHex(data).ToByteArray();

    private static string BytesToHex(byte[] data)
        => SrpInteger.FromByteArray(data).ToHex();

    public SecureRemotePasswordAuthenticator()
        : this(new AesEncryption()) { }

    private readonly INetworkEncryption encryption = encryption;
    private readonly SrpClient client = new();
    private readonly SrpServer server = new();
    private readonly MemoryReader<byte> reader = new();

    public byte[] CreateVerifier(string identifier, string secret)
    {
        SrpClient client = new();
        string salt = client.GenerateSalt();
        string verifier = client.DeriveVerifier(client.DerivePrivateKey(salt, identifier, secret));

        return Buffer<byte>.CreateArray(writer =>
        {
            writer.WriteBlock(HexToBytes(salt));
            writer.WriteBlock(HexToBytes(verifier));
        });
    }

    public INetworkSession CreateSession(string identifier, ReadOnlySpan<byte> verifier)
        => new Session(this, identifier, verifier);

    public (INetworkEncryptor Encryptor, byte[] ResponseData) CreateEncryptor(ReadOnlySpan<byte> sessionData, string identifier, string secret)
    {
        reader.Position = 0;
        string salt = BytesToHex(reader.ReadBlock(sessionData));
        string remoteEphemeral = BytesToHex(reader.ReadBlock(sessionData));

        SrpEphemeral ephemeral = client.GenerateEphemeral();
        SrpSession session = client.DeriveSession(ephemeral.Secret, remoteEphemeral, salt, identifier, client.DerivePrivateKey(salt, identifier, secret));
        INetworkEncryptor encryptor = encryption.CreateEncryptor(HexToBytes(session.Key));

        byte[] data = Buffer<byte>.CreateArray(writer =>
        {
            writer.WriteBlock(HexToBytes(ephemeral.Public));
            writer.WriteBlock(HexToBytes(session.Proof));
        });
            
        return (encryptor, data);
    }

    private sealed class Session : INetworkSession
    {
        public Session(SecureRemotePasswordAuthenticator authenticator, string identifier, ReadOnlySpan<byte> verifier)
        {
            this.authenticator = authenticator;
            Identifier = identifier;

            authenticator.reader.Position = 0;
            saltData = authenticator.reader.ReadBlock(verifier);
            verifierData = authenticator.reader.ReadBlock(verifier);
            ephemeral = authenticator.server.GenerateEphemeral(BytesToHex(verifierData));

            Data = Buffer<byte>.CreateArray(data =>
            {
                data.WriteBlock(saltData);
                data.WriteBlock(HexToBytes(ephemeral.Public));
            });
        }

        private readonly SecureRemotePasswordAuthenticator authenticator;
        private readonly byte[] saltData;
        private readonly byte[] verifierData;
        private readonly SrpEphemeral ephemeral;

        public string Identifier { get; }
        public byte[] Data { get; }

        public INetworkEncryptor CreateEncryptor(ReadOnlySpan<byte> responseData)
        {
            try
            {
                authenticator.reader.Position = 0;
                string remoteEphemeral = BytesToHex(authenticator.reader.ReadBlock(responseData));
                string remoteProof = BytesToHex(authenticator.reader.ReadBlock(responseData));

                SrpSession session = authenticator.server.DeriveSession(ephemeral.Secret, remoteEphemeral, BytesToHex(saltData), Identifier, BytesToHex(verifierData), remoteProof);
                return authenticator.encryption.CreateEncryptor(HexToBytes(session.Key));
            }
            catch (SecurityException ex)
            {
                throw new NetworkAuthenticationException(null, ex);
            }
        }
    }
}