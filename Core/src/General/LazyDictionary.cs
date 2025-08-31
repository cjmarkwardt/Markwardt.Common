namespace Markwardt;

public interface ILazyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
    TValue Get(TKey key);
    void Remove(TKey key);
}

public class LazyDictionary<TKey, TValue>(Func<TKey, TValue> create) : ILazyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> dictionary = [];

    public int Count => dictionary.Count;

    public TValue Get(TKey key)
    {
        if (!dictionary.TryGetValue(key, out TValue? value))
        {
            value = create(key);
            dictionary[key] = value;
        }

        return value;
    }

    public void Remove(TKey key)
        => dictionary.Remove(key);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
