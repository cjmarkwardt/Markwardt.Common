namespace Markwardt;

public interface IReadOnlyKeyLookup<T, TKey>
{
    bool TryLookupKey(TKey key, [MaybeNullWhen(false)] out T value);
}

public static class ReadOnlyKeyLookupExtensions
{
    public static bool ContainsKey<T, TKey>(this IReadOnlyKeyLookup<T, TKey> lookup, TKey key)
        => lookup.TryLookupKey(key, out _);

    public static T? GetValueOrDefault<TKey, T>(this IReadOnlyKeyLookup<T, TKey> lookup, TKey key)
    {
        lookup.TryLookupKey(key, out var value);
        return value;
    }
}