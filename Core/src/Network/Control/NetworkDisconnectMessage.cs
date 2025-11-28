namespace Markwardt;

public record NetworkDisconnectMessage(string Reason) : NetworkControlMessage(NetworkControlHeader.Disconnect)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
        => buffer.WriteString(Reason);
}