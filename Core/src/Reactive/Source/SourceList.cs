namespace Markwardt;

public interface ISourceList<T> : ISourceCollection<T>, IObservableList<T>, IList<T>;

public class SourceList<T> : SourceCollection<List<T>, T>, ISourceList<T>
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
}