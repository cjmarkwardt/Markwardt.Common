namespace Markwardt;

public record NetworkCloseChannelMessage(int ChannelId) : NetworkControlMessage(NetworkControlHeader.CloseChannel)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
        => buffer.WriteVariableInteger(ChannelId);
}