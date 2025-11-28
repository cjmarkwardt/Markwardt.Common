namespace Markwardt;

public interface INetworkTracker<T>
{
    TimeSpan ReuseDelay { get; set; }

    IEnumerable<T> List();
    T? Find(int id);
    int Add(T item);
    void Remove(int id);
}

public class NetworkTracker<T>(int idStart, bool negate = false) : INetworkTracker<T>
{
    private readonly Dictionary<int, T> items = [];
    private readonly IdRange ids = new(idStart);

    public TimeSpan ReuseDelay { get => ids.ReuseDelay; set => ids.ReuseDelay = value; }

    public int Add(T item)
    {
        int id = ids.Next();

        if (negate)
        {
            id = -id;
        }

        items.Add(id, item);
        return id;
    }

    public T? Find(int id)
        => items.GetValueOrDefault(id);

    public IEnumerable<T> List()
        => items.Values;

    public void Remove(int id)
    {
        ids.Release(id);
        items.Remove(id);
    }
}