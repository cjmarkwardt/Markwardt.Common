namespace Markwardt;

public class DataList<T> : DataCollection<T>, IList<T>
{
    private readonly List<T> list = [];

    public T this[int index]
    {
        get => list[index];
        set
        {
            T oldValue = list[index];
            if (!oldValue.ValueEquals(value))
            {
                list[index] = value;
                PushRemove(oldValue);
                PushAdd(value);
            }
        }
    }

    protected override ICollection<T> Collection => list;

    public int IndexOf(T item)
        => list.IndexOf(item);

    public void Insert(int index, T item)
    {
        list.Insert(index, item);
        PushAdd(item);
    }

    public void RemoveAt(int index)
    {
        T value = list[index];
        list.RemoveAt(index);
        PushRemove(value);
    }

    public override void Inject(object? key, object? item)
        => list.Add((T)item!);

    protected override bool ExecuteAdd(T item)
    {
        list.Add(item);
        return true;
    }
}