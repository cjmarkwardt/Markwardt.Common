namespace Markwardt;

public interface IKeyLookup<T, TKey> : IReadOnlyKeyLookup<T, TKey>
{
    bool RemoveKey(TKey key);
}