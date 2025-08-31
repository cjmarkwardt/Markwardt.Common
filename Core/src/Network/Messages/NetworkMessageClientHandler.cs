namespace Markwardt;

public interface INetworkMessageClientHandler : INetworkMessageProcessorHandler
{
    void OnOpened();
    void OnClosed(Exception? exception);
}