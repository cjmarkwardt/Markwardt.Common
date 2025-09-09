namespace Markwardt;

public interface ISourceSet<T> : ISourceCollection<T>, IObservableSet<T>, ISet<T>;

public interface ISourceSet<T, TKey> : ISourceSet<T>, IObservableSet<T, TKey>;

public class SourceSet<T> : SourceCollection<HashSet<T>, T>, ISourceSet<T>
{
    public new bool Add(T item)
    {
        bool result = Items.Add(item);
        CommitAdd(item);
        return result;
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
        => Items.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other)
        => Items.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other)
        => Items.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other)
        => Items.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other)
        => Items.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other)
        => Items.SetEquals(other);

    public void ExceptWith(IEnumerable<T> other)
    {
        throw new NotImplementedException();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        throw new NotImplementedException();
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        throw new NotImplementedException();
    }

    public void UnionWith(IEnumerable<T> other)
    {
        throw new NotImplementedException();
    }
}


public class SourceSet<T, TKey>(Func<T, TKey> getKey) : SourceSet<T>, ISourceSet<T, TKey>
{
    private readonly ExtendedDictionary<TKey, T> lookup = [];

    public bool TryLookupKey(TKey key, [MaybeNullWhen(false)] out T value)
        => lookup.TryGetValue(key, out value);

    protected override void OnCommitAdd(T item)
    {
        base.OnCommitAdd(item);
        lookup.Add(getKey(item), item);
    }

    protected override void OnCommitRemove(T item)
    {
        base.OnCommitRemove(item);
        lookup.Remove(getKey(item));
    }
}