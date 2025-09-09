namespace Markwardt;

public interface IObservableSet<T> : IObservableCollection<T>, IReadOnlySet<T>;

public interface IObservableSet<T, TKey> : IObservableCollection<T, TKey>, IObservableSet<T>;