namespace Markwardt;

public interface IReactiveList<T> : ISourceList<T>, IReactiveAttachable<IEnumerable<ItemChange<T>>>;

public class ReactiveList<T> : ObservableList<T>, IReactiveList<T>
{
    T IList<T>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    bool ICollection<T>.IsReadOnly => false;

    public void Add(T item)
    {
        throw new NotImplementedException();
    }

    public void Attach(IObservable<IEnumerable<ItemChange<T>>> source)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public void Detach()
    {
        throw new NotImplementedException();
    }

    public int IndexOf(T item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, T item)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public new bool Contains(T item)
        => base.Contains(item);
}