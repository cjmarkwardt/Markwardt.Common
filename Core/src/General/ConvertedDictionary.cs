namespace Markwardt;

public static class ConvertedDictionaryExtensions
{
    public static ConvertedDictionary<TKey, TValue, TConvertedValue> ConvertValues<TKey, TValue, TConvertedValue>(this IReadOnlyDictionary<TKey, TValue> source, Func<TValue, TConvertedValue> convert)
        => new(source, convert);
}

public class ConvertedDictionary<TKey, TValue, TConvertedValue>(IReadOnlyDictionary<TKey, TValue> source, Func<TValue, TConvertedValue> convert) : IReadOnlyDictionary<TKey, TConvertedValue>
{
    public TConvertedValue this[TKey key] => convert(source[key]);

    public IEnumerable<TKey> Keys => source.Keys;

    public IEnumerable<TConvertedValue> Values => source.Values.Select(convert);

    public int Count => source.Count;

    public bool ContainsKey(TKey key)
        => source.ContainsKey(key);

    public IEnumerator<KeyValuePair<TKey, TConvertedValue>> GetEnumerator()
        => source.Select(x => new KeyValuePair<TKey, TConvertedValue>(x.Key, convert(x.Value))).GetEnumerator();

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TConvertedValue value)
    {
        if (source.TryGetValue(key, out TValue? sourceValue))
        {
            value = convert(sourceValue);
            return true;
        }

        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
