namespace Markwardt;

public record SteamTarget(SteamNetworkingIdentity Id)
{
    public static implicit operator SteamTarget(SteamNetworkingIdentity id)
        => new(id);

    public static implicit operator SteamNetworkingIdentity(SteamTarget target)
        => target.Id;

    public static implicit operator SteamTarget(CSteamID id)
    {
        SteamNetworkingIdentity networkId = new();
        networkId.SetSteamID(id);
        return new(networkId);
    }

    public static SteamTarget Self { get; } = new(SteamUser.GetSteamID());
}