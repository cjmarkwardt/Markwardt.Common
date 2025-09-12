namespace Markwardt;

public interface ISourceList : ISourceCollection, IObservableList
{
    void SetAt(int index, object? item);
    void InsertAt(int index, object? item);
    void RemoveAt(int index);
}

public interface ISourceList<T> : ISourceCollection<T>, IObservableList<T>, IList<T>
{
    new T this[int index]
    {
        get => ((IList<T>)this)[index];
        set => ((IList<T>)this)[index] = value;
    }
}

public class SourceList<T> : SourceCollection<List<T>, T>, ISourceList<T>, ISourceList
{
    public T this[int index]
    {
        get => Items[index];
        set
        {
            T oldValue = Items[index];
            Items[index] = value;
            CommitRemove(oldValue);
            CommitAdd(value);
        }
    }

    public int IndexOf(T item)
        => Items.IndexOf(item);

    public void Insert(int index, T item)
    {
        Items.Insert(index, item);
        CommitAdd(item);
    }

    public void RemoveAt(int index)
    {
        T item = Items[index];
        Items.RemoveAt(index);
        CommitRemove(item);
    }

    object? IObservableList.GetAt(int index)
        => this[index];

    void ISourceList.InsertAt(int index, object? item)
        => Insert(index, (T)item!);

    void ISourceList.SetAt(int index, object? item)
        => this[index] = (T)item!;
}