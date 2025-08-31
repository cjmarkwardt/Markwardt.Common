namespace Markwardt;

public interface INetworkMessageProcessorHandler
{
    void OnReceived(object message, INetworkChannel? channel);
    ValueTask<object> OnRequested(object message);
    void OnRecycled(object message);
}