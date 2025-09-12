namespace Markwardt;

public interface IObservableDictionary : IObservableCollection
{
    Type KeyType { get; }
    Type ValueType { get; }
    new IEnumerable<KeyValuePair<object?, object?>> Items { get; }
    new IObservable<IEnumerable<ItemChange<KeyValuePair<object?, object?>>>> Changes { get; }

    bool ContainsKey(object? key);
    bool TryLookupKey(object? key, [MaybeNullWhen(false)] out object? value);
}

public interface IObservableDictionary<T, TKey> : IObservableCollection<KeyValuePair<TKey, T>, TKey>, IReadOnlyDictionary<TKey, T>;