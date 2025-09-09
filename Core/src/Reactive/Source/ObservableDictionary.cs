namespace Markwardt;

public interface IObservableDictionary<T, TKey> : IObservableCollection<KeyValuePair<TKey, T>, TKey>, IReadOnlyDictionary<TKey, T>;