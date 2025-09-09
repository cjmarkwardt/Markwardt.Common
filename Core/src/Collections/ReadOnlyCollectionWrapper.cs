namespace Markwardt;

public class ReadOnlyCollectionWrapper<T>(Func<ICollection<T>> getCollection) : IReadOnlyCollection<T>
{
    public int Count => getCollection().Count;

    public IEnumerator<T> GetEnumerator()
        => getCollection().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}