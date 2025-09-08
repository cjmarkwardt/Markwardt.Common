namespace Markwardt;

public interface IReadOnlyKeyLookup<T, TKey>
{
    bool TryGetValue(TKey key, [MaybeNullWhen(false)] out T value);
}

public static class ReadOnlyKeyLookupExtensions
{
    public static bool ContainsKey<T, TKey>(this IReadOnlyKeyLookup<T, TKey> lookup, TKey key)
        => lookup.TryGetValue(key, out _);

    public static T? GetValueOrDefault<TKey, T>(this IReadOnlyKeyLookup<T, TKey> lookup, TKey key)
    {
        lookup.TryGetValue(key, out var value);
        return value;
    }
}