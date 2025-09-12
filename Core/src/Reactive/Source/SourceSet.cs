namespace Markwardt;

public interface ISourceKeySet : ISourceCollection, IObservableKeySet;

public interface ISourceSet<T> : ISourceCollection<T>, IObservableSet<T>, ISet<T>
{
    new bool Contains(T item)
        => ((ISet<T>)this).Contains(item);

    new bool IsProperSubsetOf(IEnumerable<T> other)
        => ((ISet<T>)this).IsProperSubsetOf(other);

    new bool IsProperSupersetOf(IEnumerable<T> other)
        => ((ISet<T>)this).IsProperSupersetOf(other);

    new bool IsSubsetOf(IEnumerable<T> other)
        => ((ISet<T>)this).IsSubsetOf(other);

    new bool IsSupersetOf(IEnumerable<T> other)
        => ((ISet<T>)this).IsSupersetOf(other);

    new bool Overlaps(IEnumerable<T> other)
        => ((ISet<T>)this).Overlaps(other);

    new bool SetEquals(IEnumerable<T> other)
        => ((ISet<T>)this).SetEquals(other);
}

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


public class SourceSet<T, TKey>(Func<T, TKey> getKey) : SourceSet<T>, ISourceSet<T, TKey>, ISourceKeySet
{
    private readonly ExtendedDictionary<TKey, T> lookup = [];

    Type IObservableKeySet.KeyType => typeof(TKey);

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

    bool IObservableKeySet.ContainsKey(object? key)
        => this.ContainsKey((TKey)key!);

    object? IObservableKeySet.GetKey(object? item)
        => getKey((T)item!);

    bool IObservableKeySet.TryLookupKey(object? key, out object? item)
    {
        if (TryLookupKey((TKey)key!, out T? lookupItem))
        {
            item = lookupItem;
            return true;
        }
        else
        {
            item = default;
            return false;
        }
    }
}