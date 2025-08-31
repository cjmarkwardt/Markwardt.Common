namespace Markwardt;

public interface IReadOnlyMap<TKey, T> : IReadOnlyCollection<KeyValuePair<TKey, T>>
{
    IEnumerable<TKey> Keys { get; }
    IEnumerable<T> Values { get; }

    bool Contains(TKey key);
    bool TryGet(TKey key, [MaybeNullWhen(false)] out T value);
}

public static class ReadOnlyMapExtensions
{
    public static bool Contains<TKey, T>(this IReadOnlyMap<TKey, T> map, TKey key, T value)
        => map.Any(x => x.Key.ValueEquals(key) && x.Value.ValueEquals(value));

    public static bool ContainsValue<TKey, T>(this IReadOnlyMap<TKey, T> map, T value)
        => map.Values.Any(x => x.ValueEquals(value));

    public static int CountValue<TKey, T>(this IReadOnlyMap<TKey, T> map, T value)
        => map.Values.Count(x => x.ValueEquals(value));

    public static Maybe<T> MaybeGet<TKey, T>(this IReadOnlyMap<TKey, T> map, TKey key)
        => map.TryGet(key, out T? value) ? value.Maybe() : default;

    public static T Get<TKey, T>(this IReadOnlyMap<TKey, T> map, TKey key)
        => map.TryGet(key, out T? value) ? value : throw new KeyNotFoundException($"Key {key} not found");

    public static T GetOrDefault<TKey, T>(this IReadOnlyMap<TKey, T> map, TKey key, T defaultValue)
        => map.TryGet(key, out T? value) ? value : defaultValue;

    public static T? GetOrDefault<TKey, T>(this IReadOnlyMap<TKey, T> map, TKey key)
        => map.TryGet(key, out T? value) ? value : default;

    public static IEnumerable<TKey> FindValue<TKey, T>(this IReadOnlyMap<TKey, T> map, T value)
        => map.Where(x => x.Value.ValueEquals(value)).Select(x => x.Key);
}