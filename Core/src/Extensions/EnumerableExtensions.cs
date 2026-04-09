namespace Markwardt;

public static class EnumerableExtensions
{
    public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> enumerable)
        => enumerable.Select((x, i) => (x, i));

    public static IEnumerable<(T Item, bool IsFirst, bool IsLast)> WithFirstLast<T>(this IEnumerable<T> enumerable)
    {
        bool isFirst = true;
        Maybe<T> last = default;
        foreach (T value in enumerable)
        {
            if (last.HasValue)
            {
                yield return (last.Value, isFirst, false);
                isFirst = false;
            }

            last = value.Maybe();
        }

        if (last.HasValue)
        {
            yield return (last.Value, isFirst, true);
        }
    }
        
    public static Maybe<T> MaybeFirst<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        foreach (T item in enumerable)
        {
            if (predicate(item))
            {
                return item.Maybe();
            }
        }

        return default;
    }

    public static Maybe<T> MaybeFirst<T>(this IEnumerable<T> enumerable)
        => enumerable.MaybeFirst(_ => true);

    public static IEnumerable<(T Key, TSelected Item)> SelectWhere<T, TSelected>(this IEnumerable<T> enumerable, Func<T, Maybe<TSelected>> select)
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

    public static IEnumerable<(T Key, TSelected Item)> SelectWhere<T, TSelected>(this IEnumerable<T> enumerable, Func<T, TSelected?> select)
        => enumerable.SelectWhere(x =>
        {
            TSelected? item = select(x);
            return item is not null ? item.Maybe() : default;
        });

    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    public static void ForEach<T, TState>(this IEnumerable<T> enumerable, TState state, Action<T, int, Flag, TState> action, bool reverse = false)
    {
        if (enumerable is IReadOnlyList<T> list)
        {
            Indexer<T> indexer = list.NewIndexer();
            indexer.For(state, reverse, action);
            indexer.Recycle();
        }
        else
        {
            int index = reverse ? enumerable.Count() - 1 : 0;
            int step = reverse ? -1 : 1;

            Flag flag = Flag.New();

            foreach (T item in reverse ? enumerable.Reverse() : enumerable)
            {
                action(item, index, flag, state);
                index += step;

                if (flag.IsSet)
                {
                    break;
                }
            }

            flag.Recycle();
        }
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int, Flag> action, bool reverse = false)
        => enumerable.ForEach<T, object?>(null, (x, index, flag, _) => action(x, index, flag), reverse);

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action, bool reverse = false)
        => enumerable.ForEach<T, object?>(null, (x, index, _, _) => action(x, index), reverse);

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action, bool reverse = false)
        => enumerable.ForEach<T, object?>(null, (x, _, _, _) => action(x), reverse);

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