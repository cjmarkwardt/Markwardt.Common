namespace Markwardt;

public interface ISourceDictionary : ISourceCollection, IObservableDictionary
{
    void SetKey(object? key, object? value);
    void RemoveKey(object? key);
}

public interface ISourceDictionary<T, TKey> : ISourceCollection<KeyValuePair<TKey, T>>, IObservableDictionary<T, TKey>, IDictionary<TKey, T>
{
    new T this[TKey key]
    {
        get => ((IDictionary<TKey, T>)this)[key];
        set => ((IDictionary<TKey, T>)this)[key] = value;
    }

    new bool ContainsKey(TKey key)
        => ((IDictionary<TKey, T>)this).ContainsKey(key);

    new bool TryGetValue(TKey key, [MaybeNullWhen(false)] out T value)
        => ((IDictionary<TKey, T>)this).TryGetValue(key, out value);
}

public class SourceDictionary<T, TKey> : SourceCollection<ExtendedDictionary<TKey, T>, KeyValuePair<TKey, T>>, ISourceDictionary<T, TKey>, ISourceDictionary
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

    Type IObservableDictionary.KeyType => typeof(TKey);
    Type IObservableDictionary.ValueType => typeof(T);

    IEnumerable<KeyValuePair<object?, object?>>? IObservableDictionary.Items => this.Select(x => new KeyValuePair<object?, object?>(x.Key, x.Value));
    IObservable<IEnumerable<ItemChange<KeyValuePair<object?, object?>>>> IObservableDictionary.Changes => Changes.Select(x => x.Select(change => change.Convert(y => new KeyValuePair<object?, object?>(y.Key, y.Value))));

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
    void ISourceDictionary.SetKey(object? key, object? value)
        => this[(TKey)key!] = (T)value!;

    void ISourceDictionary.RemoveKey(object? key)
        => Remove((TKey)key!);

    bool IObservableDictionary.ContainsKey(object? key)
        => ContainsKey((TKey)key!);

    bool IObservableDictionary.TryLookupKey(object? key, out object? value)
    {
        if (TryGetValue((TKey)key!, out T? lookupValue))
        {
            value = lookupValue;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
}