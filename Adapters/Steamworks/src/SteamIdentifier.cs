namespace Markwardt;

internal interface ISteamIdentifier
{
    ulong? GetId();
    string? GetName();
    string? GetName(ulong id);
}

internal class SteamIdentifier : ISteamIdentifier
{
    public required ISteamInitializer Initializer { get; init; }

    public bool IsAvailable => Initializer.IsInitialized;

    public ulong? GetId()
        => Initializer.IsInitialized ? (ulong)SteamUser.GetSteamID() : null;

    public string? GetName()
        => Initializer.IsInitialized ? SteamFriends.GetPersonaName() : null;

    public string? GetName(ulong id)
        => Initializer.IsInitialized ? SteamFriends.GetFriendPersonaName((CSteamID)id) : null;
}