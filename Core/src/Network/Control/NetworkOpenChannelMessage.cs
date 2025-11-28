namespace Markwardt;

public record NetworkOpenChannelMessage(int ChannelId, object Message) : NetworkControlMessage(NetworkControlHeader.OpenChannel)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
    {
        buffer.WriteVariableInteger(ChannelId);
        WriteMessage(serializer, buffer, Message);
    }
}