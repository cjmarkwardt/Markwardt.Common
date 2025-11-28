namespace Markwardt;

public interface INetworkFormatHandler
{
    ValueTask OnMessage(object message);
    ValueTask OnRequest(int requestId, object message);
    ValueTask OnResponse(int requestId, object message);
    ValueTask OnUpdate(int channelId, byte sequence, object message);
    ValueTask OnSync(int channelId, byte sequence);
    ValueTask OnControl(NetworkControlMessage message);
}