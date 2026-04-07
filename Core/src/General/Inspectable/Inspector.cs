namespace Markwardt;

public static class FallbackDictionaryExtensions
{
    public static IDictionary<TKey, TValue> WithFallback<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> fallback)
        => new FallbackDictionary<TKey, TValue>(dictionary, fallback);
}

public class FallbackDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> fallback) : IDictionary<TKey, TValue>
{
    public TValue this[TKey key]
    {
        get => dictionary.TryGetValue(key, out var value) ? value : fallback[key];
        set => dictionary[key] = value;
    }

    public ICollection<TKey> Keys => dictionary.Keys.Concat(fallback.Keys.Where(x => !dictionary.ContainsKey(x))).ToList();
    public ICollection<TValue> Values => dictionary.Values.Concat(fallback.Where(x => !dictionary.ContainsKey(x.Key)).Select(x => x.Value)).ToList();
    public int Count => Keys.Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public void Add(TKey key, TValue value)
        => dictionary.Add(key, value);

    public void Add(KeyValuePair<TKey, TValue> item)
        => dictionary.Add(item);

    public void Clear()
        => dictionary.Clear();

    public bool Contains(KeyValuePair<TKey, TValue> item)
        => dictionary.Contains(item) || fallback.Contains(item);

    public bool ContainsKey(TKey key)
        => dictionary.ContainsKey(key) || fallback.ContainsKey(key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        => this.ToList().CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => this.ToList().GetEnumerator();

    public bool Remove(TKey key)
        => dictionary.Remove(key);

    public bool Remove(KeyValuePair<TKey, TValue> item)
        => dictionary.Remove(item);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        => dictionary.TryGetValue(key, out value) || fallback.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}