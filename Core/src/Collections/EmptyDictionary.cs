namespace Markwardt;

public class EmptyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
    public static EmptyDictionary<TKey, TValue> Instance { get; } = new();

    public TValue this[TKey key] => throw new KeyNotFoundException();

    public IEnumerable<TKey> Keys => [];

    public IEnumerable<TValue> Values => [];

    public int Count => 0;

    public bool ContainsKey(TKey key)
        => false;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}