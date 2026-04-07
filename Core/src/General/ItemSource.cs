namespace Markwardt;

public interface IItemSource<T>
{
    IReadOnlyList<T> Items { get; }
}