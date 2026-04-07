namespace Markwardt;

public interface IIndexer<T> : IRecyclable
{
    int Count { get; }

    T Get(int index);
}

public static class IndexerExtensions
{
    public static Indexer<T> NewIndexer<T>(this IReadOnlyList<T> list)
        => Indexer<T>.New(() => list.Count, index => list[index]);

    public static void For<T, TState>(this IIndexer<T> indexer, TState state, bool reverse, Action<T, int, Flag, TState> action)
    {
        Flag flag = Flag.New();

        int start = reverse ? indexer.Count - 1 : 0;
        int end = reverse ? -1 : indexer.Count;
        int step = reverse ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            action(indexer.Get(i), i, flag, state);

            if (flag.IsSet)
            {
                break;
            }
        }

        flag.Recycle();
    }
}

public class Indexer<T> : IIndexer<T>
{
    private readonly static Pool<Indexer<T>> pool = new(() => new());

    public static Indexer<T> New(Func<int> getCount, Func<int, T> get)
    {
        Indexer<T> indexer = pool.Get();
        indexer.getCount = getCount;
        indexer.get = get;
        return indexer;
    }

    private Indexer() { }

    private Func<int> getCount = default!;
    private Func<int, T> get = default!;

    public int Count => getCount();

    public T Get(int index)
        => get(index);

    public void Recycle()
    {
        getCount = default!;
        get = default!;
        pool.Recycle(this);
    }
}