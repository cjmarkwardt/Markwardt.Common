namespace Markwardt;

public record NetworkSecureMessage() : NetworkControlMessage(NetworkControlHeader.Secure)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer) { }
}