using System.Collections.Specialized;

namespace Markwardt;

public interface IObservableCollection : INotifyCollectionChanged
{
    IAccessor Accessor { get; }

    IDisposable StartEdit();

    interface IAccessor
    {
        int Count { get; }
        Type ItemType { get; }
        IEnumerable<object?> Items { get; }
        IObservable<IEnumerable<ItemChange<object?>>> Changes { get; }

        bool Contains(object? item);
    }

    interface IKeyedAccessor : IAccessor
    {
        Type KeyType { get; }
        new IEnumerable<KeyValuePair<object?, object?>>? Items { get; }
        new IObservable<IEnumerable<ItemChange<KeyValuePair<object?, object?>>>> Changes { get; }

        bool ContainsKey(object? key);
    }
}

public interface IObservableCollection<T> : IObservableCollection, IReadOnlyCollection<T>
{
    IObservable<IEnumerable<ItemChange<T>>> Changes { get; }
}

public interface IObservableCollection<T, TKey> : IObservableCollection<T>, IReadOnlyKeyLookup<T, TKey>;

public abstract class ObservableCollection<T> : IObservableCollection<T>, IObservableCollection.IAccessor
{
    public ObservableCollection()
        => editor.Changes.Subscribe(x => HandleCollectionChanged(x, (action, items) => CollectionChanged?.Invoke(this, new(action, items))));

    private readonly ItemChangeEditor<T> editor = new();

    protected abstract ICollection<T> Items { get; }

    Type IObservableCollection.IAccessor.ItemType => typeof(T);
    IEnumerable<object?> IObservableCollection.IAccessor.Items => Items.Cast<object?>();
    IObservable<IEnumerable<ItemChange<object?>>> IObservableCollection.IAccessor.Changes => Changes.Select(x => x.Select(y => y.Cast<object?>()));

    public IObservable<IEnumerable<ItemChange<T>>> Changes => editor.Changes;
    public IObservableCollection.IAccessor Accessor => this;
    public int Count => Items.Count;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public IEnumerator<T> GetEnumerator()
        => Items.GetEnumerator();

    public IDisposable StartEdit()
        => editor.StartEdit();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    bool IObservableCollection.IAccessor.Contains(object? item)
        => Contains((T)item!);

    protected virtual bool Contains(T item)
        => Items.Contains(item);

    private void HandleCollectionChanged(IEnumerable<ItemChange<T>> changes, Action<NotifyCollectionChangedAction, IList> invoke)
    {
        ItemChangeKind? kind = null;
        List<T>? pending = null;

        void Commit()
        {
            if (kind is null)
            {
                throw new InvalidOperationException();
            }

            if (pending is not null)
            {
                invoke(kind == ItemChangeKind.Add ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove, pending);
                pending = null;
            }
        }

        foreach (ItemChange<T> change in changes)
        {
            if (change.Kind != kind)
            {
                Commit();
                kind = change.Kind;
                pending = [];
            }

            switch (change.Kind)
            {
                case ItemChangeKind.Add:
                    pending?.Add(change.Item);
                    break;
                case ItemChangeKind.Remove:
                    pending?.Add(change.Item);
                    break;
                default:
                    throw new NotSupportedException(change.Kind.ToString());
            }
        }

        Commit();
    }

    private sealed class ItemChangeEditor<T2>
    {
        private readonly Subject<IEnumerable<ItemChange<T2>>> changes = new();

        private int level;
        private List<ItemChange<T2>> pendingChanges = [];

        public IObservable<IEnumerable<ItemChange<T2>>> Changes => changes;

        public IDisposable StartEdit()
        {
            level++;
            return new Edit(this);
        }

        public void Add(T2 item)
            => pendingChanges.Add(new(ItemChangeKind.Add, item));

        public void Remove(T2 item)
            => pendingChanges.Add(new(ItemChangeKind.Remove, item));

        public void Commit()
        {
            if (pendingChanges.Count > 0)
            {
                IEnumerable<ItemChange<T2>> cache = pendingChanges;
                pendingChanges = [];
                changes.OnNext(cache);
            }
        }

        private void EndEdit()
        {
            level--;

            if (level == 0 && pendingChanges.Count > 0)
            {
                IEnumerable<ItemChange<T2>> cache = pendingChanges;
                pendingChanges = [];
                changes.OnNext(cache);
            }
        }

        private sealed class Edit(ItemChangeEditor<T2> editor) : IDisposable
        {
            private bool isDisposed;

            public void Dispose()
            {
                if (!isDisposed)
                {
                    editor.EndEdit();
                    isDisposed = true;
                }
            }
        }
    }
}

public interface IObservableSet<T> : IObservableCollection<T>, IReadOnlySet<T>;

public interface IObservableSet<T, TKey> : IObservableCollection<T, TKey>, IObservableSet<T>;

public interface IObservableDictionary<T, TKey> : IObservableCollection<KeyValuePair<TKey, T>, TKey>, IReadOnlyDictionary<TKey, T>;

public interface ISourceDictionary<T, TKey> : IObservableDictionary<T, TKey>, IDictionary<TKey, T>;