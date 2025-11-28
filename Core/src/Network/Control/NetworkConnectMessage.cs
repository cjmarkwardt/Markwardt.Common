namespace Markwardt;

public record NetworkConnectMessage(object? Request) : NetworkControlMessage(NetworkControlHeader.Connect)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
        => WriteMessage(serializer, buffer, Request);
}