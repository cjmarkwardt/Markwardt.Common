namespace Markwardt;

public interface ISourceDictionary<T, TKey> : IObservableDictionary<T, TKey>, IDictionary<TKey, T>;

public class SourceDictionary<T, TKey> : SourceCollection<ExtendedDictionary<TKey, T>, KeyValuePair<TKey, T>>, ISourceDictionary<T, TKey>, ISourceCollection.IPairAccessor
{
    public T this[TKey key]
    {
        get => Items[key];
        set
        {
            T oldValue = Items[key];
            Items[key] = value;
            CommitRemove(new(key, oldValue));
            CommitAdd(new(key, value));
        }
    }

    public IEnumerable<TKey> Keys => Items.Keys;
    public IEnumerable<T> Values => Items.Values;

    ICollection<TKey> IDictionary<TKey, T>.Keys => Items.Keys;
    ICollection<T> IDictionary<TKey, T>.Values => Items.Values;

    Type IObservableCollection.IPairAccessor.ItemType => typeof(TKey);
    Type IObservableCollection.IPairAccessor.KeyType => typeof(T);

    IEnumerable<KeyValuePair<object?, object?>>? IObservableCollection.IPairAccessor.Items => this.Select(x => new KeyValuePair<object?, object?>(x.Key, x.Value));
    IObservable<IEnumerable<ItemChange<KeyValuePair<object?, object?>>>> IObservableCollection.IPairAccessor.Changes => Changes.Select(x => x.Select(change => change.Convert(y => new KeyValuePair<object?, object?>(y.Key, y.Value))));

    public bool ContainsKey(TKey key)
        => Items.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out T value)
        => Items.TryGetValue(key, out value);

    public bool TryLookupKey(TKey key, [MaybeNullWhen(false)] out KeyValuePair<TKey, T> value)
    {
        if (Items.TryGetValue(key, out T? pairValue))
        {
            value = new(key, pairValue);
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public void Add(TKey key, T value)
    {
        Items.Add(key, value);
        CommitAdd(new(key, value));
    }

    public bool Remove(TKey key)
    {
        if (Items.TryGetValue(key, out T? value))
        {
            Items.Remove(key);
            CommitRemove(new(key, value));
            return true;
        }
        else
        {
            return false;
        }
    }

    void ISourceCollection.IPairAccessor.AddPair(object? key, object? item)
        => Add((TKey)key!, (T)item!);

    void ISourceCollection.IPairAccessor.RemoveKey(object? key)
        => Remove((TKey)key!);

    bool IObservableCollection.IPairAccessor.ContainsKey(object? key)
        => ContainsKey((TKey)key!);
}