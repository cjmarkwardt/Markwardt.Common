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

    public static IEnumerable<T> AppendAt<T>(this IEnumerable<T> collection, T target, int index)
    {
        int i = 0;
        foreach (T item in collection)
        {
            if (index == i)
            {
                yield return target;
            }
            
            yield return item;

            i++;
        }
    }

    public static IEnumerable<T> WithoutFirst<T>(this IEnumerable<T> collection, T target)
    {
        bool removed = false;
        foreach (T item in collection)
        {
            if (item.ValueEquals(target) && removed)
            {
                if (removed)
                {
                    continue;
                }
                else
                {
                    removed = true;
                }
            }

            yield return item;
        }
    }

    public static IEnumerable<T> WithoutIndex<T>(this IEnumerable<T> collection, int index)
    {
        int i = 0;
        foreach (T item in collection)
        {
            if (i != index)
            {
                yield return item;
            }

            i++;
        }
    }
}