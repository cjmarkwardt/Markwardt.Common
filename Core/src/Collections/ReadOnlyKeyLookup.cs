namespace Markwardt;

public interface IReadOnlyKeyLookup<T, TKey>
{
    IEnumerable<TKey> Keys { get; }

    TKey GetKey(T item);
    bool ContainsKey(TKey key);
    bool TryLookupKey(TKey key, [MaybeNullWhen(false)] out T value);
}