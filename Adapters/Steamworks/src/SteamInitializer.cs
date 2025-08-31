namespace Markwardt;

public interface ISteamInitializer : IDisposable
{
    bool IsInitialized { get; }

    void Initialize(int applicationId);
}

public class SteamInitializer : ISteamInitializer
{
    private bool isDisposed;

    public bool IsInitialized { get; private set; }

    public void Initialize(int applicationId)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (IsInitialized)
        {
            throw new InvalidOperationException("Already initialized");
        }

        File.WriteAllText("steam_appid.txt", applicationId.ToString());

        try
        {
            if (!DllCheck.Test() || !Packsize.Test())
            {
                throw new InvalidOperationException("Invalid version of Steam API");
            }

            if (!SteamAPI.Init())
            {
                throw new InvalidOperationException("Failed to initialize Steam");
            }
        }
        catch (DllNotFoundException)
        {
            throw new InvalidOperationException("Steam API DLL not found");
        }

        IsInitialized = true;
        StartCallbacks();
    }

    public void Dispose()
    {
        if (!isDisposed && IsInitialized)
        {
            isDisposed = true;
            IsInitialized = false;
            SteamAPI.Shutdown();
        }
    }

    private async void StartCallbacks()
    {
        while (!isDisposed)
        {
            SteamAPI.RunCallbacks();
            await Task.Delay(50);
        }
    }
}