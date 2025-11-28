namespace Markwardt;

public record NetworkStartSessionMessage(ReadOnlyMemory<byte> SessionResponseData) : NetworkControlMessage(NetworkControlHeader.StartSession)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
        => buffer.WriteBlock(SessionResponseData);
}