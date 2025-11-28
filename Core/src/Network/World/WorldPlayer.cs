namespace Markwardt;

public interface IWorldPlayer : IDisposable
{
    string Name { get; }
    bool IsCurrent { get; }
    bool IsAdmin { get; }
    INetworkConnection Connection { get; }
}

public class WorldPlayer(string name, bool isCurrent) : IWorldPlayer
{
    public string Name => name;
    public bool IsCurrent => isCurrent;

    public bool IsAdmin { get; set; }
    public INetworkConnection Connection { get; set; } = default!;

    public void Dispose()
        => Connection.Dispose();
}