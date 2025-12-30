namespace Markwardt;

public static class CollectionExtensions
{
    public static void ForEachChange<T>(this IReadOnlySet<T> current, IEnumerable<T> next, Func<T, bool> nextContains, Action<T> onAdded, Action<T> onRemoved)
    {
        foreach (T item in current)
        {
            if (!nextContains(item))
            {
                onRemoved(item);
            }
        }

        foreach (T item in next)
        {
            if (!current.Contains(item))
            {
                onAdded(item);
            }
        }
    }

    public static void ForEachChange<T, TValue>(this IReadOnlyDictionary<T, TValue> current, IEnumerable<T> next, Func<T, bool> nextContains, Action<T> onAdded, Action<T, TValue> onRemoved)
    {
        foreach ((T item, TValue value) in current)
        {
            if (!nextContains(item))
            {
                onRemoved(item, value);
            }
        }

        foreach (T item in next)
        {
            if (!current.ContainsKey(item))
            {
                onAdded(item);
            }
        }
    }
    
    public static void ForEachChange<T, TValue>(this IReadOnlyDictionary<T, TValue> current, IEnumerable<T> next, Func<T, bool> nextContains, Action<T, TValue> onAdded, Action<T, TValue> onRemoved)
        => current.ForEachChange(next, nextContains, item => onAdded(item, current[item]), onRemoved);

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