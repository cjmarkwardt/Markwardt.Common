namespace Markwardt;

public interface IObservableKeySet : IObservableCollection
{
    Type KeyType { get; }

    object? GetKey(object? item);
    bool ContainsKey(object? key);
    bool TryLookupKey(object? key, [MaybeNullWhen(false)] out object? item);
}

public interface IObservableSet<T> : IObservableCollection<T>, IReadOnlySet<T>;

public interface IObservableSet<T, TKey> : IObservableCollection<T, TKey>, IObservableSet<T>;