namespace Markwardt;

public interface IWorldServer<TWorld>
{
    string WorldName { get; }
    TWorld World { get; }
    IEnumerable<INetworkHost> Hosts { get; }
    IEnumerable<IWorldPlayer> Players { get; }

    string? Password { get; set; }
    string? AdminPassword { get; set; }

    IWorldPlayer? FindPlayer(int id);
    void Rename(string name);
    void Host(INetworkHoster hoster, object? profile = null);
    ValueTask Join(string name, CancellationToken cancellation = default);
    ValueTask Broadcast(IEnumerable<IWorldPlayer> players, object message, NetworkSecurity? security = null, NetworkReliability mode = NetworkReliability.Reliable, CancellationToken cancellation = default);
    ValueTask Save(CancellationToken cancellation = default);
    ValueTask Delete(CancellationToken cancellation = default);
}

public static class WorldServerExtensions
{
    public static async ValueTask Broadcast(this IWorldServer<object> server, object message, NetworkSecurity? security = null, NetworkReliability mode = NetworkReliability.Reliable, CancellationToken cancellation = default)
        => await server.Broadcast(server.Players, message, security, mode, cancellation);
}