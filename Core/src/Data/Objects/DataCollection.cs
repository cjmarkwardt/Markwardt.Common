namespace Markwardt;

public interface IDataCollection
{
    Type? KeyType { get; }
    Type ItemType { get; }
    int Count { get; }
    IEnumerable<KeyValuePair<object?, object?>> Items { get; }

    void Inject(object? key, object? item);
    IObservable<CollectionChange> Watch();
}

public record DataCollectionType(Type Type, Type ImplementationType)
{
    public static DataCollectionType? Scan(Type type)
    {
        Type? implementationType = null;
        if (type.IsGenericType)
        {
            Type[] argumentTypes = type.GetGenericArguments();
            Type definitionType = type.GetGenericTypeDefinition();
            if (definitionType == typeof(IList<>) || definitionType == typeof(ICollection<>))
            {
                implementationType = typeof(DataList<>).MakeGenericType(argumentTypes);
            }
            else if (definitionType == typeof(ISet<>))
            {
                implementationType = typeof(DataSet<>).MakeGenericType(argumentTypes);
            }
            else if (definitionType == typeof(IDictionary<,>))
            {
                implementationType = typeof(DataDictionary<,>).MakeGenericType(argumentTypes);
            }
        }

        return implementationType is null ? null : new DataCollectionType(type, implementationType);
    }

    public IDataCollection Create()
        => (IDataCollection)Activator.CreateInstance(ImplementationType).NotNull();
}

public abstract class DataCollection<T> : IDataCollection, ICollection<T>
{
    protected abstract ICollection<T> Collection { get; }

    bool ICollection<T>.IsReadOnly => false;

    private readonly Subject<CollectionChange> changes = new();

    public virtual Type? KeyType => null;
    public virtual Type ItemType => typeof(T);

    public int Count => Collection.Count;
    public virtual IEnumerable<KeyValuePair<object?, object?>> Items => Collection.Select(x => new KeyValuePair<object?, object?>(null, x));

    public IObservable<CollectionChange> Watch()
        => changes;

    public bool Add(T item)
    {
        if (ExecuteAdd(item))
        {
            PushAdd(item);
            return true;
        }

        return false;
    }

    public bool Remove(T item)
    {
        if (Collection.Remove(item))
        {
            PushRemove(item);
            return true;
        }

        return false;
    }

    public void Clear()
    {
        if (Count > 0)
        {
            List<T> items = Collection.ToList();
            Collection.Clear();
            PushRemove(items);
        }
    }

    public bool Contains(T item)
        => Collection.Contains(item);

    public void CopyTo(T[] array, int arrayIndex)
        => Collection.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator()
        => Collection.GetEnumerator();

    public abstract void Inject(object? key, object? item);

    protected abstract bool ExecuteAdd(T item);

    protected void PushAdd(params IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            changes.OnNext(new(ItemChangeKind.Add, item));
        }
    }

    protected void PushRemove(params IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            changes.OnNext(new(ItemChangeKind.Remove, item));
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    void ICollection<T>.Add(T item)
        => Add(item);
}