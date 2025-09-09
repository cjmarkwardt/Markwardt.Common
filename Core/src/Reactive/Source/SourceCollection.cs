namespace Markwardt;

public interface ISourceCollection : IObservableCollection
{
    new IAccessor Accessor { get; }

    new interface IAccessor : IObservableCollection.IAccessor
    {
        void Add(object? item);
        void Remove(object? item);
        void Clear();
    }

    new interface IPairAccessor : IObservableCollection.IPairAccessor
    {
        void AddPair(object? key, object? item);
        void RemoveKey(object? key);
    }
}

public interface ISourceCollection<T> : ISourceCollection, IObservableCollection<T>, ICollection<T>, ISourceAttachable<IEnumerable<ItemChange<T>>>;

public abstract class SourceCollection<TCollection, T> : ObservableCollection<T>, ISourceCollection<T>, ISourceCollection.IAccessor
    where TCollection : ICollection<T>, new()
{
    public SourceCollection()
        => Collection = new ReadOnlyCollectionWrapper<T>(() => Items);

    private readonly WeakSubscriber<IEnumerable<ItemChange<T>>> subscriber = new();

    protected TCollection Items { get; private set; } = new();

    protected override IReadOnlyCollection<T> Collection { get; }

    public new ISourceCollection.IAccessor Accessor => this;

    bool ICollection<T>.IsReadOnly => false;

    public new bool Contains(T item)
        => base.Contains(item);

    public void CopyTo(T[] array, int arrayIndex)
        => this.ForEach(x => array[arrayIndex++] = x);

    public void Add(T item)
    {
        Items.Add(item);
        CommitAdd(item);
    }

    public bool Remove(T item)
    {
        bool result = Items.Remove(item);
        CommitRemove(item);
        return result;
    }

    public void Clear()
    {
        using IDisposable edit = StartEdit();
        IEnumerable<T> cache = Items;
        Items = new();
        cache.ForEach(CommitRemove);
    }

    public void Attach(IObservable<IEnumerable<ItemChange<T>>> source)
        => subscriber.Subscribe(source, changes =>
        {
            foreach (ItemChange<T> change in changes)
            {
                switch (change.Kind)
                {
                    case ItemChangeKind.Add:
                        Add(change.Item);
                        break;
                    case ItemChangeKind.Remove:
                        Remove(change.Item);
                        break;
                }
            }
        });

    public void Detach()
        => subscriber.Unsubscribe();

    void ISourceCollection.IAccessor.Add(object? item)
        => Add((T)item!);

    void ISourceCollection.IAccessor.Remove(object? item)
        => Remove((T)item!);
}