namespace Markwardt;

public class DataSet<T> : DataCollection<T>, ISet<T>
{
    private readonly HashSet<T> set = [];

    protected override ICollection<T> Collection => set;

    public bool IsProperSubsetOf(IEnumerable<T> other)
        => set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other)
        => set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other)
        => set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other)
        => set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other)
        => set.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other)
        => set.SetEquals(other);

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        List<T> removed = this.Intersect(other).ToList();
        List<T> added = other.Except(this).ToList();
        if (removed.Count > 0 || added.Count > 0)
        {
            set.SymmetricExceptWith(other);
            PushRemove(removed);
            PushAdd(added);
        }
    }

    public void UnionWith(IEnumerable<T> other)
    {
        List<T> added = other.Except(this).ToList();
        if (added.Count > 0)
        {
            set.UnionWith(other);
            PushAdd(added);
        }
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        List<T> removed = this.Intersect(other).ToList();
        if (removed.Count > 0)
        {
            set.ExceptWith(other);
            PushRemove(removed);
        }
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        List<T> removed = this.Except(other).ToList();
        if (removed.Count > 0)
        {
            set.IntersectWith(other);
            PushRemove(removed);
        }
    }

    public override void Inject(object? key, object? item)
        => set.Add((T)item!);

    protected override bool ExecuteAdd(T item)
        => set.Add(item);
}