namespace Markwardt;

public class DataDictionary<TKey, TValue> : DataCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
{
    private readonly ExtendedDictionary<TKey, TValue> dictionary = [];

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set
        {
            TValue oldValue = dictionary[key];
            if (!oldValue.ValueEquals(value))
            {
                dictionary[key] = value;
                PushRemove(key, oldValue);
                PushAdd(key, value);
            }
        }
    }

    public ICollection<TKey> Keys => dictionary.Keys;
    public ICollection<TValue> Values => dictionary.Values;

    protected override ICollection<KeyValuePair<TKey, TValue>> Collection => dictionary;

    public override IEnumerable<KeyValuePair<object?, object?>> Items => dictionary.Select(x => new KeyValuePair<object?, object?>(x.Key, x.Value));

    public override Type? KeyType => typeof(TKey);
    public override Type ItemType => typeof(TValue);

    public void Add(TKey key, TValue value)
    {
        dictionary.Add(key, value);
        PushAdd(key, value);
    }

    public bool ContainsKey(TKey key)
        => dictionary.ContainsKey(key);

    public bool Remove(TKey key)
    {
        if (TryGetValue(key, out TValue? value))
        {
            dictionary.Remove(key);
            PushRemove(key, value);
            return true;
        }

        return false;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        => dictionary.TryGetValue(key, out value);

    public override void Inject(object? key, object? item)
        => dictionary.Add((TKey)key!, (TValue)item!);

    protected override bool ExecuteAdd(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
        return true;
    }

    private void PushAdd(TKey key, TValue value)
        => PushAdd(new KeyValuePair<TKey, TValue>(key, value));

    private void PushRemove(TKey key, TValue value)
        => PushRemove(new KeyValuePair<TKey, TValue>(key, value));
}