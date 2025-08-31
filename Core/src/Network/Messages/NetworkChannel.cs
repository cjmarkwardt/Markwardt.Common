namespace Markwardt;

public interface INetworkChannel
{
    int Id { get; }

    object? Profile { get; set; }
    TimeSpan SafetyDelay { get; set; }

    void Send(object message);
    void Destroy();
}