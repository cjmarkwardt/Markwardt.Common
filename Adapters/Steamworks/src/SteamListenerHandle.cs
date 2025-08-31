namespace Markwardt;

public class SteamListenerHandle(HSteamListenSocket value) : Finalized<HSteamListenSocket>(value)
{
    protected override void Release(HSteamListenSocket value)
        => SteamNetworkingSockets.CloseListenSocket(value);
}