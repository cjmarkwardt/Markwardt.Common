namespace Markwardt;

public static class CollectionExtensions
{
    public static void Add<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            collection.Add(item);
        }
    }

    public static void Remove<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            collection.Remove(item);
        }
    }
}