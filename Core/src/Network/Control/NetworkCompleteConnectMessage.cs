namespace Markwardt;

public record NetworkCompleteConnectMessage(object? Response) : NetworkControlMessage(NetworkControlHeader.CompleteConnect)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
        => WriteMessage(serializer, buffer, Response);
}