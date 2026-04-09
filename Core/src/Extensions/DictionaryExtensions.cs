namespace Markwardt;

public static class DictionaryExtensions
{
    public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> getValue)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, getValue());
        }
    }

    public static Maybe<TValue> MaybeGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        => dictionary.TryGetValue(key, out TValue? value) ? value.Maybe() : default;

    public static void Overwrite<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        => pairs.ForEach(x => dictionary[x.Key] = x.Value);

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> getValue)
    {
        if (!dictionary.TryGetValue(key, out TValue? value))
        {
            value = getValue();
            dictionary.Add(key, value);
        }

        return value;
    }
}