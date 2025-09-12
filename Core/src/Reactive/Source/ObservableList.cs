namespace Markwardt;

public interface IObservableList : IObservableCollection
{
    object? GetAt(int index);
}

public interface IObservableList<T> : IObservableCollection<T>, IReadOnlyList<T>;