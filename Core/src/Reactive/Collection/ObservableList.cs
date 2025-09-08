namespace Markwardt;

public interface IObservableList<T> : IObservableCollection<T>, IReadOnlyList<T>;

public abstract class ObservableList<T> : ObservableCollection<T>, IObservableList<T>
{
    public T this[int index] => List[index];
    
    protected List<T> List { get; } = [];

    protected override ICollection<T> Items => List;
}