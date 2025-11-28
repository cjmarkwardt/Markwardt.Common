namespace Markwardt;

public record NetworkAuthenticateMessage(string Identifier) : NetworkControlMessage(NetworkControlHeader.Authenticate)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
        => buffer.WriteString(Identifier);
}