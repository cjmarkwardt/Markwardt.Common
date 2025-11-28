namespace Markwardt;

public record NetworkIdOptions
{
    public TimeSpan? HostIdReuseDelay { get; init; } = TimeSpan.FromMinutes(10);
    public TimeSpan? ConnectionIdReuseDelay { get; init; } = TimeSpan.FromMinutes(10);
    public TimeSpan? GroupChannelIdReuseDelay { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan? ChannelIdReuseDelay { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan? RequestIdReuseDelay { get; init; } = TimeSpan.FromMinutes(1);
}

public record NetworkManagerOptions
{
    public INetworkAuthenticator? Authenticator { get; init; }
    public INetworkBlockPool? DefaultPool { get; init; }
    public bool SecureByDefault { get; init; }
}