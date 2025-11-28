namespace Markwardt;

public static class EnumerableExtensions
{
    public static IEnumerable<(T, TSelected)> SelectWhere<T, TSelected>(this IEnumerable<T> enumerable, Func<T, Maybe<TSelected>> select)
    {
        foreach (T item in enumerable)
        {
            Maybe<TSelected> trySelect = select(item);
            if (trySelect.HasValue)
            {
                yield return (item, trySelect.Value);
            }
        }
    }

    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (T item in enumerable)
        {
            action(item);
        }
    }

    public static int IndexOf<T>(this IEnumerable<T> enumerable, T value)
    {
        int i = 0;
        foreach (T item in enumerable)
        {
            if (item.ValueEquals(value))
            {
                return i;
            }

            i++;
        }

        return -1;
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> getValue)
    {
        if (!dictionary.TryGetValue(key, out TValue? value))
        {
            value = getValue();
            dictionary.Add(key, value);
        }

        return value;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : class
        => enumerable.Where(x => x is not null).Select(x => x!);

    public static ReadOnlyMemory<T> AsReadOnlyMemory<T>(this IEnumerable<T> enumerable)
        => enumerable is T[] array ? array : enumerable.ToArray();

    public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        => enumerable is IReadOnlyCollection<T> collection ? collection : enumerable.ToList();

    public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> enumerable)
        => enumerable is IReadOnlyList<T> list ? list : enumerable.ToList();

    public static IReadOnlySet<T> AsReadOnlySet<T>(this IEnumerable<T> enumerable)
        => enumerable is IReadOnlySet<T> set ? set : enumerable.ToHashSet();

    public static IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        where TKey : notnull
        => enumerable is IReadOnlyDictionary<TKey, TValue> dictionary ? dictionary : enumerable.ToDictionary(x => x.Key, x => x.Value);
}