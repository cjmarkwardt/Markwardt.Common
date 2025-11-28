namespace Markwardt;

public interface INetworkHost : IDisposable
{
    object? Profile { get; }
    IEnumerable<INetworkConnection> Connections { get; }
}