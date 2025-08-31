namespace Markwardt;

public class SteamJoinListener : INetworkJoinListener
{
    public IDisposable Subscribe(IObserver<string> observer)
        => Callback<GameRichPresenceJoinRequested_t>.Create(callback => observer.OnNext(callback.m_rgchConnect));
}