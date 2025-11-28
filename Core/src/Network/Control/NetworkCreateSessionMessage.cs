namespace Markwardt;

public record NetworkCreateSessionMessage(ReadOnlyMemory<byte> SessionData) : NetworkControlMessage(NetworkControlHeader.CreateSession)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
        => buffer.WriteBlock(SessionData);
}