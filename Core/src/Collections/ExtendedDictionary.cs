namespace Markwardt;

public static class ExtendedDictionaryExtensions
{
    public static ExtendedDictionary<TKey, TValue> ToExtendedDictionary<T, TKey, TValue>(this IEnumerable<T> items, Func<T, TKey> selectKey, Func<T, TValue> selectValue)
        => new(items.Select(x => new KeyValuePair<TKey, TValue>(selectKey(x), selectValue(x))));

    public static ExtendedDictionary<TKey, T> ToExtendedDictionary<T, TKey>(this IEnumerable<T> items, Func<T, TKey> selectKey)
        => items.ToExtendedDictionary(selectKey, x => x);
}

public class ExtendedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
{
    private static Maybe<TKey> ConvertKey(TKey key)
        => key is null ? new Maybe<TKey>() : key.Maybe();

    private static TKey DeconvertKey(Maybe<TKey> key)
        => key.HasValue ? key.Value : default!;

    public ExtendedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> pairs, DictionaryComparer<TKey, TValue>? comparer = null)
    {
        dictionary = comparer is null ? new Dictionary<Maybe<TKey>, TValue>() : new SortedDictionary<Maybe<TKey>, TValue>(new Comparer(this, comparer));
        keys = new ReadOnlyCollection<TKey>(() => Count, ContainsKey, dictionary.Keys.Select(DeconvertKey));
        values = new ReadOnlyCollection<TValue>(() => Count, dictionary.Values.Contains, dictionary.Values);

        foreach (KeyValuePair<TKey, TValue> pair in pairs)
        {
            dictionary.Add(ConvertKey(pair.Key), pair.Value);
        }
    }

    public ExtendedDictionary(DictionaryComparer<TKey, TValue>? sorter = null)
        : this([], sorter) { }

    private readonly IDictionary<Maybe<TKey>, TValue> dictionary;
    private readonly ICollection<TKey> keys;
    private readonly ICollection<TValue> values;

    public TValue this[TKey key]
    {
        get => dictionary[ConvertKey(key)];
        set => dictionary[ConvertKey(key)] = value;
    }

    public int Count => dictionary.Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public ICollection<TKey> Keys => keys;
    public ICollection<TValue> Values => values;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => values;

    public void Add(TKey key, TValue value)
        => dictionary.Add(ConvertKey(key), value);

    public void Clear()
        => dictionary.Clear();

    public bool ContainsKey(TKey key)
        => dictionary.ContainsKey(ConvertKey(key));

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            array[arrayIndex++] = pair;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => dictionary.Select(x => new KeyValuePair<TKey, TValue>(DeconvertKey(x.Key), x.Value)).GetEnumerator();

    public bool Remove(TKey key)
        => dictionary.Remove(ConvertKey(key));

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        => dictionary.TryGetValue(ConvertKey(key), out value);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        => dictionary.Add(new KeyValuePair<Maybe<TKey>, TValue>(ConvertKey(item.Key), item.Value));

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        => dictionary.Contains(new KeyValuePair<Maybe<TKey>, TValue>(ConvertKey(item.Key), item.Value));

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        => dictionary.Remove(new KeyValuePair<Maybe<TKey>, TValue>(ConvertKey(item.Key), item.Value));

    private sealed class Comparer(ExtendedDictionary<TKey, TValue> dictionary, DictionaryComparer<TKey, TValue> comparer) : IComparer<Maybe<TKey>>
    {
        public int Compare(Maybe<TKey> x, Maybe<TKey> y)
            => comparer(new(DeconvertKey(x), () => dictionary.dictionary[x]), new(DeconvertKey(y), () => dictionary.dictionary[y]));
    }

    private sealed class ReadOnlyCollection<TItem>(Func<int> count, Func<TItem, bool> contains, IEnumerable<TItem> items) : ICollection<TItem>
    {
        public int Count => count();
        public bool IsReadOnly => true;

        public bool Contains(TItem item)
            => contains(item);

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            foreach (TItem item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public IEnumerator<TItem> GetEnumerator()
            => items.GetEnumerator();

        public void Add(TItem item)
            => throw new NotSupportedException();

        public bool Remove(TItem item)
            => throw new NotSupportedException();

        public void Clear()
            => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}