namespace Markwardt;



public interface IItems<T> : ISet<T>
{
    void AddMany(IEnumerable<T> items);
    void RemoveMany(IEnumerable<T> items);
    void RemoveWhere(Func<T, bool> predicate);
}

public interface IOrderedItems<T> : IItems<T>, IList<T>
{
    void InsertMany(int index, IEnumerable<T> items);
    void RemoveRange(System.Range range);
    void Swap(int index1, int index2, int count = 1);
    void Shift(int index, int shift, int count = 1);
    IEnumerable<T> Dequeue(int count);
    IEnumerable<T> Pop(int count);
}

public interface IReadOnlyItems<T> : IReadOnlySet<T>, IReadOnlyDeque<T>;

public interface IReadOnlyOrderedItems<T> : IReadOnlyItems<T>, IReadOnlyList<T>;

public class ReadOnlyItems<T>(IEnumerable<T> target) : IReadOnlyItems<T>
{
    protected virtual IEnumerable<T> Target => target;

    public T this[int index] => Target is IReadOnlyList<T> list ? list[index] : Target.ElementAt(index);
    public int Count => Target is IReadOnlyCollection<T> collection ? collection.Count : Target.Count();

    public bool Contains(T item)
        => Target is IReadOnlySet<T> set ? set.Contains(item) : Target.Contains(item);

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        if (Target is IReadOnlySet<T> set)
        {
            return set.IsProperSubsetOf(other);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        if (Target is IReadOnlySet<T> set)
        {
            return set.IsProperSupersetOf(other);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        if (Target is IReadOnlySet<T> set)
        {
            return set.IsSubsetOf(other);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        if (Target is IReadOnlySet<T> set)
        {
            return set.IsSupersetOf(other);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        if (Target is IReadOnlySet<T> set)
        {
            return set.Overlaps(other);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        if (Target is IReadOnlySet<T> set)
        {
            return set.SetEquals(other);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public IEnumerable<T> PeekDequeue(int count)
    {
        if (Target is IReadOnlyDeque<T> deque)
        {
            return deque.PeekDequeue(count);
        }
        else
        {
            return Target.Take(count);
        }
    }

    public IEnumerable<T> PeekPop(int count)
    {
        if (Target is IReadOnlyDeque<T> deque)
        {
            return deque.PeekPop(count);
        }
        else if (Target is IReadOnlyList<T> list)
        {
            return Enumerable.Range(list.Count - count, count).Reverse().Select(x => list[x]);
        }
        else
        {
            return Target.Reverse().Take(count);
        }
    }

    public IEnumerator<T> GetEnumerator()
        => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}