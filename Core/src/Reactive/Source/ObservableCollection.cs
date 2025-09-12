using System.Collections.Specialized;

namespace Markwardt;

public interface IObservableCollection
{
    int Count { get; }
    Type ItemType { get; }
    IEnumerable<object?> Items { get; }
    IObservable<IEnumerable<ItemChange<object?>>> Changes { get; }

    bool Contains(object? item);
}

public interface IObservableCollection<T> : IReadOnlyCollection<T>, IObservableItems<T>;

public interface IObservableCollection<T, TKey> : IObservableCollection<T>, IReadOnlyKeyLookup<T, TKey>;

public interface IObservableItems<T> : IEnumerable<T>
{
    IObservable<IEnumerable<ItemChange<T>>> Changes { get; }
}

public class ObservableItems<T>(IEnumerable<T> items, IObservable<IEnumerable<ItemChange<T>>> changes) : IObservableItems<T>
{
    public IObservable<IEnumerable<ItemChange<T>>> Changes => changes;

    public IEnumerator<T> GetEnumerator()
        => items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}

public static class ObservableCollectionExtensions
{
    public static IObservableItems<TSelect> SelectItems<T, TSelect>(this IObservableItems<T> source, Func<T, TSelect> selector)
        => new ObservableItems<TSelect>(source.Select(selector), source.Changes.Select(x => x.Select(change => change.Convert(y => selector(y)))));

    public static IObservableItems<TSelect> SelectWhereItems<T, TSelect>(this IObservableItems<T> source, Func<T, Maybe<TSelect>> selector)
        => new ObservableItems<TSelect>(source.Select(selector).Where(x => x.HasValue).Select(x => x.Value), source.Changes.Select(x => x.Select(change => change.Convert(y => selector(y)))).Select(x => x.Where(y => y.Item.HasValue).Select(y => y.Convert(z => z.Value))).Where(x => x.Any()));

    public static IObservableItems<T> WhereItems<T>(this IObservableItems<T> source, Func<T, bool> predicate)
        => new ObservableItems<T>(source.Where(predicate), source.Changes.Select(x => x.Where(change => predicate(change.Item))).Where(x => x.Any()));

    public static IObservable<T> WhereItemsAdded<T>(this IObservableItems<T> source)
        => source.Changes.SelectMany(x => x.Where(change => change.Kind is ItemChangeKind.Add).Select(change => change.Item));

    public static IObservable<T> WhereItemsRemoved<T>(this IObservableItems<T> source)
        => source.Changes.SelectMany(x => x.Where(change => change.Kind is ItemChangeKind.Remove).Select(change => change.Item));

    public static ISourceList<T> ToSourceList<T>(this IEnumerable<T> items, IObservable<IEnumerable<ItemChange<T>>>? source = null)
    {
        SourceList<T> list = [];
        items.ForEach(list.Add);

        if (source is not null)
        {
            list.Attach(source);
        }

        return list;
    }

    public static ISourceList<T> ObserveAsList<T>(this IObservableItems<T> source)
        => source.ToSourceList(source.Changes);

    public static ISourceSet<T> ToSourceSet<T>(this IEnumerable<T> items, IObservable<IEnumerable<ItemChange<T>>>? source = null)
    {
        SourceSet<T> set = [];
        items.ForEach(x => set.Add(x));

        if (source is not null)
        {
            set.Attach(source);
        }

        return set;
    }

    public static ISourceSet<T> ObserveAsSet<T>(this IObservableItems<T> source)
        => source.ToSourceSet(source.Changes);

    public static ISourceSet<T, TKey> ToSourceSet<T, TKey>(this IEnumerable<T> items, Func<T, TKey> getKey, IObservable<IEnumerable<ItemChange<T>>>? source = null)
    {
        SourceSet<T, TKey> set = new(getKey);
        items.ForEach(x => set.Add(x));

        if (source is not null)
        {
            set.Attach(source);
        }

        return set;
    }

    public static ISourceSet<T, TKey> ObserveAsSet<T, TKey>(this IObservableItems<T> source, Func<T, TKey> getKey)
        => source.ToSourceSet(getKey, source.Changes);

