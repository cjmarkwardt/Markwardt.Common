namespace Markwardt;

public class SteamJoinListener
{
    public IDisposable Subscribe(IObserver<string> observer)
        => Callback<GameRichPresenceJoinRequested_t>.Create(callback => observer.OnNext(callback.m_rgchConnect));
}