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

    void Do()
    {
        ISourceDictionary<string, string> f = null!;
        if (f.TryGetValue("g", out string? value))
        {
            
        }
    }
}

public interface IObservableCollection<T> : IObservableCollection, IReadOnlyCollection<T>
{
    IObservable<IEnumerable<ItemChange<T>>> Changes { get; }
}

public interface IObservableCollection<T, TKey> : IObservableCollection<T>, IReadOnlyKeyLookup<T, TKey>;

public class ObservableCollection<T> : IObservableCollection<T>
{
    private readonly ItemChangeEditor<T> editor = new();

    public IObservable<IEnumerable<ItemChange<T>>> Changes => editor.Changes;

    public IObservableCollection.IAccessor Accessor => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public IDisposable StartEdit()
        => editor.StartEdit();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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