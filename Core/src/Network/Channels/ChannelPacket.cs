namespace Markwardt;

public interface IChannelPacket
{
    Maybe<MessageChannelHeader> GetChannel();
    void SetChannel(MessageChannelHeader header);
}