    public static ISourceDictionary<T, TKey> ToSourceDictionary<T, TKey>(this IEnumerable<KeyValuePair<TKey, T>> items, IObservable<IEnumerable<ItemChange<KeyValuePair<TKey, T>>>>? source = null)
    {
        SourceDictionary<T, TKey> dictionary = [];
        items.ForEach(dictionary.Add);

        if (source is not null)
        {
            dictionary.Attach(source);
        }

        return dictionary;
    }

    public static ISourceDictionary<T, TKey> ObserveAsDictionary<T, TKey>(this IObservableItems<KeyValuePair<TKey, T>> source)
        => source.ToSourceDictionary(source.Changes);
}

public abstract class ObservableCollection<T> : IObservableCollection<T>, IObservableCollection, INotifyCollectionChanged
{
    event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
    {
        add => editor.CollectionChanged += value;
        remove => editor.CollectionChanged -= value;
    }

    private readonly ItemChangeEditor editor = new();

    protected abstract IReadOnlyCollection<T> Collection { get; }

    Type IObservableCollection.ItemType => typeof(T);
    IEnumerable<object?> IObservableCollection.Items => Collection.Cast<object?>();
    IObservable<IEnumerable<ItemChange<object?>>> IObservableCollection.Changes => Changes.Select(x => x.Select(y => y.Cast<object?>()));

    public IObservable<IEnumerable<ItemChange<T>>> Changes => editor.Changes;
    public int Count => Collection.Count;

    public IEnumerator<T> GetEnumerator()
        => Collection.GetEnumerator();

    public IDisposable StartEdit()
        => editor.StartEdit();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    bool IObservableCollection.Contains(object? item)
        => Contains((T)item!);

    protected virtual bool Contains(T item)
        => Collection.Contains(item);

    protected virtual void OnCommitAdd(T item) { }
    protected virtual void OnCommitRemove(T item) { }

    protected void CommitAdd(T item)
    {
        OnCommitAdd(item);
        editor.Commit(ItemChangeKind.Add, item);
    }

    protected void CommitRemove(T item)
    {
        OnCommitRemove(item);
        editor.Commit(ItemChangeKind.Remove, item);
    }

    private void HandleCollectionChanged(IEnumerable<ItemChange<T>> changes, Action<NotifyCollectionChangedAction, IList> invoke)
    {
        ItemChangeKind? kind = null;
        List<T>? pending = null;

        void TryTriggerEvent()
        {
            if (kind is not null && pending is not null)
            {
                invoke(kind == ItemChangeKind.Add ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove, pending);
                pending = null;
            }
        }

        foreach (ItemChange<T> change in changes)
        {
            if (change.Kind != kind)
            {
                TryTriggerEvent();
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

        TryTriggerEvent();
    }

    private sealed class ItemChangeEditor : INotifyCollectionChanged
    {
        private readonly Subject<IEnumerable<ItemChange<T>>> subject = new();

        private int level;
        private List<ItemChange<T>> pendingChanges = [];

        private bool IsSubscribed => subject.HasObservers || CollectionChanged is not null;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public IObservable<IEnumerable<ItemChange<T>>> Changes => subject;

        public IDisposable StartEdit()
        {
            level++;
            return new Edit(this);
        }

        public void Commit(ItemChangeKind kind, T item)
        {
            ItemChange<T> change = new(kind, item);
            if (level == 0)
            {
                if (IsSubscribed)
                {
                    Push(change.Yield());
                }
            }
            else
            {
                pendingChanges.Add(change);
            }
        }

        private void EndEdit()
        {
            level--;

            if (level == 0 && pendingChanges.Count > 0)
            {
                if (IsSubscribed)
                {
                    IEnumerable<ItemChange<T>> cachedChanges = pendingChanges;
                    pendingChanges = [];
                    Push(cachedChanges);
                }
                else
                {
                    pendingChanges.Clear();
                }
            }
        }

        private void Push(IEnumerable<ItemChange<T>> changes)
        {
            subject.OnNext(changes);

            ItemChangeKind? kind = null;
            List<T>? pending = null;

            void TryTriggerEvent()
            {
                if (kind is not null && pending is not null)
                {
                    CollectionChanged?.Invoke(null, new(kind is ItemChangeKind.Add ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove, pending));
                    pending = null;
                }
            }

            foreach (ItemChange<T> change in changes)
            {
                if (change.Kind != kind)
                {
                    TryTriggerEvent();
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

            TryTriggerEvent();
        }

        private sealed class Edit(ItemChangeEditor editor) : IDisposable
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