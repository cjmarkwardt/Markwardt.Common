namespace Markwardt;

public class SteamJoinPublisher
{
    public void Publish(string? connector)
        => SteamFriends.SetRichPresence("connect", connector ?? string.Empty);
}