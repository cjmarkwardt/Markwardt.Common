namespace Markwardt.Network;

public interface IRemoteEntityResolver<TContent>
{
    TContent Resolve<T>(T value);
}

public abstract class RemoteEntityValue<TContent, T>
{
    public abstract TContent GetContent(T value);
    public abstract T GetValue(TContent content);
}

public interface IRemoteEntity<TContent> : IDisposable
{
    int Id { get; }

    Maybe<T> GetValue<T>(RemoteEntityValue<TContent, T> key);
    void SetValue<T>(RemoteEntityValue<TContent, T> key, T value);
    void CloseValue<T>(RemoteEntityValue<TContent, T> key);
}

public interface IRemoteEntityManager<TContent>
{
    IReadOnlyDictionary<int, IRemoteEntity<TContent>> Entities { get; }

    IRemoteEntity<TContent> CreateEntity();
}