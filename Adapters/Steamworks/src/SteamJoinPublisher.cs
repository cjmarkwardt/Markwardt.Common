namespace Markwardt;

public class SteamJoinPublisher : INetworkJoinPublisher
{
    public void Publish(string? connector)
        => SteamFriends.SetRichPresence("connect", connector ?? string.Empty);
}